#!/usr/bin/env sh
# round-trip: install the assembled plugin and ask each of its skills what that skill describes.
# `tooling/README.md` says what it proves, how to run it and which corpus proves which skill.
#
# It reads `.dist/`, writes only inside a work directory of its own, and installs into a Claude config directory
# of its own, so it leaves the machine as it found it.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process
# substitution. Command substitution is used freely, because a file on disk reaches the shell
# unparsed.

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

# Milliseconds since the epoch, or nothing where `date` has no `%N`, which POSIX does not require.
now_ms() {
  NS=$(date +%s%N)
  case $NS in *[!0-9]*) ;; *) echo $((NS / 1000000)) ;; esac
}

# The span includes starting `date` once, which costs more under Git Bash than under sh.
elapsed() {
  if [ -n "$1" ] && [ -n "$2" ]; then echo "$(($2 - $1)) ms"; else echo "an unmeasured time, because date has no %N"; fi
}

# Every read of a manifest goes through here. jq writes CRLF on Windows and Git Bash keeps the
# carriage return, which then reaches a path, a comparison and a URL — and is invisible in the
# message reporting the failure, because printing it returns the cursor to the start of the line.
jqr() {
  jq -r "$@" | tr -d '\r'
}

[ -d "$REPO/.dist/plugin" ] || fail "no .dist/plugin — run kac export and kac bundle first."

# The fetch below reads the commit the export was taken at and compares it with the working tree.
EXPORT_COMMIT=$(jqr '.commit // empty' "$REPO/.dist/export/manifest.json")
HEAD_COMMIT=$(git -C "$REPO" rev-parse HEAD)
[ -z "$EXPORT_COMMIT" ] || [ "$EXPORT_COMMIT" = "$HEAD_COMMIT" ] \
  || fail "the export was taken at $EXPORT_COMMIT and HEAD is $HEAD_COMMIT. Run kac export and kac bundle again."

# Both names are read rather than written down, because each corpus names its own plugin and marketplace.
MARKETPLACE=$(jqr '.name' "$REPO/.dist/.claude-plugin/marketplace.json")
PLUGIN=$(jqr '.plugins[0].name' "$REPO/.dist/.claude-plugin/marketplace.json")

echo "round-trip: installing $PLUGIN from $MARKETPLACE"
claude plugin marketplace add "$REPO/.dist"
claude plugin install "$PLUGIN@$MARKETPLACE"

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

# A component surviving when its type stayed behind is a skill pointing at files the export never wrote.
# `bundle.json` travels inside the plugin, so the guard reads it from the installed copy.
sh "$(dirname "$0")/bundled-components.sh" "$ROOT" || exit 1
BUNDLE="$ROOT/bundle.json"

# Each skill names the parts file of the type in its `requires`, and no other type's. Skills only: a hook
# renders its text when the bundle is assembled and searches nothing.
jqr '.types[].partsFile // empty' "$EXPORT_MANIFEST" > "$WORK/parts.txt"
jqr '.included[] | select(.path | startswith("skills/")) | .path' "$BUNDLE" > "$WORK/skills.txt"

while read -r skill; do
  [ -n "$skill" ] || continue

  SKILL_FILE="$ROOT/$skill/SKILL.md"
  [ -f "$SKILL_FILE" ] || fail "$skill holds no SKILL.md."

  jqr --arg p "$skill" '.included[] | select(.path == $p) | .requires[]' "$BUNDLE" \
    | sed 's/@.*//' > "$WORK/requires.txt"

  : > "$WORK/own.txt"

  while read -r type; do
    [ -n "$type" ] || continue
    OWN=$(jqr --arg t "$type" '.types[] | select(.type == $t) | .partsFile // empty' "$EXPORT_MANIFEST")
    # A type exporting no parts file leaves this skill with nothing to name.
    [ -n "$OWN" ] || continue
    echo "$OWN" >> "$WORK/own.txt"
    grep -qF "$OWN" "$SKILL_FILE" || fail "$skill requires $type and names no $OWN."
  done < "$WORK/requires.txt"

  while read -r parts; do
    [ -n "$parts" ] || continue
    if grep -qxF "$parts" "$WORK/own.txt"; then
      continue
    fi
    if grep -qF "$parts" "$SKILL_FILE"; then
      fail "$skill names $parts, which belongs to a type it does not require."
    fi
  done < "$WORK/parts.txt"

  echo "round-trip: $skill names its own parts file and no other."
