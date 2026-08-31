#!/usr/bin/env sh
# round-trip — install the assembled plugin and ask each of its skills what that skill describes.
#
# Everything else about the export and the bundle is proved over data: the golden fixtures diff the
# tree file for file, and the unit tests pin the rules that built it. None of them can show the chain
# end to end. A bundle installs, the components it kept arrive and the ones it trimmed do not, the
# paths a skill names resolve inside the installed copy, and a link built from its template fetches
# the record it points at.
#
# Run it from the corpus root, after `kac export` and `kac bundle`:
#
#   cd examples/library && sh ../../tooling/tests/round-trip.sh
#
# The corpus it runs in decides which lookup it performs. `example-libraries` proves the glossary
# skill, `example-engineering` proves the policy skill and `example-payments` proves the standards
# skill, because each holds the records that skill was written for.
#
# It reads `.dist/`, writes only inside a work directory of its own, and installs into a Claude
# config directory of its own, so it leaves the machine it ran on as it found it.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process
# substitution. Command substitution is used freely, because a file on disk reaches the shell
# unparsed.
#
# Three prerequisites, all already on a GitHub runner: `jq` reads the manifests, `curl` fetches a
# record's source, and the Claude Code CLI installs the plugin.

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

# ── the components the bundle kept, and the ones it trimmed ───────────────────────────────────────
#
# One plugin tree serves corpora that adopted different types, and the trim is what fits it to each of
# them. A component that survives when its type stayed behind is a skill pointing at files the export
# never wrote, and an agent reads an empty search as an answer.
#
# `bundle.json` travels inside the plugin, so both lists are read from the installed copy. The corpus
# a run happens to be in decides which of the two cases it proves: a corpus adopting one of the two
# types proves the trim, and a corpus adopting both proves that neither skill crowds the other out.

BUNDLE="$ROOT/bundle.json"
[ -f "$BUNDLE" ] || fail "the installed plugin holds no bundle.json."

jqr '.included[].path' "$BUNDLE" > "$WORK/included.txt"
jqr '.trimmed[].path' "$BUNDLE" > "$WORK/trimmed.txt"

