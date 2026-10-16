#!/bin/sh
# Scores one run of the plugin trial from the two result files `claude plugin eval` wrote.
#
#   score.sh <plugin stage aggregate-result.json> <prose stage aggregate-result.json> <run folder>
#
# Writes sessions.tsv, aggregate.md, and both result files with every absolute path cut to its last part.
set -eu

method=1

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

# One line per session. A verdict is 1 where every grader with that prefix passed, 0 where one failed, and empty
# where the case has none.
sessions() {
  jq -r --arg prose "$2" '
    def verdict(p): [.graders[]? | select(.name | startswith(p))]
      | if length == 0 then "" elif all(.passed) then "1" else "0" end;
    .cases[] | .name as $case | .arms | to_entries[]
    | (if $prose == "1" then "prose" elif .key == "with" then "plugin" else "none" end) as $arm
    | .value[]
    | [$arm, $case, (if .error == null then "0" else "1" end),
       verdict("task-"), verdict("comply-"), verdict("cite-"), verdict("fired-"),
       (.costUsd // 0), (.turns // "")]
    | @tsv
  ' "$1"
}

{
  printf 'arm\tcase\terror\ttask\tcomply\tcite\tfired\tcost_usd\tturns\n'
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
    cases[$2] = 1
    sessions[$1]++
    total++
    if ($3 == "1") { errors[$1]++; next }
    graded[$1]++
    if ($4 != "0") {
      done[$1]++
      if ($5 == "1") complied[$1]++
    }
    if ($6 == "1") cited[$1]++
    if ($7 == "1") fired[$1]++
  }
  END {
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
    split("plugin prose none", arms, " ")
    for (i = 1; i <= 3; i++) {
      a = arms[i]
      row(a ".sessions", sessions[a] + 0)
      row(a ".errors", errors[a] + 0)
      row(a ".task_done", done[a] + 0)
      row(a ".complied", complied[a] + 0)
      compliance[a] = rate(complied[a], done[a])
      row(a ".compliance_rate", compliance[a])
      row(a ".cited", cited[a] + 0)
      row(a ".citation_rate", rate(cited[a], graded[a]))
    }
    row("plugin.fired", fired["plugin"] + 0)
    row("lead_over_prose", lead(compliance["plugin"], compliance["prose"]))
    row("lead_over_none", lead(compliance["plugin"], compliance["none"]))
    row("cost_usd", sprintf("%.2f", cost))
    row("cost_per_session_usd", total == 0 ? "" : sprintf("%.2f", cost / total))
  }
' "$out/sessions.tsv" > "$out/aggregate.md"

# A path on the machine that ran the trial says nothing to a reader, and may name its user.
strip='walk(if type == "string" and startswith("/") then (split("/") | last) else . end)'
jq "$strip" "$plugin_json" > "$out/results/plugin-stage.json"
jq "$strip" "$prose_json" > "$out/results/prose-stage.json"

echo "scored $(($(wc -l < "$out/sessions.tsv") - 1)) session(s) into $out"