done < "$WORK/skills.txt"

# The one assertion here that leaves the machine. A wrong host, ref or prefix still assembles a plausible URL, so
# only the fetch tells them apart, and comparing the response with the working tree says it addressed this commit.
# The URL is assembled from the base, prefix and ref the export carries, which is the work a consumer does.
PUB_TARGET=$(jqr '.publishing.target' "$EXPORT_MANIFEST")
PUB_BASE=$(jqr '.publishing.base // empty' "$EXPORT_MANIFEST")
PUB_PREFIX=$(jqr '.publishing.pathPrefix // empty' "$EXPORT_MANIFEST")
PUB_REF=$(jqr '.publishing.ref // empty' "$EXPORT_MANIFEST")

if [ "$PUB_TARGET" != "github" ] || [ -z "$PUB_BASE" ] || [ -z "$PUB_REF" ]; then
  echo "round-trip: '$PUB_TARGET' serves no source to an anonymous caller, so no fetch is checked."
else
  RAW_BASE=$(echo "$PUB_BASE" | sed 's|https://github.com/|https://raw.githubusercontent.com/|')
  # An `&&` one-liner would be the shorter spelling and would end the run: `set -e` is on, so the
  # test failing on an absent prefix is a non-zero status at the top level.
  PREFIX_SEG=""
  if [ -n "$PUB_PREFIX" ]; then
    PREFIX_SEG="/$PUB_PREFIX"
  fi

  jqr '.types[].dir' "$EXPORT_MANIFEST" > "$WORK/dirs.txt"

  while read -r dir; do
    # A type contributing no record has no path to substitute, which the corpus is entitled to.
    RECORDS=$(jqr --arg d "$dir" '.types[] | select(.dir == $d) | .records' "$EXPORT_MANIFEST")
    if [ "$RECORDS" = "0" ]; then
      echo "round-trip: $dir carries no record — no link to build."
      continue
    fi

    PARTS_FILE=$(jqr --arg d "$dir" '.types[] | select(.dir == $d) | .partsFile // empty' "$EXPORT_MANIFEST")
    PARTS_NAME=$(basename "$PARTS_FILE")

    # Directly inside the type's directory: a subdirectory holds the records of a consumed corpus, published at
    # that corpus's own commit under its own prefix. Those are fetched below, from the source that published them.
    RECORD=$(find "$ROOT/$CORPUS_ROOT/$dir" -maxdepth 1 -type f -name '*.json' ! -name "$PARTS_NAME" \
      | sort | head -1)

    if [ -z "$RECORD" ]; then
      echo "round-trip: $dir holds only inherited records — nothing of this corpus's own to fetch."
      continue
    fi

    RECORD_PATH=$(jqr '.path' "$RECORD")
    URL="$RAW_BASE/$PUB_REF$PREFIX_SEG/$RECORD_PATH"

    echo "round-trip: fetching $URL"
    curl -sS --fail --location --output "$WORK/fetched.md" "$URL" \
      || fail "the base, prefix and ref built a URL that does not fetch: $URL"

    cmp -s "$WORK/fetched.md" "$REPO/$RECORD_PATH" \
      || fail "the URL fetched something other than $RECORD_PATH as this corpus holds it."

    echo "round-trip: $dir — the fetched source matched the record."
  done < "$WORK/dirs.txt"
fi

# An inherited record sits at its producer's commit under its producer's prefix, so its address is built from
# `sources`. The response is not compared with the working tree, because a consumer holds only the export.
jqr '.sources[]? | select(.publishing.target == "github") | .shortcode' "$EXPORT_MANIFEST" \
  > "$WORK/sources.txt"

