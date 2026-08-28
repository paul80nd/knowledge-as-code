#!/usr/bin/env sh
# import-round-trip — publish from one corpus, consume it from another, and break it on purpose.
#
# The layers above prove each half apart. `FreshnessTests` and `ResolverTests` hold the rules over sets
# of strings, the golden fixtures diff an export tree file for file, and the feature specs hold what the
# validator says about a corpus. None of them can show that a governance change reaches a repository its
# owner does not control, which is the promise the whole import half was built for.
#
# So this runs the chain over the two worked corpora: export and pack in `examples/engineering/`, restore
# and validate in `examples/payments/`. Then it renames a clause upstream, leaving that corpus valid, and
# asserts the downstream build goes red naming the citation nobody downstream touched.
#
# Run it from anywhere:
#
#   sh tooling/tests/import-round-trip.sh
#
# It edits one file in `examples/engineering/` and puts it back, from a copy rather than from git, so an
# uncommitted edit of your own survives a run. It also rebuilds `.dist/` in both corpora and `.imports/`
# in the consumer, all of which are untracked and rebuilt by any later command.
#
# Held to the subset Git Bash and older macOS bash agree on: no arrays, no `[[`, no process substitution.
#
# One prerequisite beyond the .NET SDK: `jq` reads the export manifest.

set -e

REPO=$(cd "$(dirname "$0")/../.." && pwd)
KAC="$REPO/tooling/kac"
PRODUCER="$REPO/examples/engineering"
CONSUMER="$REPO/examples/payments"

# The clause this proves the trip with. It is a real citation: `examples/payments/` binds its card-data
# standard to the governance policy on collecting no more personal data than is needed.
POLICY="$PRODUCER/policies/data-data-protection.md"
CLAUSE=MINIMAL
RENAMED=MINIMUM
CITATION="eng:pol-DATA.$CLAUSE"

WORK=${WORK:-$(mktemp -d)}
rm -rf "$WORK"
mkdir -p "$WORK"

fail() {
  echo "import-round-trip: $1"
  exit 1
}

# The upstream file goes back however this run ends, including on a failed assertion. A copy rather than
# `git checkout`, which would also discard whatever else the person running this had in that file.
cp "$POLICY" "$WORK/policy.md"
restore_policy() {
  cp "$WORK/policy.md" "$POLICY"
}
trap restore_policy EXIT

# One `kac` invocation at a time throughout: concurrent runs build the same project and contend over its
# output.
kac() {
  corpus=$1
  shift
  (cd "$corpus" && dotnet run --project "$KAC" -- "$@")
}

# The consumer takes the package the producer sealed, and never the producer's working tree. `.imports/`
# is deleted first because a restore keeps a folder already holding the version it resolved to, so a
# rebuilt package at an unchanged `content-version` would not be unpacked over it.
publish_and_restore() {
  kac "$PRODUCER" export > /dev/null
  kac "$PRODUCER" pack > /dev/null
  rm -rf "$CONSUMER/.imports"
  kac "$CONSUMER" restore > /dev/null
}

echo "import-round-trip: the citation is there to prove"

# Asserted rather than assumed, because every assertion below passes over a corpus that cites nothing.
grep -rq "$CITATION" "$CONSUMER/standards" || fail "$CONSUMER/standards cites no $CITATION."

# And it reaches across the boundary rather than into the corpus doing the citing. A consumer holding its
# own `pol-DATA` would resolve this locally and prove nothing about an import.
if grep -rq "^id: pol-DATA$" "$CONSUMER/standards" "$CONSUMER/services" "$CONSUMER/nfrs" 2>/dev/null; then
  fail "$CONSUMER holds a record with id 'pol-DATA', so $CITATION resolves locally."
fi

echo "import-round-trip: publish, restore, validate"

publish_and_restore

MANIFEST="$CONSUMER/.imports/eng/manifest.json"
[ -f "$MANIFEST" ] || fail "a restore left no manifest at $MANIFEST."

# The clause arrived in the import, read through the manifest rather than through a path spelled here.
# A consumer finds a type's parts file where that type's own entry says it is.
PARTS=$(jq -r '.types[] | select(.type == "policies") | .partsFile' "$MANIFEST" | tr -d '\r')
[ -n "$PARTS" ] || fail "$MANIFEST names no parts file for policies."
grep -q "\"$CLAUSE\"" "$CONSUMER/.imports/eng/$PARTS" \
  || fail "the restored import holds no clause '$CLAUSE'."

kac "$CONSUMER" validate || fail "the consumer does not validate against the import it restored."

echo "import-round-trip: break the clause upstream"

# A rename the producer is free to make. Their corpus stays valid: the id is well formed, the table still
# parses, and nothing upstream cited the old spelling. What it breaks is downstream, in a repository the
# producer cannot edit, which is the whole reason the citation is checked rather than trusted.
sed "s/\`$CLAUSE\`/\`$RENAMED\`/" "$POLICY" > "$WORK/renamed.md"
cmp -s "$POLICY" "$WORK/renamed.md" && fail "renaming '$CLAUSE' in $POLICY changed nothing."
cp "$WORK/renamed.md" "$POLICY"

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

echo "import-round-trip: ok"
