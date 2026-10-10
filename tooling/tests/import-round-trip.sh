#!/usr/bin/env sh
# import-round-trip: publish from one corpus, consume it from another, and break it on purpose.
# `tooling/README.md` says what it proves and how to run it.
#
# It edits one file in `examples/engineering/` and puts it back from a copy, so an uncommitted edit of yours
# survives a run. It rebuilds `.dist/` in both corpora and `.imports/` in the consumer, all untracked.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process substitution.

set -e

REPO=$(cd "$(dirname "$0")/../.." && pwd)
KAC="$REPO/tooling/kac"
PRODUCER="$REPO/examples/engineering"
CONSUMER="$REPO/examples/payments"

# It cites no external framework, which keeps the rename clean: renaming a clause that cites one would break the
# producer's own `rpt-framework-coverage` as well.
POLICY="$PRODUCER/policies/delivery/derv-derived-data-verification.md"
CLAUSE=LINEAGE
RENAMED=ORIGIN
CITATION="eng:pol-DERV.$CLAUSE"

WORK=${WORK:-$(mktemp -d)}
mkdir -p "$WORK"

fail() {
  echo "import-round-trip: $1"
  exit 1
}

# A copy rather than `git checkout`, which would also discard whatever else you had in that file.
cp "$POLICY" "$WORK/policy.md"
DONE=0

restore_policy() {
  cp "$WORK/policy.md" "$POLICY"
}

cleanup() {
  restore_policy
  # A restore keeps a folder already holding the version it resolved to, so a broken export left there would be
  # read by every later run.
  [ "$DONE" = 1 ] || rm -rf "$CONSUMER/.imports"
}

trap cleanup EXIT

# One `kac` invocation at a time throughout: concurrent runs build the same project and contend over its
# output.
kac() {
  corpus=$1
  shift
  (cd "$corpus" && dotnet run --project "$KAC" -- "$@")
}

publish_and_restore() {
  kac "$PRODUCER" export > /dev/null
  kac "$PRODUCER" pack > /dev/null
  # A restore keeps a folder already holding the version it resolved to, so a rebuilt package at an unchanged
  # `content-version` would not be unpacked over it.
  rm -rf "$CONSUMER/.imports"
  kac "$CONSUMER" restore > /dev/null
}

echo "import-round-trip: the citation is there to prove"

# Asserted rather than assumed, because every assertion below passes over a corpus that cites nothing.
grep -rq "$CITATION" "$CONSUMER/standards" || fail "$CONSUMER/standards cites no $CITATION."

# A consumer holding its own `pol-DATA` would resolve the citation locally and prove nothing about an import. Read
# over the whole corpus, because a list of type folders here would be a copy of `types:`.
if grep -rq "^id: pol-DATA$" "$CONSUMER" --exclude-dir=.imports --exclude-dir=.dist; then
  fail "$CONSUMER holds a record with id 'pol-DATA', so $CITATION resolves locally."
fi

echo "import-round-trip: publish, restore, validate"

publish_and_restore

MANIFEST="$CONSUMER/.imports/eng/manifest.json"
[ -f "$MANIFEST" ] || fail "a restore left no manifest at $MANIFEST."

# Through the manifest rather than a path spelled here: a consumer finds a parts file where its type's entry says.
PARTS=$(jq -r '.types[] | select(.type == "policies") | .partsFile' "$MANIFEST" | tr -d '\r')
[ -n "$PARTS" ] || fail "$MANIFEST names no parts file for policies."
grep -q "\"$CLAUSE\"" "$CONSUMER/.imports/eng/$PARTS" \
  || fail "the restored import holds no clause '$CLAUSE'."

kac "$CONSUMER" validate || fail "the consumer does not validate against the import it restored."

echo "import-round-trip: break the clause upstream"

# A rename the producer is free to make. Their corpus stays valid, and what breaks is downstream.
sed "s/\`$CLAUSE\`/\`$RENAMED\`/" "$POLICY" > "$WORK/renamed.md"
cmp -s "$POLICY" "$WORK/renamed.md" && fail "renaming '$CLAUSE' in $POLICY changed nothing."
cp "$WORK/renamed.md" "$POLICY"

# `export` does not validate, so a rename that broke the producer would ship, and the failure below would mean
# something else.
kac "$PRODUCER" validate \
  || fail "renaming '$CLAUSE' left $PRODUCER invalid, so the break is not a clean one. A record \
upstream cites the old spelling, and a report naming that clause is the likely one."

publish_and_restore

if kac "$CONSUMER" validate > "$WORK/broken.txt" 2>&1; then
  cat "$WORK/broken.txt"
  fail "the consumer still validates against an import that no longer carries '$CLAUSE'."
fi

# Named rather than merely failed. A run that went red for another reason would pass a bare exit check.
grep -q "$CLAUSE" "$WORK/broken.txt" || {
  cat "$WORK/broken.txt"
  fail "the failure does not name the clause '$CLAUSE'."
}

echo "import-round-trip: put the clause back"

restore_policy
publish_and_restore
kac "$CONSUMER" validate || fail "the consumer does not validate after the clause was put back."

DONE=1
echo "import-round-trip: ok"
