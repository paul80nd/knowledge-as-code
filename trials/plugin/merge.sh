#!/bin/sh
# Merges one stage's result files from several batches into one result file that score.sh reads.
#
#   merge.sh <out.json> <aggregate-result.json>...
#
# Each case's runs are the runs of every batch that ran it. The cost is the sum, and the result is partial if any
# batch was. Every batch must have run the same stage, model, Claude Code version and plugin, and must grade each case
# it shares with the others by the same graders.
set -eu

if [ "$#" -lt 3 ]; then
  echo "usage: merge.sh <out.json> <aggregate-result.json> <aggregate-result.json>..." >&2
  exit 2
fi

out=$1
shift

for field in .suite.ablation .suite.modelOverride .claudeVersion '.suite.plugins | map({name, version})'; do
  if [ "$(jq -s "map($field) | unique | length" "$@")" != "1" ]; then
    echo "merge.sh: the batches differ in $field: $(jq -sc "map($field) | unique" "$@")" >&2
    exit 1
  fi
done

graders=$(jq -s '[.[].cases[] | {name, graders: [.graders[] | {name, type, config}]}] | group_by(.name)
  | map(select(map(.graders) | unique | length > 1) | .[0].name)' "$@")
if [ "$graders" != "[]" ]; then
  echo "merge.sh: these cases are graded differently in different batches: $graders" >&2
  echo "merge.sh: regrade the older batch with regrade.mjs first" >&2
  exit 1
fi

jq -s '. as $all
  | .[0]
  | .costUsd = ($all | map(.costUsd) | add)
  | .partial = ($all | map(.partial) | any)
  | .cases = ([$all[].cases[]] | group_by(.name) | map(.[0] as $case
      | $case | .arms |= with_entries(.key as $arm | .value = [$all[].cases[] | select(.name == $case.name)
        | .arms[$arm][]?])))' "$@" > "$out"

echo "merged $# batch(es) into $out"
