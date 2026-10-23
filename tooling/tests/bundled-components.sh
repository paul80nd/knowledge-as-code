#!/usr/bin/env sh
# bundled-components: check that a plugin holds every component its bundle.json kept, and none it trimmed.
# Usage: sh tooling/tests/bundled-components.sh <plugin directory>. The `corpora` job runs it over `.dist/plugin`,
# and `round-trip.sh` runs it over the installed copy. A component is a directory or a file, so both pass `[ -e ]`.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process substitution.

set -e

fail() {
  echo "bundled-components: $1"
  exit 1
}

[ $# -eq 1 ] || fail "usage: sh bundled-components.sh <plugin directory>."

PLUGIN_DIR=$1
BUNDLE="$PLUGIN_DIR/bundle.json"
[ -f "$BUNDLE" ] || fail "$PLUGIN_DIR holds no bundle.json."

# jq writes CRLF on Windows and Git Bash keeps the carriage return, which then reaches a path unseen.
jqr() {
  jq -r "$@" | tr -d '\r'
}

# POSIX sh has no `pipefail`, so a jq failure and a renamed key both read as an empty list. Asking for the
# type catches both, because `select` on a false condition writes nothing at all.
COMPONENTS=$(jqr 'select((.included | type) == "array" and (.trimmed | type) == "array")
  | (.included | length) + (.trimmed | length)' "$BUNDLE")
[ -n "$COMPONENTS" ] && [ "$COMPONENTS" -gt 0 ] \
  || fail "bundle.json names no component under included and trimmed."

INCLUDED=$(jqr '.included[].path' "$BUNDLE")
TRIMMED=$(jqr '.trimmed[].path' "$BUNDLE")

missing=''
while read -r component; do
  [ -n "$component" ] || continue
  if [ -e "$PLUGIN_DIR/$component" ]; then
    echo "bundled-components: $component is in the plugin."
  else
    missing="$missing $component"
  fi
done <<EOF
$INCLUDED
EOF

[ -z "$missing" ] || fail "bundle.json kept components $PLUGIN_DIR does not hold:$missing"

stray=''
while read -r component; do
  [ -n "$component" ] || continue
  if [ -e "$PLUGIN_DIR/$component" ]; then
    stray="$stray $component"
  else
    echo "bundled-components: $component was trimmed, and is absent."
  fi
done <<EOF
$TRIMMED
EOF

[ -z "$stray" ] || fail "bundle.json trimmed components $PLUGIN_DIR holds anyway:$stray"

echo "bundled-components: $PLUGIN_DIR holds what bundle.json kept, and nothing it trimmed."
