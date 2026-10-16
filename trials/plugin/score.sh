#!/bin/sh
# Scores one run of the plugin trial from the two result files `claude plugin eval` wrote.
#
#   score.sh <plugin stage aggregate-result.json> <prose stage aggregate-result.json> <run folder>
#
# Writes sessions.tsv, aggregate.md, and both result files with every absolute path cut to its last part.
set -eu

method=2

if [ "$#" -ne 3 ]; then
  echo "usage: score.sh <plugin stage result> <prose stage result> <run folder>" >&2
  exit 2
fi

plugin_json=$1
prose_json=$2
out=$3

for f in "$plugin_json" "$prose_json"; do
  if [ "$(jq -r '.partial' "$f")" != "false" ]; then
    echo "score.sh: $f is a partial run, which this trial does not score" >&2
    exit 1
  fi
done
if [ "$(jq -r '.suite.ablation' "$prose_json")" != "none" ]; then
  echo "score.sh: $prose_json did not run under --ablation none" >&2
  exit 1
fi

mkdir -p "$out/results"

# One line per session. Graders whose names share the part before `--` are alternatives, and pass where one does.
# A verdict is 1 where every grader or set of alternatives with that prefix passed, 0 where one failed, and empty
# where the case has none. A case's kind is the folder it sits in, house or control.
sessions() {
  jq -r --arg prose "$2" '
    def verdict(p): [.graders[]? | select(.name | startswith(p))]
      | group_by(.name | split("--")[0]) | map(any(.passed))
      | if length == 0 then "" elif all then "1" else "0" end;
    .cases[] | .name as $case | (.dir | split("/") | .[-2]) as $kind | .arms | to_entries[]
    | (if $prose == "1" then "prose" elif .key == "with" then "plugin" else "none" end) as $arm
    | .value[]
    | [$kind, $arm, $case, (if .error == null then "0" else "1" end),
       verdict("task-"), verdict("comply-"), verdict("cite-"), verdict("fired-"),
       (.costUsd // 0), (.turns // "")]
    | @tsv
  ' "$1"
}

{
  printf 'kind\tarm\tcase\terror\ttask\tcomply\tcite\tfired\tcost_usd\tturns\n'
  sessions "$plugin_json" 0
  sessions "$prose_json" 1
} > "$out/sessions.tsv"

model=$(jq -r '.suite.modelOverride // ""' "$plugin_json")
prose_model=$(jq -r '.suite.modelOverride // ""' "$prose_json")
if [ "$model" != "$prose_model" ]; then
  echo "score.sh: the stages ran on different models, $model and $prose_model" >&2
  exit 1
fi
version=$(jq -r '.claudeVersion' "$plugin_json")
runs=$(jq -r '[.cases[].arms[] | length] | max' "$plugin_json")
cost=$(jq -s '(.[0].costUsd + .[1].costUsd) * 100 | round / 100' "$plugin_json" "$prose_json")

awk -F '\t' -v method="$method" -v model="$model" -v version="$version" -v runs="$runs" -v cost="$cost" '
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
    if ($4 == "1") { errors[k]++; next }
    graded[k]++
    if ($5 != "0") {
      done[k]++
      if ($6 == "1") complied[k]++
    }
    if ($7 == "1") cited[k]++
    if ($8 == "1") fired[k]++
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
    row("runs_per_case", runs)
    row("sessions", total)
    split("house control", kinds, " ")
    split("plugin prose none", arms, " ")
    for (j = 1; j <= 2; j++) {
      for (i = 1; i <= 3; i++) {
        k = kinds[j] "." arms[i]
        row(k ".sessions", sessions[k] + 0)
        row(k ".errors", errors[k] + 0)
        row(k ".task_done", done[k] + 0)
        row(k ".complied", complied[k] + 0)
        compliance[k] = rate(complied[k], done[k])
        row(k ".compliance_rate", compliance[k])
        row(k ".cited", cited[k] + 0)
        row(k ".citation_rate", rate(cited[k], graded[k]))
      }
      row(kinds[j] ".plugin.fired", fired[kinds[j] ".plugin"] + 0)
      row(kinds[j] ".lead_over_prose", lead(compliance[kinds[j] ".plugin"], compliance[kinds[j] ".prose"]))
      row(kinds[j] ".lead_over_none", lead(compliance[kinds[j] ".plugin"], compliance[kinds[j] ".none"]))
    }
    row("cost_usd", sprintf("%.2f", cost))
    row("cost_per_session_usd", total == 0 ? "" : sprintf("%.2f", cost / total))
  }
' "$out/sessions.tsv" > "$out/aggregate.md"

# A path on the machine that ran the trial says nothing to a reader, and may name its user.
strip='walk(if type == "string" and startswith("/") then (split("/") | last) else . end)'
jq "$strip" "$plugin_json" > "$out/results/plugin-stage.json"
jq "$strip" "$prose_json" > "$out/results/prose-stage.json"

echo "scored $(($(wc -l < "$out/sessions.tsv") - 1)) session(s) into $out"
