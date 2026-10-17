#!/bin/sh
# Stages the plugin, prose and none arms for `claude plugin eval`, from a built plugin and a folder of cases.
#
#   stage.sh <plugin dir> <cases dir> <empty stage dir>
#
# <cases dir> keeps each case in house/ or control/, which is how score.sh tells the two kinds apart.
# A case listed in <cases dir>/excluded.tsv is not staged, so no session runs it.
# <stage>/plugin is the plugin with the cases under evals/, for the plugin and none arms.
# <stage>/prose is a plugin with no components, whose cases append the generated CLAUDE.md to the system prompt.
set -eu

if [ "$#" -ne 3 ]; then
  echo "usage: stage.sh <plugin dir> <cases dir> <empty stage dir>" >&2
  exit 2
fi

plugin=$1
cases=$2
stage=$3
here=$(cd "$(dirname "$0")" && pwd)

if [ ! -f "$plugin/.claude-plugin/plugin.json" ] || [ ! -d "$plugin/corpus" ]; then
  echo "stage.sh: $plugin is not a plugin kac bundle built" >&2
  exit 1
fi
if [ -e "$stage" ] && [ -n "$(ls -A "$stage")" ]; then
  echo "stage.sh: $stage is not empty" >&2
  exit 1
fi

misplaced=$(find "$cases" -name prompt.md | while IFS= read -r p; do
  kind=$(basename "$(dirname "$(dirname "$p")")")
  if [ "$kind" != house ] && [ "$kind" != control ]; then
    echo "$p"
  fi
done)
if [ -n "$misplaced" ]; then
  echo "stage.sh: every case belongs in house/ or control/. These are not:" >&2
  echo "$misplaced" >&2
  exit 1
fi

mkdir -p "$stage/plugin" "$stage/prose/.claude-plugin"

cp -R "$plugin/." "$stage/plugin/"
mkdir "$stage/plugin/evals"
cp -R "$cases/." "$stage/plugin/evals/"

printf '{ "name": "prose-arm", "version": "0.0.0" }\n' > "$stage/prose/.claude-plugin/plugin.json"
mkdir "$stage/prose/evals"
cp -R "$cases/." "$stage/prose/evals/"

rm -f "$stage/plugin/evals/excluded.tsv" "$stage/prose/evals/excluded.tsv"
if [ -f "$cases/excluded.tsv" ]; then
  cut -f 1 "$cases/excluded.tsv" | while IFS= read -r excluded; do
    if [ ! -d "$cases/$excluded" ]; then
      echo "stage.sh: excluded.tsv lists $excluded, which is not a case" >&2
      exit 1
    fi
    rm -rf "$stage/plugin/evals/$excluded" "$stage/prose/evals/$excluded"
  done
fi
"$here/prose-claude-md.sh" "$plugin/corpus" "$stage/CLAUDE.md"

# The tool loads no CLAUDE.md in a run, so the prose arm appends it to the system prompt.
find "$stage/prose/evals" -name case.yaml | while IFS= read -r case_file; do
  {
    printf 'execution:\n  append_system_prompt: |\n'
    sed 's/^/    /' "$stage/CLAUDE.md"
  } >> "$case_file"
done

echo "staged $(find "$stage/plugin/evals" -name prompt.md | wc -l | tr -d ' ') case(s) in $stage"
