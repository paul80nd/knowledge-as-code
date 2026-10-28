#!/bin/sh
# Scores one run of the plugin trial from the result files `claude plugin eval` wrote.
#
#   score.sh [-a <adjudication.tsv>] [-k <hook stage aggregate-result.json>] \
#     <plugin stage aggregate-result.json> <prose stage aggregate-result.json> <run folder> [<excluded.tsv>]
#
# Writes sessions.tsv, aggregate.md, and each result file with every absolute path cut to its last part.
# A case listed in <excluded.tsv> is left out of sessions.tsv and every count, and aggregate.md reports how many.
# A session <adjudication.tsv> lists is counted by the verdict read by hand, and the graders' verdict is kept beside it.
# Without -k the hook arm has no sessions, and every hook field is 0 or empty. A prose result of - does the same for the
# prose arm, and each lead over an arm with no sessions is empty.
set -eu

method=6

usage="usage: score.sh [-a <adjudication.tsv>] [-k <hook stage result>] <plugin stage result> <prose stage result | ->"
usage="$usage <run folder> [<excluded.tsv>]"
adjudication=/dev/null
hook_json=""
while getopts a:k: opt; do
  case $opt in
    a) adjudication=$OPTARG ;;
    k) hook_json=$OPTARG ;;
    *) echo "$usage" >&2; exit 2 ;;
  esac
done
shift $((OPTIND - 1))

if [ "$#" -ne 3 ] && [ "$#" -ne 4 ]; then
  echo "$usage" >&2
  exit 2
fi

plugin_json=$1
prose_json=$2
if [ "$prose_json" = "-" ]; then prose_json=""; fi
out=$3
excluded=""
if [ "$#" -eq 4 ]; then
  excluded=$(cut -f 1 "$4" | sed 's|.*/||' | tr '\n' ' ')
fi

for f in "$plugin_json" $prose_json $hook_json; do
  if [ "$(jq -r '.partial' "$f")" != "false" ]; then
    echo "score.sh: $f is a partial run, which this trial does not score" >&2
    exit 1
  fi
done
for f in $prose_json $hook_json; do
  if [ "$(jq -r '.suite.ablation' "$f")" != "none" ]; then
    echo "score.sh: $f did not run under --ablation none" >&2
    exit 1
  fi
done

mkdir -p "$out/results"

