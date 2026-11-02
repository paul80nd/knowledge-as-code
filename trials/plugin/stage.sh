#!/bin/sh
# Stages the plugin, hook, prose and none arms for `claude plugin eval`, from a built plugin and a folder of cases.
#
#   stage.sh <plugin dir> <cases dir> <empty stage dir> [prose file]
#
# <cases dir> keeps each case in house/ or control/, which is how score.mjs tells the two kinds apart.
# A case listed in <cases dir>/excluded.tsv is not staged, so no session runs it.
# <stage>/plugin is the plugin less hooks/build-index.json, so its hook prints nothing, for the plugin and none arms.
# <stage>/hook is the plugin as built, whose hook pushes the build index, for the hook arm. A plugin with no
# hooks/build-index.json is staged as well, and its hook arm pushes nothing.
# <stage>/prose is a plugin whose only component is a SessionStart hook a page, printing the prose text. The text is
# every record the export contains, or the cut [prose file] gives.
set -eu

if [ "$#" -ne 3 ] && [ "$#" -ne 4 ]; then
  echo "usage: stage.sh <plugin dir> <cases dir> <empty stage dir> [prose file]" >&2
  exit 2
fi

plugin=$1
cases=$2
stage=$3
cut=${4:-}
here=$(cd "$(dirname "$0")" && pwd)

if [ ! -f "$plugin/.claude-plugin/plugin.json" ] || [ ! -d "$plugin/corpus" ]; then
  echo "stage.sh: $plugin is not a plugin kac bundle built" >&2
  exit 1
fi
if [ ! -f "$plugin/hooks/build-index.json" ]; then
  echo "stage.sh: $plugin ships no hooks/build-index.json, so the hook arm has no build index to push" >&2
fi
if [ -n "$cut" ] && [ ! -s "$cut" ]; then
  echo "stage.sh: $cut is not a prose file" >&2
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

mkdir -p "$stage/plugin" "$stage/hook" "$stage/prose/.claude-plugin"

for arm in plugin hook; do
  cp -R "$plugin/." "$stage/$arm/"
  mkdir "$stage/$arm/evals"
  cp -R "$cases/." "$stage/$arm/evals/"
done
rm -f "$stage/plugin/hooks/build-index.json"

printf '{ "name": "prose-arm", "version": "0.0.0" }\n' > "$stage/prose/.claude-plugin/plugin.json"
mkdir "$stage/prose/evals"
cp -R "$cases/." "$stage/prose/evals/"

rm -f "$stage/plugin/evals/excluded.tsv" "$stage/hook/evals/excluded.tsv" "$stage/prose/evals/excluded.tsv"
if [ -f "$cases/excluded.tsv" ]; then
  cut -f 1 "$cases/excluded.tsv" | while IFS= read -r excluded; do
    if [ ! -d "$cases/$excluded" ]; then
      echo "stage.sh: excluded.tsv lists $excluded, which is not a case" >&2
      exit 1
    fi
    rm -rf "$stage/plugin/evals/$excluded" "$stage/hook/evals/$excluded" "$stage/prose/evals/$excluded"
  done
fi
if [ -n "$cut" ]; then
  cp "$cut" "$stage/CLAUDE.md"
else
  node "$here/prose-claude-md.mjs" "$plugin/corpus" "$stage/CLAUDE.md"
fi

# Claude Code warns on a CLAUDE.md over 40,000 characters, and the README says to give the prose arm a cut past that.
chars=$(wc -m < "$stage/CLAUDE.md" | tr -d ' ')
echo "the prose arm's CLAUDE.md is $chars characters"
if [ "$chars" -gt 40000 ]; then
  echo "stage.sh: the prose arm's CLAUDE.md is over 40,000 characters. Cut it, as the README says." >&2
fi

# Windows caps a command line at 32,767 characters, so the text reaches each session from files, never as an argument.
node "$here/prose-pages.mjs" "$stage/CLAUDE.md" "$stage/prose"

echo "staged $(find "$stage/plugin/evals" -name prompt.md | wc -l | tr -d ' ') case(s) in $stage"
