#!/usr/bin/env sh
# round-trip — install the assembled plugin and ask it the questions its skill describes.
#
# Everything else about the export and the bundle is proved over data: the golden fixtures diff the
# tree file for file, and the unit tests pin the rules that built it. What none of them can show is
# the chain end to end — that a bundle installs, that the paths its skill names resolve inside the
# installed copy, and that a link built from its template fetches the record it points at.
#
# Run it from the corpus root, after `kac export` and `kac bundle`:
#
#   cd example && sh ../tooling/tests/round-trip.sh
#
# It reads `.dist/`, writes only inside a work directory of its own, and installs into a Claude
# config directory of its own, so it leaves the machine it ran on as it found it.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process
# substitution. Command substitution is used freely, because a file on disk reaches the shell
# unparsed.
#
# Three prerequisites, all already on a GitHub runner: `jq` reads the manifests, `curl` fetches a
# raw link, and the Claude Code CLI installs the plugin.

set -e

WORK=${WORK:-$(mktemp -d)}
rm -rf "$WORK"
mkdir -p "$WORK"

REPO=$(pwd)

# The CLI is a Node program, so it reads a path the way the platform does. A shell variable reaches
# it from Git Bash unconverted, and a POSIX path there would name a different directory to the two
# sides. `cygpath -m` gives the one form both accept: a drive letter, with forward slashes, which
# MSYS tools take as readily as Node does. Absent on Linux and macOS, where the question does not
# arise.
if command -v cygpath > /dev/null 2>&1; then
  WORK=$(cygpath -m "$WORK")
  REPO=$(cygpath -m "$REPO")
fi

CLAUDE_CONFIG_DIR="$WORK/config"
export CLAUDE_CONFIG_DIR
mkdir -p "$CLAUDE_CONFIG_DIR"

fail() {
  echo "round-trip: $1"
  exit 1
}

# Every read of a manifest goes through here. jq writes CRLF on Windows and Git Bash keeps the
# carriage return, which then reaches a path, a comparison and a URL — and is invisible in the
# message reporting the failure, because printing it returns the cursor to the start of the line.
jqr() {
  jq -r "$@" | tr -d '\r'
}

[ -d "$REPO/.dist/plugin" ] || fail "no .dist/plugin — run kac export and kac bundle first."

# ── install ───────────────────────────────────────────────────────────────────────────────────────
#
# Both names are read rather than written down. A corpus names its own plugin and its own
# marketplace, so a script that hard-coded either would pass here and fail in the next corpus to
# run it.

MARKETPLACE=$(jqr '.name' "$REPO/.dist/.claude-plugin/marketplace.json")
PLUGIN=$(jqr '.plugins[0].name' "$REPO/.dist/.claude-plugin/marketplace.json")

echo "round-trip: installing $PLUGIN from $MARKETPLACE"
claude plugin marketplace add "$REPO/.dist"
claude plugin install "$PLUGIN@$MARKETPLACE"

# ── the installed copy ────────────────────────────────────────────────────────────────────────────
#
# `${CLAUDE_PLUGIN_ROOT}` in the skill resolves to whatever directory the install produced, and the
# skill's paths hang off it. Finding that directory by its manifest rather than by a cache layout
# keeps this reading the same thing the agent reads.

MANIFEST=$(find "$CLAUDE_CONFIG_DIR/plugins/cache" -type f -path '*/.claude-plugin/plugin.json' | head -1)
[ -n "$MANIFEST" ] || fail "the install produced no plugin manifest under the config directory."

ROOT=$(dirname "$(dirname "$MANIFEST")")
echo "round-trip: installed at $ROOT"