while read -r code; do
  [ -n "$code" ] || continue

  SRC_BASE=$(jqr --arg c "$code" '.sources[] | select(.shortcode == $c) | .publishing.base // empty' \
    "$EXPORT_MANIFEST")
  SRC_PREFIX=$(jqr --arg c "$code" \
    '.sources[] | select(.shortcode == $c) | .publishing.pathPrefix // empty' "$EXPORT_MANIFEST")
  SRC_REF=$(jqr --arg c "$code" '.sources[] | select(.shortcode == $c) | .publishing.ref // empty' \
    "$EXPORT_MANIFEST")

  if [ -z "$SRC_BASE" ] || [ -z "$SRC_REF" ]; then
    echo "round-trip: source '$code' states no base or no ref, so no fetch is checked for it."
    continue
  fi

  INHERITED=$(find "$ROOT/$CORPUS_ROOT" -type d -name "$code" | sort | head -1)
  [ -n "$INHERITED" ] || fail "'$code' is a source and no directory holds the records it published."

  RECORD=$(find "$INHERITED" -type f -name '*.json' | sort | head -1)
  [ -n "$RECORD" ] || fail "'$code' is a source and $INHERITED holds no record."

  RECORD_PATH=$(jqr '.path' "$RECORD")

  SRC_RAW=$(echo "$SRC_BASE" | sed 's|https://github.com/|https://raw.githubusercontent.com/|')
  SRC_SEG=""
  if [ -n "$SRC_PREFIX" ]; then
    SRC_SEG="/$SRC_PREFIX"
  fi

  URL="$SRC_RAW/$SRC_REF$SRC_SEG/$RECORD_PATH"

  echo "round-trip: fetching $URL"
  curl -sS --fail --location --output "$WORK/inherited.md" "$URL" \
    || fail "'$code' in sources built a URL that does not fetch: $URL"

  [ -s "$WORK/inherited.md" ] || fail "'$code' built a URL that fetched an empty file: $URL"

  # The producer resolved its own link when it exported, so that address and the one rebuilt from `sources` meet here.
  HUMAN=$(jqr '.links.human // empty' "$RECORD")
  case "$HUMAN" in
    "$SRC_BASE"*) ;;
    "") fail "the inherited record $RECORD carries no resolved link." ;;
    *) fail "$RECORD is published under '$HUMAN' and 'sources' names '$SRC_BASE'." ;;
  esac

  echo "round-trip: '$code' fetched a record from the corpus that published it."
done < "$WORK/sources.txt"

# Every session reads the breadcrumb, so a wrong count in it is the one nobody checks. A merged export credits each
# corpus beside its own count. Read from the installed copy, because the hook prints a file `bundle` wrote. A plugin
# whose hook was trimmed ships none, which is reported rather than failed.
BREADCRUMB="$ROOT/hooks/breadcrumb.txt"

if [ -f "$BREADCRUMB" ]; then
  grep -qF "travels with this session as data" "$BREADCRUMB" \
    || fail "the breadcrumb does not open by naming the corpus that travels."

  echo "round-trip: the breadcrumb names the corpus travelling with the session."

  jqr '.sources[]? | .shortcode' "$EXPORT_MANIFEST" > "$WORK/credited.txt"

  while read -r code; do
    [ -n "$code" ] || continue

    # A corpus reaches `sources` for its address whether or not this export counts anything of its own. A line
    # counts a corpus only where the export holds a directory of its records or a part line naming it, so the
    # export is asked first.
    if ! find "$ROOT/$CORPUS_ROOT" -type d -name "$code" | grep -q . \
      && ! grep -rq "\"shortcode\": *\"$code\"" "$ROOT/$CORPUS_ROOT"; then
      echo "round-trip: '$code' sent nothing this export counts, so no line names it."
      continue
    fi

    grep -qF "(from $code)" "$BREADCRUMB" \
      || fail "'$code' wrote records this export carries and the breadcrumb credits none to it."

    echo "round-trip: the breadcrumb credits $code with the records it wrote."
  done < "$WORK/credited.txt"
else
  echo "round-trip: this plugin ships no hook, so there is no breadcrumb to read."
fi

# The build-index hook prints a file `bundle` wrote, and Claude Code passes at most 10,000 characters of it to the
# session. Run twice for one session, the installed script prints it once, keyed by a marker in a data dir of its own.
BUILD_INDEX="$ROOT/hooks/build-index.json"

