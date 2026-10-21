#!/bin/sh
# Copies one section of the documentation site, or the library corpus, into an empty directory for one reader.
set -eu

if [ $# -ne 2 ]; then
  echo "usage: stage.sh <use|framework|reference|library> <empty-directory>" >&2
  exit 2
fi

root=$(cd "$(dirname "$0")/../.." && pwd)

mkdir -p "$2"
[ -z "$(ls -A "$2")" ] || { echo "$2 is not empty" >&2; exit 2; }

if [ "$1" = library ]; then
  # Only the files git tracks, so a restored .imports/ or a built .dist/ never reaches the reader.
  corpus="$root/examples/library"
  git -C "$corpus" ls-files -z | (cd "$corpus" && xargs -0 tar -cf -) | (cd "$2" && tar -xf -)
  staged="examples/library"
else
  section="$root/docs/$1"
  [ -d "$section" ] || { echo "no section at docs/$1" >&2; exit 2; }
  cp -R "$section"/. "$2"/
  staged="docs/$1"
fi

find "$2" -name '*.md' -exec cat {} + | wc -w | awk -v s="$staged" '{ print "staged " s ": " $1 " words" }'