# An install that referenced the source directory instead of copying it would make every assertion
# below a statement about `.dist/` rather than about what a reader receives.
case "$ROOT" in
  "$REPO"/*) fail "the install referenced the source tree rather than taking a copy of it." ;;
esac

CORPUS_ROOT=$(jqr '.metadata.corpusRoot' "$MANIFEST")
[ -d "$ROOT/$CORPUS_ROOT" ] || fail "the installed plugin holds no $CORPUS_ROOT directory."

EXPORT_MANIFEST="$ROOT/$CORPUS_ROOT/manifest.json"
[ -f "$EXPORT_MANIFEST" ] || fail "the installed plugin holds no $CORPUS_ROOT/manifest.json."

# ── a link built from the template, and fetched ───────────────────────────────────────────────────
#
# The one assertion here that leaves the machine. A template with the wrong host, the wrong ref or a
# path prefix that no longer exists assembles into a string that matches any pattern you would write
# for it and fetches a 404, or a page from a version of the corpus nobody asked about. Only the fetch
# tells those apart, and comparing the response against the file in the working tree is what says the
# template addressed this corpus at this commit.
#
# Written to outlive glossary. It takes the template and one record's `path` from each type the
# export declares, so a corpus that adopts a second type brings its records under the same check
# without a line changing here. An ADR needs this more than a term does: a term line carries its
# whole content, and a broken raw link costs the reader nothing, where an ADR's raw link is the only
# route an agent has to the text.

RAW_TEMPLATE=$(jqr '.publishing.rawTemplate // empty' "$EXPORT_MANIFEST")

if [ -z "$RAW_TEMPLATE" ]; then
  echo "round-trip: the export names no raw template — this corpus publishes nowhere, so no link is checked."
else
  jqr '.types[].dir' "$EXPORT_MANIFEST" > "$WORK/dirs.txt"

  while read -r dir; do
    # A type contributing no record has no path to substitute. Saying so beats failing over an
    # emptiness the corpus is entitled to.
    RECORDS=$(jqr --arg d "$dir" '.types[] | select(.dir == $d) | .records' "$EXPORT_MANIFEST")
    if [ "$RECORDS" = "0" ]; then
      echo "round-trip: $dir carries no record — no link to build."
      continue
    fi

    PARTS_FILE=$(jqr --arg d "$dir" '.types[] | select(.dir == $d) | .partsFile // empty' "$EXPORT_MANIFEST")
    PARTS_NAME=$(basename "$PARTS_FILE")

    RECORD=$(find "$ROOT/$CORPUS_ROOT/$dir" -type f -name '*.json' ! -name "$PARTS_NAME" | sort | head -1)
    [ -n "$RECORD" ] || fail "$dir declares $RECORDS record(s) and the installed plugin holds none."

    RECORD_PATH=$(jqr '.path' "$RECORD")
    URL=$(echo "$RAW_TEMPLATE" | sed "s|{path}|$RECORD_PATH|")

    echo "round-trip: fetching $URL"
    curl -sS --fail --location --output "$WORK/fetched.md" "$URL" \
      || fail "the raw template built a link that does not fetch: $URL"

    cmp -s "$WORK/fetched.md" "$REPO/$RECORD_PATH" \
      || fail "the link fetched something other than $RECORD_PATH as this corpus holds it."

    echo "round-trip: $dir — the raw link returned the record's own source."
  done < "$WORK/dirs.txt"
fi

# ── the lookup the skill describes ────────────────────────────────────────────────────────────────
#
# Glossary only, and skipped where the export carries none, because this is the one section written
# about a particular type. What it proves is the whole reason the plugin exists: a term searched for
# by the skill's own pattern comes back carrying the two things the skill tells a reader to give.

TERMS="$ROOT/$CORPUS_ROOT/glossary/terms.jsonl"

if [ ! -f "$TERMS" ]; then
  echo "round-trip: the export carries no glossary — no lookup to perform."
  exit 0
fi

echo "round-trip: terms reachable at $CORPUS_ROOT/glossary/terms.jsonl inside the installed plugin"

# The skill's first pattern, written as ERE. It says to write `\s*` rather than a literal space,
# because nothing promises the export puts one after a colon; ` *` is the same statement in the
# dialect grep speaks.
grep -iE '"title": *"Borrower"' "$TERMS" > "$WORK/hits.txt" || fail "no line defines Borrower."

HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
[ "$HITS" = "1" ] || fail "expected one line defining Borrower, found $HITS."

ID=$(jqr '.id' "$WORK/hits.txt")
[ "$ID" = "gls-example-libraries.borrower" ] || fail "Borrower came back as $ID."

DEFINITION=$(jqr '.definition // empty' "$WORK/hits.txt")
[ -n "$DEFINITION" ] || fail "$ID came back with no definition."

# The `**Not:**` line, which the skill tells a reader to give alongside the definition. A reader
# handed only the definition goes on to apply the term to the things it excludes.
NOT=$(jqr '.not // empty' "$WORK/hits.txt")
[ -n "$NOT" ] || fail "$ID came back with no Not line."

echo "round-trip: $ID — definition and Not line both present."

# ── what the order of the file means, and what it does not ────────────────────────────────────────
#
# `ExporterTests` pins these rules over corpora it builds for the purpose. This asserts them over the
# corpus a reader actually receives, which is the only place the two could have come apart.

line_of() {
  grep -n "\"id\": *\"$1\"" "$TERMS" | cut -d: -f1
}

# The chain, which is the case the ordering exists for. `gls-search` narrows `gls-example-libraries`
# and both define `title`, so a grep meets the general entry before the one refining it.
GENERAL=$(line_of gls-example-libraries.title)
NARROWER=$(line_of gls-search.title)

[ -n "$GENERAL" ] || fail "gls-example-libraries.title is not in the export."
[ -n "$NARROWER" ] || fail "gls-search.title is not in the export."
[ "$GENERAL" -lt "$NARROWER" ] || fail "gls-search.title came before the entry it narrows."

echo "round-trip: gls-example-libraries.title precedes gls-search.title, as the chain requires."

# Across roots, only that the order does not move. `record` is a bibliographic record in one glossary
# and a knowledge document in the other; neither narrows the other, and nothing ranks them. This is
# deliberately not an assertion that the first is the more general — reading it that way is what would
# hand a reader the wrong domain, so the absence of that assertion is the point rather than an
# oversight.
FIRST=$(line_of gls-example-libraries.record)
SECOND=$(line_of gls-knowledge-as-code.record)

[ -n "$FIRST" ] || fail "gls-example-libraries.record is not in the export."
[ -n "$SECOND" ] || fail "gls-knowledge-as-code.record is not in the export."
[ "$FIRST" -lt "$SECOND" ] || fail "the order of the two record entries moved."

echo "round-trip: the two record entries come back in a stable order, and neither is claimed the general one."
echo "round-trip: passed."
