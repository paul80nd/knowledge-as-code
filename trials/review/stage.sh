#!/bin/sh
# Stages a built plugin and a folder of review cases for `claude plugin eval`, which reads cases from below the plugin.
#
#   stage.sh <plugin dir> <cases dir> <empty stage dir>
#
# <stage>/plugin is the plugin as built, with the cases under evals/.
set -eu

if [ "$#" -ne 3 ]; then
  echo "usage: stage.sh <plugin dir> <cases dir> <empty stage dir>" >&2
  exit 2
fi

plugin=$1
cases=$2
stage=$3

if [ ! -f "$plugin/.claude-plugin/plugin.json" ] || [ ! -f "$plugin/views/review.md" ]; then
  echo "stage.sh: $plugin is not a plugin kac bundle built with a review view" >&2
  exit 1
fi
if [ ! -f "$plugin/skills/standards-review/SKILL.md" ]; then
  echo "stage.sh: $plugin ships no standards-review skill" >&2
  exit 1
fi
if [ -e "$stage" ] && [ -n "$(ls -A "$stage")" ]; then
  echo "stage.sh: $stage is not empty" >&2
  exit 1
fi

mkdir -p "$stage/plugin"
cp -R "$plugin/." "$stage/plugin/"
mkdir "$stage/plugin/evals"
cp -R "$cases/." "$stage/plugin/evals/"

echo "staged $(find "$stage/plugin/evals" -name prompt.md | wc -l | tr -d ' ') case(s) in $stage"
