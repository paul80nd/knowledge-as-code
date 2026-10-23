#!/usr/bin/env sh
# bundled-components: check that a plugin holds every component its bundle.json kept, none it trimmed, and
# nothing in .claude-plugin/ but plugin.json. Usage: sh tooling/tests/bundled-components.sh <plugin directory>.
# The `corpora` job runs it over `.dist/plugin`, and `round-trip.sh` runs it over the installed copy. A component
# is a directory or a file, so both pass `[ -e ]`.
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

# POSIX sh has no `pipefail`, so a jq failure and a renamed key both read as an empty list. The type test
# below catches both.
COMPONENT_COUNT=$(jqr 'select((.included | type) == "array" and (.trimmed | type) == "array")
  | (.included | length) + (.trimmed | length)' "$BUNDLE")
[ -n "$COMPONENT_COUNT" ] && [ "$COMPONENT_COUNT" -gt 0 ] \
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

leftover=''
while read -r component; do
  [ -n "$component" ] || continue
  if [ -e "$PLUGIN_DIR/$component" ]; then
    leftover="$leftover $component"
  else
    echo "bundled-components: $component was trimmed, and is absent."
  fi
done <<EOF
$TRIMMED
EOF

[ -z "$leftover" ] || fail "bundle.json trimmed components $PLUGIN_DIR holds anyway:$leftover"

# Claude Code loads nothing from .claude-plugin/ but the manifest, so a component filed there is lost.
STRAY=$(find "$PLUGIN_DIR/.claude-plugin" -mindepth 1 -maxdepth 1 ! -name plugin.json)
if [ -n "$STRAY" ]; then
  echo "bundled-components: filed inside .claude-plugin/, where nothing loads it:"
  echo "$STRAY"
  exit 1
fi

echo "bundled-components: $PLUGIN_DIR holds what bundle.json kept, nothing it trimmed, and no stray file."