if [ -f "$BUILD_INDEX" ]; then
  CONTEXT_LENGTH=$(jqr '.hookSpecificOutput.additionalContext | length' "$BUILD_INDEX")
  [ "$CONTEXT_LENGTH" -gt 0 ] && [ "$CONTEXT_LENGTH" -le 10000 ] \
    || fail "the build-index hook pushes $CONTEXT_LENGTH characters, and Claude Code passes 1 to 10,000."

  echo '{"session_id":"round-trip-session","hook_event_name":"PreToolUse","tool_name":"Read"}' > "$WORK/hook-input.json"
  FIRST=$(CLAUDE_PLUGIN_DATA="$WORK/plugin-data" "$ROOT/hooks/build-index" < "$WORK/hook-input.json")
  BEFORE=$(now_ms)
  SECOND=$(CLAUDE_PLUGIN_DATA="$WORK/plugin-data" "$ROOT/hooks/build-index" < "$WORK/hook-input.json")
  AFTER=$(now_ms)

  [ "$FIRST" = "$(cat "$BUILD_INDEX")" ] \
    || fail "the build-index hook did not print its output on a session's first call."
  [ -z "$SECOND" ] || fail "the build-index hook printed its output a second time in one session."

  echo "round-trip: the build-index hook pushes $CONTEXT_LENGTH characters, once a session."
  echo "round-trip: a later call in that session took $(elapsed "$BEFORE" "$AFTER")."

  # Every tool call runs the hook, and a Write call's input carries the whole file.
  printf '{"session_id":"round-trip-session","tool_input":{"content":"%s"}}\n' \
    "$(head -c 200000 /dev/zero | tr '\0' a)" > "$WORK/hook-input-large.json"
  BEFORE=$(now_ms)
  CLAUDE_PLUGIN_DATA="$WORK/plugin-data" "$ROOT/hooks/build-index" < "$WORK/hook-input-large.json" > /dev/null
  AFTER=$(now_ms)
  echo "round-trip: a later call with a 200 KB input took $(elapsed "$BEFORE" "$AFTER")."
else
  echo "round-trip: this plugin ships no build index, so the build-index hook prints nothing."
fi

# Everything above is read from the manifests and holds for any corpus. Each lookup below is written about the corpus
# holding the records its skill was written for.

line_of() {
  grep -n "\"id\": *\"$2\"" "$1" | cut -d: -f1
}

glossary_lookup() {
  TERMS="$ROOT/$CORPUS_ROOT/glossary/terms.jsonl"

  [ -f "$TERMS" ] || fail "this corpus proves the glossary skill and its export carries no glossary."

  echo "round-trip: terms reachable at $CORPUS_ROOT/glossary/terms.jsonl inside the installed plugin"

  # A search for the term's title. ` *` admits any spacing after the colon, which the export does not fix.
  grep -iE '"title": *"Borrower"' "$TERMS" > "$WORK/hits.txt" || fail "no line defines Borrower."

  HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line defining Borrower, found $HITS."

  ID=$(jqr '.id' "$WORK/hits.txt")
  [ "$ID" = "gls-example-libraries.borrower" ] || fail "Borrower came back as $ID."

  DEFINITION=$(jqr '.definition // empty' "$WORK/hits.txt")
  [ -n "$DEFINITION" ] || fail "$ID came back with no definition."

  # The skill tells a reader to give the `not` line beside the definition.
  NOT=$(jqr '.not // empty' "$WORK/hits.txt")
  [ -n "$NOT" ] || fail "$ID came back with no Not line."

  echo "round-trip: $ID — definition and Not line both present."

  # `ExporterTests` pins the ordering over corpora it builds. This asserts it over the corpus a reader receives.
  # `gls-search` narrows `gls-example-libraries` and both define `title`, so a grep meets the general entry first.
  GENERAL=$(line_of "$TERMS" gls-example-libraries.title)
  NARROWER=$(line_of "$TERMS" gls-search.title)

  [ -n "$GENERAL" ] || fail "gls-example-libraries.title is not in the export."
  [ -n "$NARROWER" ] || fail "gls-search.title is not in the export."
  [ "$GENERAL" -lt "$NARROWER" ] || fail "gls-search.title came before the entry it narrows."

  echo "round-trip: gls-example-libraries.title precedes gls-search.title, as the chain requires."

  # Across roots, only that the order does not move. Neither glossary narrows the other, so asserting that the first
  # is the more general would hand a reader the wrong domain.
  FIRST=$(line_of "$TERMS" gls-example-libraries.record)
  SECOND=$(line_of "$TERMS" gls-knowledge-as-code.record)

  [ -n "$FIRST" ] || fail "gls-example-libraries.record is not in the export."
  [ -n "$SECOND" ] || fail "gls-knowledge-as-code.record is not in the export."
  [ "$FIRST" -lt "$SECOND" ] || fail "the order of the two record entries moved."

  echo "round-trip: the two record entries come back in a stable order, and neither is claimed the general one."
}

