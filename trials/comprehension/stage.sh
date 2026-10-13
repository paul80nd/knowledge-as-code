#!/bin/sh
# Copies one section of the documentation site into an empty directory, so a reader is given that section alone.
set -eu

if [ $# -ne 2 ]; then
  echo "usage: stage.sh <use|framework|reference> <empty-directory>" >&2
  exit 2
fi

root=$(cd "$(dirname "$0")/../.." && pwd)
section="$root/docs/$1"
[ -d "$section" ] || { echo "no section at docs/$1" >&2; exit 2; }

mkdir -p "$2"
[ -z "$(ls -A "$2")" ] || { echo "$2 is not empty" >&2; exit 2; }

cp -R "$section"/. "$2"/
find "$2" -name '*.md' -exec cat {} + | wc -w | awk -v s="$1" '{ print "staged docs/" s ": " $1 " words" }'
