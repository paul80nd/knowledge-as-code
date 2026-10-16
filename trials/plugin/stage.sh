#!/bin/sh
# Stages the plugin, prose and none arms for `claude plugin eval`, from a built plugin and a folder of cases.
#
#   stage.sh <plugin dir> <cases dir> <empty stage dir>
#
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

mkdir -p "$stage/plugin" "$stage/prose/.claude-plugin"

cp -R "$plugin/." "$stage/plugin/"
mkdir "$stage/plugin/evals"
cp -R "$cases/." "$stage/plugin/evals/"

printf '{ "name": "prose-arm", "version": "0.0.0" }\n' > "$stage/prose/.claude-plugin/plugin.json"
mkdir "$stage/prose/evals"
cp -R "$cases/." "$stage/prose/evals/"
"$here/prose-claude-md.sh" "$plugin/corpus" "$stage/CLAUDE.md"

# The tool loads no CLAUDE.md in a run, so the prose arm appends it to the system prompt.
for case_file in "$stage"/prose/evals/*/case.yaml; do
  {
    printf 'execution:\n  append_system_prompt: |\n'
    sed 's/^/    /' "$stage/CLAUDE.md"
  } >> "$case_file"
done

echo "staged $(find "$stage/plugin/evals" -name prompt.md | wc -l | tr -d ' ') case(s) in $stage"