policy_lookup() {
  CLAUSES="$ROOT/$CORPUS_ROOT/policies/clauses.jsonl"

  [ -f "$CLAUSES" ] || fail "this corpus proves the policy skill and its export carries no policies."

  echo "round-trip: clauses reachable at $CORPUS_ROOT/policies/clauses.jsonl inside the installed plugin"

  # A search for the subject's stem. `timeframe` is the stem `timeframes` in pol-VURM sits on.
  grep -iE 'timeframe' "$CLAUSES" > "$WORK/subject.txt" || fail "no clause mentions a timeframe."

  grep -qE '"record": *"pol-VURM"' "$WORK/subject.txt" \
    || fail "a search for the subject reached no clause of pol-VURM."

  # Every clause of the policy the first hit named.
  grep -E '"record": *"pol-VURM"' "$CLAUSES" > "$WORK/clauses.txt" \
    || fail "no clause belongs to pol-VURM."

  grep -E '"id": *"pol-VURM.TIMEBOX"' "$WORK/clauses.txt" > "$WORK/hits.txt" \
    || fail "pol-VURM carries no TIMEBOX clause."

  HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for pol-VURM.TIMEBOX, found $HITS."

  WORDING=$(jqr '.clause // empty' "$WORK/hits.txt")
  [ -n "$WORDING" ] || fail "pol-VURM.TIMEBOX came back with no clause."

  # Without `level` a skill can only guess whether the estate committed to the clause or merely suggested it.
  LEVEL=$(jqr '.level // empty' "$WORK/hits.txt")
  [ "$LEVEL" = "MUST" ] || fail "pol-VURM.TIMEBOX came back at level '$LEVEL' rather than MUST."

  echo "round-trip: pol-VURM.TIMEBOX binds at $LEVEL, and the wording came with it."

  # `MUST NOT` opens with `MUST`, so asserting the longer level whole tells a real `level` field from one rebuilt by
  # matching the first word.
  grep -E '"id": *"pol-VURM.SHIP"' "$CLAUSES" > "$WORK/prohibition.txt" \
    || fail "pol-VURM carries no SHIP clause."

  HITS=$(wc -l < "$WORK/prohibition.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for pol-VURM.SHIP, found $HITS."

  LEVEL=$(jqr '.level // empty' "$WORK/prohibition.txt")
  [ "$LEVEL" = "MUST NOT" ] || fail "pol-VURM.SHIP came back at level '$LEVEL' rather than MUST NOT."

  echo "round-trip: pol-VURM.SHIP binds at $LEVEL, whole rather than shortened to its first word."

  # A policy's clauses come back in the order its table wrote them, so a reader can quote them as the policy reads.
  FIRST=$(line_of "$CLAUSES" pol-VURM.SCAN)
  LAST=$(line_of "$CLAUSES" pol-VURM.INDEP)

  [ -n "$FIRST" ] || fail "pol-VURM.SCAN is not in the export."
  [ -n "$LAST" ] || fail "pol-VURM.INDEP is not in the export."
  [ "$FIRST" -lt "$LAST" ] || fail "the clauses of pol-VURM came back out of table order."

  echo "round-trip: pol-VURM.SCAN precedes pol-VURM.INDEP, as the clause table has them."

  # The skill sends a reader on to the record for the three sections saying what the clause binds.
  RECORD=$(jqr '.record' "$WORK/hits.txt")
  POLICY="$ROOT/$CORPUS_ROOT/policies/$RECORD.json"

  [ -f "$POLICY" ] || fail "$RECORD.json is not beside the clause table."

  for section in Purpose Scope Exceptions; do
    BODY=$(jqr --arg s "$section" '.sections[$s] // empty' "$POLICY")
    [ -n "$BODY" ] || fail "$RECORD.json carries no $section for the clause to be read against."
  done

  echo "round-trip: $RECORD.json carries Purpose, Scope and Exceptions."
}

standards_lookup() {
  RULES="$ROOT/$CORPUS_ROOT/standards/rules.jsonl"

  [ -f "$RULES" ] || fail "this corpus proves the standards skill and its export carries no standards."

  echo "round-trip: rules reachable at $CORPUS_ROOT/standards/rules.jsonl inside the installed plugin"

  # A search for the subject's stem. `idempot` is the stem `idempotency` and `idempotent` sit on.
  grep -iE 'idempot' "$RULES" > "$WORK/subject.txt" || fail "no rule mentions idempotency."

  grep -qE '"record": *"std-IDEM"' "$WORK/subject.txt" \
    || fail "a search for the subject reached no rule of std-IDEM."

  # Every rule of the standard the first hit named.
  grep -E '"record": *"std-IDEM"' "$RULES" > "$WORK/rules.txt" \
    || fail "no rule belongs to std-IDEM."

  grep -E '"id": *"std-IDEM.KEY"' "$WORK/rules.txt" > "$WORK/hits.txt" \
    || fail "std-IDEM carries no KEY rule."

  HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for std-IDEM.KEY, found $HITS."

  OBLIGATIONS=$(jqr '.obligations[]? | select(.level == "MUST") | .text' "$WORK/hits.txt")
  [ -n "$OBLIGATIONS" ] || fail "std-IDEM.KEY came back with no MUST obligation."

  # The level is read from a key, and the text still travels as the markdown the standard wrote.
  echo "$OBLIGATIONS" | grep -qF '**MUST**' \
    || fail "std-IDEM.KEY came back with a MUST whose text lost its bold."

  echo "round-trip: std-IDEM.KEY carries its obligations, level and all."

  # `MUST NOT` opens with `MUST`, so a rule holding a prohibition shows the longer level survives the export whole.
  grep -E '"id": *"std-IDEM.WAIT"' "$RULES" \
    > "$WORK/prohibition.txt" || fail "std-IDEM carries no WAIT rule."

  jqr '.obligations[]? | select(.level == "MUST NOT") | .text' "$WORK/prohibition.txt" | grep -qF '**MUST NOT**' \
    || fail "std-IDEM.WAIT came back with no MUST NOT."

  echo "round-trip: a prohibition arrives as MUST NOT, whole rather than shortened to its first word."

  # A rule is cited by the id its heading opens with, and linked at the slug of the whole heading, so a link built from
  # the template lands on the rule and not on the standard.
  ANCHOR=$(jqr '.anchor' "$WORK/hits.txt")
  PART=$(jqr '.part' "$WORK/hits.txt")

  [ "$PART" = "KEY" ] || fail "the rule's part is '$PART', where its heading opens with 'KEY'."
  [ "$ANCHOR" = "key-the-caller-chooses-the-key" ] \
    || fail "the rule's anchor is '$ANCHOR', where its heading slugs to 'key-the-caller-chooses-the-key'."

  echo "round-trip: a rule's part is its id, and its anchor is the slug of its whole heading."

  # A heading-sourced type sorts on the heading's text, without its id. The standard writes this pair the other way
  # round, and the ids sort the other way too, which is what makes the assertion say something.
  FIRST=$(line_of "$RULES" std-IDEM.WAIT)
  LAST=$(line_of "$RULES" std-IDEM.KEY)

  [ -n "$FIRST" ] || fail "std-IDEM.WAIT is not in the export."
  [ -n "$LAST" ] || fail "std-IDEM.KEY is not in the export."
  [ "$FIRST" -lt "$LAST" ] || fail "the rules of std-IDEM came back out of heading order."

  echo "round-trip: the rules of std-IDEM come back sorted on the heading's text."

  # The link's target is stripped out of the words, so `seeAlso` is where the id survives. `std-RECON` points at a
  # rule of `std-LEDGER`.
  grep -E '"id": *"std-RECON.HOLD"' "$RULES" \
    > "$WORK/refers.txt" || fail "std-RECON carries no HOLD rule."

  SEEALSO=$(jqr '.seeAlso // [] | join(",")' "$WORK/refers.txt")
  [ "$SEEALSO" = "std-LEDGER.NOAMEND" ] \
    || fail "std-RECON.HOLD points at '$SEEALSO'."

  echo "round-trip: a rule carries the other standard's rule it leans on, as a part id."

  # The skill sends a reader on to the record for the two sections saying what the rule is for and how it is shown.
  RECORD=$(jqr '.record' "$WORK/hits.txt")
  STANDARD="$ROOT/$CORPUS_ROOT/standards/$RECORD.json"

  [ -f "$STANDARD" ] || fail "$RECORD.json is not beside the rules file."

  for section in Summary "Conformance checklist"; do
    BODY=$(jqr --arg s "$section" '.sections[$s] // empty' "$STANDARD")
    [ -n "$BODY" ] || fail "$RECORD.json carries no $section for the rule to be read against."
  done

  echo "round-trip: $RECORD.json carries Summary and Conformance checklist."

  # payments consumes engineering, so one search reaches both corpora's rules, each under the shortcode of the corpus
  # that wrote it. A rule filed under a bare id would read as this estate's own.
  grep -E '"shortcode": *"eng"' "$RULES" > "$WORK/inherited-rules.txt" \
    || fail "this corpus consumes eng and no rule in its export names eng as the producer."

  head -1 "$WORK/inherited-rules.txt" > "$WORK/inherited-rule.txt"

  INHERITED_ID=$(jqr '.id' "$WORK/inherited-rule.txt")
  INHERITED_RECORD=$(jqr '.record' "$WORK/inherited-rule.txt")

  case "$INHERITED_ID" in
    eng:*) ;;
    *) fail "a rule eng wrote arrived as '$INHERITED_ID', which names no producer." ;;
  esac

  case "$INHERITED_RECORD" in
    eng:*) ;;
    *) fail "a rule eng wrote belongs to record '$INHERITED_RECORD', which names no producer." ;;
  esac

  echo "round-trip: an engineering rule arrived in payments' own file, carrying eng: on its id."

  # eng publishes under its own prefix, and this corpus's prefix names a directory of different files. Both assemble
  # into a URL that looks right, so only the fetch tells them apart.
  ENG_TARGET=$(jqr '.sources[] | select(.shortcode == "eng") | .publishing.target // empty' "$EXPORT_MANIFEST")
  ENG_BASE=$(jqr '.sources[] | select(.shortcode == "eng") | .publishing.base // empty' "$EXPORT_MANIFEST")
  ENG_PREFIX=$(jqr '.sources[] | select(.shortcode == "eng") | .publishing.pathPrefix // empty' "$EXPORT_MANIFEST")
  ENG_REF=$(jqr '.sources[] | select(.shortcode == "eng") | .publishing.ref // empty' "$EXPORT_MANIFEST")
  OWN_PREFIX=$(jqr '.publishing.pathPrefix // empty' "$EXPORT_MANIFEST")

  [ -n "$ENG_PREFIX" ] || fail "eng is a source and states no path prefix for its standards."
  [ "$ENG_PREFIX" != "$OWN_PREFIX" ] \
    || fail "eng and this corpus both publish under '$OWN_PREFIX', so this proves nothing."

  # Only a public GitHub repository answers without credentials.
  [ "$ENG_TARGET" = "github" ] || {
    echo "round-trip: eng publishes to $ENG_TARGET, which no unauthenticated fetch reaches."
    return 0
  }

  RULE_PATH=$(jqr '.path' "$WORK/inherited-rule.txt")
  RULE_TITLE=$(jqr '.title' "$WORK/inherited-rule.txt")

  ENG_RAW=$(echo "$ENG_BASE" | sed 's|https://github.com/|https://raw.githubusercontent.com/|')
  URL="$ENG_RAW/$ENG_REF/$ENG_PREFIX/$RULE_PATH"

  echo "round-trip: fetching $URL"
  curl -sS --fail --location --output "$WORK/inherited-rule.md" "$URL" \
    || fail "the sources entry for eng built a URL that does not fetch: $URL"

  grep -qF "$RULE_TITLE" "$WORK/inherited-rule.md" \
    || fail "$URL fetched a file that does not carry the rule '$RULE_TITLE'."

  echo "round-trip: $INHERITED_ID reads at eng's commit, under eng's path prefix."
}

CORPUS=$(jqr '.corpus' "$EXPORT_MANIFEST")

case "$CORPUS" in
  example-libraries)
    glossary_lookup
    ;;
  example-engineering)
    policy_lookup
    ;;
  example-payments)
    standards_lookup
    ;;
  *)
    # A corpus exporting any of these types and reaching here was renamed out of the arms above, and would otherwise
    # pass having asked the skill nothing.
    for parts in glossary/terms.jsonl policies/clauses.jsonl standards/rules.jsonl; do
      if [ -f "$ROOT/$CORPUS_ROOT/$parts" ]; then
        fail "$CORPUS exports $parts and no arm above reads it. Name this corpus in one of them."
      fi
    done

    echo "round-trip: no lookup is written for $CORPUS. The install, the components and the links are what it proves."
    ;;
esac

echo "round-trip: passed."
