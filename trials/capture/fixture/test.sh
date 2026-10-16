#!/bin/sh
# Builds the fixture, takes both readings with capture.sh, and compares each aggregate with the expected one.
set -eu

here=$(cd "$(dirname "$0")" && pwd)
capture=$here/../capture.sh
GIT_CONFIG_NOSYSTEM=1 GIT_CONFIG_GLOBAL=/dev/null
export GIT_CONFIG_NOSYSTEM GIT_CONFIG_GLOBAL

dir=$(mktemp -d "${TMPDIR:-/tmp}/capture-fixture.XXXXXX")
trap 'rm -rf "$dir"' EXIT
sh "$here/build.sh" "$dir/fixture"
fx=$dir/fixture
owners='olive@example.invalid,olive.work@example.invalid'
failed=0

# Takes one reading, and compares its aggregate with the expected file.
reading() {
  sh "$capture" -c "$fx/clone/kb" -d "$2" -r "$1" -o "$owners" -x bot@example.invalid \
    -f "$fx/findings.tsv" -l "$fx/log-$1.csv" "$dir/reading-$1" 2>"$dir/stderr-$1" >/dev/null || {
    echo "FAIL reading $1: capture.sh stopped"; cat "$dir/stderr-$1"; failed=1; return
  }
  if diff -u "$here/expected/reading-$1.md" "$dir/reading-$1/aggregate.md"; then
    echo "pass reading $1"
  else
    echo "FAIL reading $1: the aggregate differs from expected/reading-$1.md"; failed=1
  fi
}

# Runs capture.sh where it must stop, and checks that it does with the message given.
refuses() {
  what=$1 message=$2; shift 2
  if sh "$capture" "$@" "$dir/refused" >/dev/null 2>"$dir/stderr"; then
    echo "FAIL $what: capture.sh did not stop"; failed=1
  elif grep -qF "$message" "$dir/stderr"; then
    echo "pass $what"
  else
    echo "FAIL $what: expected '$message', got:"; cat "$dir/stderr"; failed=1
  fi
  rm -rf "$dir/refused"
}

reading 1 2026-03-10
reading 2 2026-06-10

refuses 'an -o matching no commit' 'no commit to the corpus up to 2026-06-10 is by an address under -o' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o nobody@example.invalid -f "$fx/findings.tsv"
refuses 'an azure-devops tracker with no -f' 'the tracker is on Azure DevOps, which the script cannot query' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners"
printf '2026-03-04 no tab here\n' >"$dir/bad-findings.tsv"
refuses 'a findings line with no tab' 'each line of the findings file takes a date, a tab and an address' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners" -f "$dir/bad-findings.tsv"

[ "$failed" = 0 ] && echo "every case passed"
exit "$failed"