# Two lists read as empty leave every assertion below true of nothing and still close on a pass. Two
# ways to arrive there: jq fails, which `jqr` cannot report because POSIX sh has no `pipefail`, or a
# key is renamed, which jq answers with `null` rather than an error. Asking for the type catches both,
# because `select` on a false condition writes nothing at all.
COMPONENTS=$(jqr 'select((.included | type) == "array" and (.trimmed | type) == "array")
  | (.included | length) + (.trimmed | length)' "$BUNDLE")
[ -n "$COMPONENTS" ] && [ "$COMPONENTS" -gt 0 ] \
  || fail "bundle.json names no component under included and trimmed."

while read -r component; do
  [ -n "$component" ] || continue
  [ -e "$ROOT/$component" ] || fail "bundle.json kept $component and the installed plugin does not hold it."
  echo "round-trip: $component installed."
done < "$WORK/included.txt"

while read -r component; do
  [ -n "$component" ] || continue
  if [ -e "$ROOT/$component" ]; then
    fail "$component was trimmed and the installed plugin holds it anyway."
  fi
  echo "round-trip: $component trimmed, and absent from the install."
done < "$WORK/trimmed.txt"

# ── each skill reads its own type and no other ────────────────────────────────────────────────────
#
# Two skills over one export is the case a corpus adopting both types produces. Each names the parts
# file of the type it declares in `requires`. Naming another type's would send its reader to a file
# written for a different question, and the reader has no way to tell.
#
# Skills only. A hook renders its text when the bundle is assembled and searches nothing.

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

# ── a link built from the template, and fetched ───────────────────────────────────────────────────
#
# The one assertion here that leaves the machine. A template with the wrong host, the wrong ref or a
# path prefix that no longer exists assembles into a string that matches any pattern you would write
# for it and fetches a 404, or a page from a version of the corpus nobody asked about. Only the fetch
# tells those apart, and comparing the response against the file in the working tree is what says the
# template addressed this corpus at this commit.
#
# Written to outlive glossary. It takes one record's `path` from each type the export declares, so a
# corpus that adopts a second type brings its records under the same check without a line changing
# here. An ADR needs this more than a term does: a term line carries its whole content, and a broken
# link costs the reader nothing, where an ADR's source is the only route an agent has to the text.
#
# The URL is assembled here rather than read from the manifest. An export carries the base, the path
# prefix and the ref an agent fetches with, and no raw template, because only a public GitHub
# repository can be fetched without credentials. Assembling it is therefore the same work a consumer
# does, over the same three values, which is what makes the fetch worth running.

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
    URL="$RAW_BASE/$PUB_REF$PREFIX_SEG/$RECORD_PATH"

    echo "round-trip: fetching $URL"
    curl -sS --fail --location --output "$WORK/fetched.md" "$URL" \
      || fail "the base, prefix and ref built a URL that does not fetch: $URL"

    cmp -s "$WORK/fetched.md" "$REPO/$RECORD_PATH" \
      || fail "the URL fetched something other than $RECORD_PATH as this corpus holds it."

    echo "round-trip: $dir — the fetched source matched the record."
  done < "$WORK/dirs.txt"
fi

# ── the lookup each skill describes ───────────────────────────────────────────────────────────────
#
# Everything above is read from the manifests, so it holds for any corpus. What a skill tells a reader
# to search for is content, and content belongs to one corpus. Each function below is written about
# the corpus that holds the records its skill was written for.

# The line a term or a clause arrived on. Ordering is a property of the file rather than of any one
# entry, so it is asked of the file.
line_of() {
  grep -n "\"id\": *\"$2\"" "$1" | cut -d: -f1
}

# What the plugin exists for: a term searched for by the skill's own pattern comes back carrying the
# two things the skill tells a reader to give.
glossary_lookup() {
  TERMS="$ROOT/$CORPUS_ROOT/glossary/terms.jsonl"

  [ -f "$TERMS" ] || fail "this corpus proves the glossary skill and its export carries no glossary."

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

  # What the order of the file means, and what it does not. `ExporterTests` pins these rules over
  # corpora it builds for the purpose. This asserts them over the corpus a reader actually receives,
  # which is the only place the two could have come apart.

  # The chain, which is the case the ordering exists for. `gls-search` narrows `gls-example-libraries`
  # and both define `title`, so a grep meets the general entry before the one refining it.
  GENERAL=$(line_of "$TERMS" gls-example-libraries.title)
  NARROWER=$(line_of "$TERMS" gls-search.title)

  [ -n "$GENERAL" ] || fail "gls-example-libraries.title is not in the export."
  [ -n "$NARROWER" ] || fail "gls-search.title is not in the export."
  [ "$GENERAL" -lt "$NARROWER" ] || fail "gls-search.title came before the entry it narrows."

  echo "round-trip: gls-example-libraries.title precedes gls-search.title, as the chain requires."

  # Across roots, only that the order does not move. `record` is a bibliographic record in one glossary
  # and a knowledge document in the other; neither narrows the other, and nothing ranks them. This is
  # deliberately not an assertion that the first is the more general — reading it that way is what would
  # hand a reader the wrong domain, so the absence of that assertion is the point rather than an
  # oversight.
  FIRST=$(line_of "$TERMS" gls-example-libraries.record)
  SECOND=$(line_of "$TERMS" gls-knowledge-as-code.record)

  [ -n "$FIRST" ] || fail "gls-example-libraries.record is not in the export."
  [ -n "$SECOND" ] || fail "gls-knowledge-as-code.record is not in the export."
  [ "$FIRST" -lt "$SECOND" ] || fail "the order of the two record entries moved."

  echo "round-trip: the two record entries come back in a stable order, and neither is claimed the general one."
}

# The same question of the policy skill. A clause is not a term: its wording is only half the answer,
# and `level` is the half the reader acts on.
policy_lookup() {
  CLAUSES="$ROOT/$CORPUS_ROOT/policies/clauses.jsonl"

  [ -f "$CLAUSES" ] || fail "this corpus proves the policy skill and its export carries no policies."

  echo "round-trip: clauses reachable at $CORPUS_ROOT/policies/clauses.jsonl inside the installed plugin"

  # The skill's first pattern: the subject in the words the estate would use. It tells a reader to
  # search the stem, and `timeframe` is the stem `timeframes` in pol-VURM sits on.
  grep -iE 'timeframe' "$CLAUSES" > "$WORK/subject.txt" || fail "no clause mentions a timeframe."

  grep -qE '"record": *"pol-VURM"' "$WORK/subject.txt" \
    || fail "a search for the subject reached no clause of pol-VURM."

  # The skill's second pattern, collecting one policy once a first hit has named it.
  grep -E '"record": *"pol-VURM"' "$CLAUSES" > "$WORK/clauses.txt" \
    || fail "no clause belongs to pol-VURM."

  grep -E '"id": *"pol-VURM.TIMEBOX"' "$WORK/clauses.txt" > "$WORK/hits.txt" \
    || fail "pol-VURM carries no TIMEBOX clause."

  HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for pol-VURM.TIMEBOX, found $HITS."

  WORDING=$(jqr '.clause // empty' "$WORK/hits.txt")
  [ -n "$WORDING" ] || fail "pol-VURM.TIMEBOX came back with no clause."

  # The field the answer turns on. A skill handed a clause without it can only guess whether the
  # estate committed to the thing or merely suggested it.
  LEVEL=$(jqr '.level // empty' "$WORK/hits.txt")
  [ "$LEVEL" = "MUST" ] || fail "pol-VURM.TIMEBOX came back at level '$LEVEL' rather than MUST."

  echo "round-trip: pol-VURM.TIMEBOX binds at $LEVEL, and the wording came with it."

  # `MUST NOT` opens with `MUST`, so a level read out of the wording files a prohibition as an
  # obligation. Asserting the longer of the two whole is what tells a real `level` field from one
  # rebuilt by matching the first word.
  grep -E '"id": *"pol-VURM.SHIP"' "$CLAUSES" > "$WORK/prohibition.txt" \
    || fail "pol-VURM carries no SHIP clause."

  HITS=$(wc -l < "$WORK/prohibition.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for pol-VURM.SHIP, found $HITS."

  LEVEL=$(jqr '.level // empty' "$WORK/prohibition.txt")
  [ "$LEVEL" = "MUST NOT" ] || fail "pol-VURM.SHIP came back at level '$LEVEL' rather than MUST NOT."

  echo "round-trip: pol-VURM.SHIP binds at $LEVEL, whole rather than shortened to its first word."

  # The clauses of one policy come back in the order its table wrote them, which is what lets a reader
  # quote them as the policy reads.
  FIRST=$(line_of "$CLAUSES" pol-VURM.SCAN)
  LAST=$(line_of "$CLAUSES" pol-VURM.INDEP)

  [ -n "$FIRST" ] || fail "pol-VURM.SCAN is not in the export."
  [ -n "$LAST" ] || fail "pol-VURM.INDEP is not in the export."
  [ "$FIRST" -lt "$LAST" ] || fail "the clauses of pol-VURM came back out of table order."

  echo "round-trip: pol-VURM.SCAN precedes pol-VURM.INDEP, as the clause table has them."

  # The skill sends a reader from the clause to the record beside it for the three sections that say
  # what the clause binds. A clause read without them is stricter than the one we wrote.
  RECORD=$(jqr '.record' "$WORK/hits.txt")
  POLICY="$ROOT/$CORPUS_ROOT/policies/$RECORD.json"

  [ -f "$POLICY" ] || fail "$RECORD.json is not beside the clause table."

  for section in Purpose Scope Exceptions; do
    BODY=$(jqr --arg s "$section" '.sections[$s] // empty' "$POLICY")
    [ -n "$BODY" ] || fail "$RECORD.json carries no $section for the clause to be read against."
  done

  echo "round-trip: $RECORD.json carries Purpose, Scope and Exceptions."
}

# The same question of the standards skill. A rule is neither a term nor a clause: one line holds
# several obligations, and the keyword that says whether each binds is written inside the wording.
standards_lookup() {
  RULES="$ROOT/$CORPUS_ROOT/standards/rules.jsonl"

  [ -f "$RULES" ] || fail "this corpus proves the standards skill and its export carries no standards."

  echo "round-trip: rules reachable at $CORPUS_ROOT/standards/rules.jsonl inside the installed plugin"

  # The skill's first pattern: the subject in the words the estate would use. It tells a reader to
  # search the stem, and `idempot` is the stem `idempotency` and `idempotent` both sit on.
  grep -iE 'idempot' "$RULES" > "$WORK/subject.txt" || fail "no rule mentions idempotency."

  grep -qE '"record": *"std-IDEM"' "$WORK/subject.txt" \
    || fail "a search for the subject reached no rule of std-IDEM."

  # The skill's second pattern, collecting one standard once a first hit has named it.
  grep -E '"record": *"std-IDEM"' "$RULES" > "$WORK/rules.txt" \
    || fail "no rule belongs to std-IDEM."

  grep -E '"id": *"std-IDEM.the-caller-chooses-the-key"' "$WORK/rules.txt" > "$WORK/hits.txt" \
    || fail "std-IDEM carries no the-caller-chooses-the-key rule."

  HITS=$(wc -l < "$WORK/hits.txt" | tr -d ' ')
  [ "$HITS" = "1" ] || fail "expected one line for std-IDEM.the-caller-chooses-the-key, found $HITS."

  OBLIGATIONS=$(jqr '.obligations // empty' "$WORK/hits.txt")
  [ -n "$OBLIGATIONS" ] || fail "std-IDEM.the-caller-chooses-the-key came back with no obligations."

  # The markdown is what carries the keyword, so it travels unflattened. A reader handed the bullets
  # with the emphasis stripped cannot tell an obligation from the prose around it.
  echo "$OBLIGATIONS" | grep -qF '**MUST**' \
    || fail "std-IDEM.the-caller-chooses-the-key came back with no bold MUST in its obligations."

  echo "round-trip: std-IDEM.the-caller-chooses-the-key carries its obligations, keyword and all."

  # `MUST NOT` opens with `MUST`, and the skill tells a reader to compare the keyword whole. A rule
  # holding a prohibition is what shows the longer keyword survives the export unshortened.
  grep -E '"id": *"std-IDEM.an-in-flight-repeat-waits-or-is-told-to-wait"' "$RULES" \
    > "$WORK/prohibition.txt" || fail "std-IDEM carries no an-in-flight-repeat-waits-or-is-told-to-wait rule."

  jqr '.obligations // empty' "$WORK/prohibition.txt" | grep -qF '**MUST NOT**' \
    || fail "std-IDEM.an-in-flight-repeat-waits-or-is-told-to-wait came back with no bold MUST NOT."

  echo "round-trip: a prohibition arrives as MUST NOT, whole rather than shortened to its first word."

  # What a heading-sourced part gives that a table row cannot. A clause resolves to the section
  # holding its table, and a rule resolves to itself, so a link built from the template lands on the
  # rule rather than at the top of the standard.
  ANCHOR=$(jqr '.anchor' "$WORK/hits.txt")
  PART=$(jqr '.part' "$WORK/hits.txt")

  [ "$ANCHOR" = "$PART" ] || fail "the rule's anchor is '$ANCHOR' and its part is '$PART'."

  echo "round-trip: a rule addresses itself, so its anchor is its own key."

  # A heading-sourced type sorts on the heading, so a grep meets a rule where somebody looking down a
  # list of names would find it. The standard writes this pair the other way round, which is what makes
  # the assertion say something: a reader wanting the author's order opens the record.
  FIRST=$(line_of "$RULES" std-IDEM.an-in-flight-repeat-waits-or-is-told-to-wait)
  LAST=$(line_of "$RULES" std-IDEM.the-caller-chooses-the-key)

  [ -n "$FIRST" ] || fail "std-IDEM.an-in-flight-repeat-waits-or-is-told-to-wait is not in the export."
  [ -n "$LAST" ] || fail "std-IDEM.the-caller-chooses-the-key is not in the export."
  [ "$FIRST" -lt "$LAST" ] || fail "the rules of std-IDEM came back out of heading order."

  echo "round-trip: the rules of std-IDEM come back sorted on the heading."

  # A rule leans on another standard's rule, and composition is what makes that ordinary. The link is
  # what states the reference, and its target is stripped out of the words, so `seeAlso` is where the
  # id survives. `std-RECON` points at a rule of `std-LEDGER`.
  grep -E '"id": *"std-RECON.nothing-downstream-reads-an-unreconciled-day"' "$RULES" \
    > "$WORK/refers.txt" || fail "std-RECON carries no nothing-downstream-reads-an-unreconciled-day rule."

  SEEALSO=$(jqr '.seeAlso // [] | join(",")' "$WORK/refers.txt")
  [ "$SEEALSO" = "std-LEDGER.nothing-amends-an-entry" ] \
    || fail "std-RECON.nothing-downstream-reads-an-unreconciled-day points at '$SEEALSO'."

  echo "round-trip: a rule carries the other standard's rule it leans on, as a part id."

  # The skill sends a reader from the rule to the record beside it for the two sections that say what
  # the rule is for and how anyone shows the work meets it.
  RECORD=$(jqr '.record' "$WORK/hits.txt")
  STANDARD="$ROOT/$CORPUS_ROOT/standards/$RECORD.json"

  [ -f "$STANDARD" ] || fail "$RECORD.json is not beside the rules file."

  for section in Summary "Conformance checklist"; do
    BODY=$(jqr --arg s "$section" '.sections[$s] // empty' "$STANDARD")
    [ -n "$BODY" ] || fail "$RECORD.json carries no $section for the rule to be read against."
  done

  echo "round-trip: $RECORD.json carries Summary and Conformance checklist."
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
    # A corpus exporting none of these types has no lookup to perform. One exporting any of them and
    # reaching here has been renamed out of the arms above, and would otherwise close on a pass having
    # asked the skill nothing.
    for parts in glossary/terms.jsonl policies/clauses.jsonl standards/rules.jsonl; do
      if [ -f "$ROOT/$CORPUS_ROOT/$parts" ]; then
        fail "$CORPUS exports $parts and no arm above reads it. Name this corpus in one of them."
      fi
    done

    echo "round-trip: no lookup is written for $CORPUS. The install, the components and the links are what it proves."
    ;;
esac

echo "round-trip: passed."