# One line per session. Graders whose names share the part before `--` are alternatives, and pass where one does.
# A verdict is 1 where every grader or set of alternatives with that prefix passed, 0 where one failed, and empty
# where the case has none. A case's kind is the folder it sits in, house or control. A session's run is its place in
# its arm, so run 2 is the second batch's where batches were merged. A stage run under --ablation none takes the arm
# it is given, and the plugin stage's two arms are plugin and none.
sessions() {
  jq -r --arg only "$2" --arg excluded "$excluded" '
    def verdict(p): [.graders[]? | select(.name | startswith(p))]
      | group_by(.name | split("--")[0]) | map(any(.passed))
      | if length == 0 then "" elif all then "1" else "0" end;
    ($excluded | split(" ")) as $skip
    | .cases[] | select(.name as $n | $skip | index($n) | not) | .name as $case | (.dir | split("/") | .[-2]) as $kind | .arms | to_entries[]
    | (if $only != "" then $only elif .key == "with" then "plugin" else "none" end) as $arm
    | .value | to_entries[] | (.key + 1) as $run | .value
    | [$kind, $arm, $case, $run, (if .error == null then "0" else "1" end),
       verdict("task-"), verdict("comply-"), verdict("cite-"), verdict("fired-"),
       (.costUsd // 0), (.turns // "")]
    | @tsv
  ' "$1"
}

{
  printf 'kind\tarm\tcase\trun\terror\ttask\tcomply\tcite\tfired\tcost_usd\tturns\n'
  sessions "$plugin_json" ""
  if [ -n "$prose_json" ]; then sessions "$prose_json" prose; fi
  if [ -n "$hook_json" ]; then sessions "$hook_json" hook; fi
} > "$out/graded.tsv"

# Appends each session's adjudicated verdict, which is empty where nobody read the session by hand. A row whose graders
# column disagrees with the verdict here, or that names no session, means the file belongs to another run.
awk -F '\t' -v OFS='\t' -v adjudication="$adjudication" '
  function verdict(task, comply) { return task == "0" ? "no task" : (comply == "1" ? "pass" : "fail") }
  FILENAME == adjudication {
    if (FNR > 1) { key = $1 FS $2 FS $3; graders[key] = $4; adjudicated[key] = $5 }
    next
  }
  FNR == 1 { print $0, "adjudicated"; next }
  {
    key = $3 FS $2 FS $4
    if (key in graders) {
      if (graders[key] != verdict($6, $7)) {
        given = verdict($6, $7)
        print "score.sh: the graders gave " $3 " " $2 " run " $4 " " given ", not " graders[key] > "/dev/stderr"
        bad = 1
      }
      used[key] = 1
      print $0, adjudicated[key]
    } else {
      print $0, ""
    }
  }
  END {
    for (key in graders) if (!(key in used)) { print "score.sh: no session matches " key > "/dev/stderr"; bad = 1 }
    exit bad
  }
' "$adjudication" "$out/graded.tsv" > "$out/sessions.tsv"
rm "$out/graded.tsv"

model=$(jq -r '.suite.modelOverride // ""' "$plugin_json")
for f in $prose_json $hook_json; do
  other_model=$(jq -r '.suite.modelOverride // ""' "$f")
  if [ "$model" != "$other_model" ]; then
    echo "score.sh: the stages ran on different models, $model and $other_model" >&2
    exit 1
  fi
done
version=$(jq -r '.claudeVersion' "$plugin_json")
runs=$(jq -r '[.cases[].arms[] | length] | max' "$plugin_json")
cost=$(jq -s '(map(.costUsd) | add) * 100 | round / 100' "$plugin_json" $prose_json $hook_json)

excluded_cases=$(printf "%s" "$excluded" | wc -w | tr -d ' ')
adjudicated=$(awk -F '\t' '$12 != "" { n++ } END { print n - 1 }' "$out/sessions.tsv")

awk -F '\t' -v method="$method" -v excluded="$excluded_cases" -v model="$model" -v version="$version" \
  -v runs="$runs" -v cost="$cost" -v adjudicated="$adjudicated" '
  function rate(n, d) { return d == 0 ? "" : sprintf("%.2f", n / d) }
  function lead(a, b) { return (a == "" || b == "") ? "" : sprintf("%.2f", a - b) }
  function row(field, value) { printf "| `%s` | %s |\n", field, value }
  NR == 1 { next }
  {
    k = $1 "." $2
    cases[$3] = 1
    if ($1 != "house" && $1 != "control") bad = $3
    sessions[k]++
    total++
    if ($5 == "1") { errors[k]++; next }
    graded[k]++
    if ($6 != "0") {
      graders_done[k]++
      if ($7 == "1") graders_complied[k]++
    }
    task = $6 != "0"
    comply = $7 == "1"
    if ($12 != "") {
      task = $12 != "no task"
      comply = $12 == "pass"
      if ($12 != ($6 == "0" ? "no task" : ($7 == "1" ? "pass" : "fail"))) overturned[k]++
    }
    if (task) {
      done[k]++
      if (comply) complied[k]++
    }
    if ($8 == "1") cited[k]++
    if ($9 == "1") fired[k]++
  }
  END {
    if (bad != "") { print "score.sh: case " bad " is in neither house/ nor control/" > "/dev/stderr"; exit 1 }
    n = 0
    for (c in cases) n++
    print "| Field | Value |"
    print "|-------|-------|"
    row("trial", "`plugin`")
    row("method", method)
    row("model", "`" model "`")
    row("claude_version", version)
    row("cases", n)
    row("excluded_cases", excluded)
    row("runs_per_case", runs)
    row("sessions", total)
    row("adjudicated", adjudicated)
    split("house control", kinds, " ")
    split("plugin hook prose none", arms, " ")
    for (j = 1; j <= 2; j++) {
      for (i = 1; i <= 4; i++) {
        k = kinds[j] "." arms[i]
        row(k ".sessions", sessions[k] + 0)
        row(k ".errors", errors[k] + 0)
        row(k ".task_done", done[k] + 0)
        row(k ".complied", complied[k] + 0)
        compliance[k] = rate(complied[k], done[k])
        row(k ".compliance_rate", compliance[k])
        row(k ".graders_task_done", graders_done[k] + 0)
        row(k ".graders_complied", graders_complied[k] + 0)
        graders_compliance[k] = rate(graders_complied[k], graders_done[k])
        row(k ".graders_compliance_rate", graders_compliance[k])
        row(k ".overturned", overturned[k] + 0)
        row(k ".cited", cited[k] + 0)
        row(k ".citation_rate", rate(cited[k], graded[k]))
      }
      p = kinds[j] ".plugin"
      h = kinds[j] ".hook"
      row(p ".fired", fired[p] + 0)
      row(h ".fired", fired[h] + 0)
      row(kinds[j] ".lead_over_prose", lead(compliance[p], compliance[kinds[j] ".prose"]))
      row(kinds[j] ".lead_over_none", lead(compliance[p], compliance[kinds[j] ".none"]))
      row(kinds[j] ".graders_lead_over_prose", lead(graders_compliance[p], graders_compliance[kinds[j] ".prose"]))
      row(kinds[j] ".graders_lead_over_none", lead(graders_compliance[p], graders_compliance[kinds[j] ".none"]))
      row(kinds[j] ".hook_lead_over_plugin", lead(compliance[h], compliance[p]))
      row(kinds[j] ".hook_lead_over_prose", lead(compliance[h], compliance[kinds[j] ".prose"]))
      row(kinds[j] ".hook_lead_over_none", lead(compliance[h], compliance[kinds[j] ".none"]))
      row(kinds[j] ".graders_hook_lead_over_plugin", lead(graders_compliance[h], graders_compliance[p]))
    }
    row("cost_usd", sprintf("%.2f", cost))
    row("cost_per_session_usd", total == 0 ? "" : sprintf("%.2f", cost / total))
  }
' "$out/sessions.tsv" > "$out/aggregate.md"

# A path on the machine that ran the trial says nothing to a reader, and may name its user.
strip='walk(if type == "string" and startswith("/") then (split("/") | last) else . end)'
jq "$strip" "$plugin_json" > "$out/results/plugin-stage.json"
if [ -n "$prose_json" ]; then jq "$strip" "$prose_json" > "$out/results/prose-stage.json"; fi
if [ -n "$hook_json" ]; then jq "$strip" "$hook_json" > "$out/results/hook-stage.json"; fi

echo "scored $(($(wc -l < "$out/sessions.tsv") - 1)) session(s) into $out"
