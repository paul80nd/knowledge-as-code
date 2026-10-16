#!/bin/sh
# Builds the capture trial's fixture: a corpus repository, a clone of it, its findings file and two question logs.
set -eu

[ $# -eq 1 ] || { echo "usage: build.sh <empty-directory>" >&2; exit 2; }
mkdir -p "$1"
[ -z "$(ls -A "$1")" ] || { echo "build.sh: $1 is not empty" >&2; exit 2; }
dir=$(cd "$1" && pwd)

# The fixture ignores global and system git settings, so every machine builds the same history.
GIT_CONFIG_NOSYSTEM=1 GIT_CONFIG_GLOBAL=/dev/null
export GIT_CONFIG_NOSYSTEM GIT_CONFIG_GLOBAL
git() { command git -c commit.gpgsign=false -c core.autocrlf=false "$@"; }

# Sets who the next commit or merge is by, and when, in UTC.
by() { name=$1 email=$2 when="$3 +0000"; }
commit() {
  git add -A
  GIT_AUTHOR_NAME=$name GIT_AUTHOR_EMAIL=$email GIT_AUTHOR_DATE=$when \
    GIT_COMMITTER_NAME=$name GIT_COMMITTER_EMAIL=$email GIT_COMMITTER_DATE=$when git commit -q -m "$1"
}
merge() {
  GIT_AUTHOR_NAME=$name GIT_AUTHOR_EMAIL=$email GIT_AUTHOR_DATE=$when \
    GIT_COMMITTER_NAME=$name GIT_COMMITTER_EMAIL=$email GIT_COMMITTER_DATE=$when \
    git merge -q --no-ff -m "Merge $1" "$1"
}
# Writes a record with frontmatter. Each record's words differ, so git pairs only a real rename.
record() {
  mkdir -p "$(dirname "$1")"
  printf -- '---\nid: %s\ntitle: %s\n---\n\n# %s\n\nThe %s rule, written for the fixture alone.\n' \
    "$2" "$3" "$3" "$3" >"$1"
}

olive='Olive Owner'
work='Olive.Work@example.invalid'

mkdir "$dir/upstream"
cd "$dir/upstream"
git init -q -b main

# The repository starts outside the corpus folder.
by "$olive" olive@example.invalid 2026-02-15T09:00:00
echo '# Fixture repository' >README.md
commit 'Start the repository'

# The corpus adopts three types. Glossary never takes a record.
by "$olive" olive@example.invalid 2026-03-01T10:00:00
mkdir -p kb/standards kb/policies kb/glossary
cat >kb/.corpus.yaml <<'EOF'
descriptor-version: 6
corpus: fixture
publishing-target: none
tracker:
  target: azure-devops
  base: "https://dev.azure.com/example/Knowledge Base"
types:
  - standards
  - policies
  - glossary
EOF
echo '# Standards' >kb/standards.md
for t in standards policies glossary; do echo "# $t" >"kb/$t/_index.md"; done
commit 'Add the corpus'

# The first record, under the owner's second address. The corpus took its first record on 2 March.
by "$olive" "$work" 2026-03-02T10:00:00
record kb/standards/naming.md std-NAME Naming
commit 'Add the naming standard'

# Bob's first commit changes no record. His first record lands five days later.
git switch -q -c bob-index
by Bob bob@example.invalid 2026-03-03T09:00:00
echo 'Ask Bob about standards.' >>kb/standards/_index.md
commit 'Point the standards index at Bob'
git switch -q main
by "$olive" "$work" 2026-03-03T11:00:00
merge bob-index

# A commit outside the corpus folder, by somebody who never commits inside it.
by Erin erin@example.invalid 2026-03-04T09:00:00
echo 'Erin was here.' >>README.md
commit 'Update the README'

git switch -q -c bob-retention
by Bob bob@example.invalid 2026-03-05T09:00:00
record kb/policies/retention.md pol-RET Retention
commit 'Add the retention policy'

# Dave commits to the corpus and never adds a record.
git switch -q main
git switch -q -c dave-glossary
by Dave dave@example.invalid 2026-03-06T09:00:00
echo 'Terms go here.' >>kb/glossary/_index.md
commit 'Describe the glossary'
git switch -q main
by "$olive" "$work" 2026-03-06T12:00:00
merge dave-glossary

# An automation account adds a record. -x leaves it out of every author count.
by 'Fixture Bot' bot@example.invalid 2026-03-07T10:00:00
record kb/policies/bot-report.md pol-BOT 'Bot report'
commit 'Add the bot report'

by "$olive" "$work" 2026-03-08T10:00:00
merge bob-retention

# A record whose name is outside ASCII.
by "$olive" olive@example.invalid 2026-03-09T10:00:00
record kb/standards/café.md std-CAFE Café
commit 'Add the café standard'

# Reading 1 is as of 10 March. A rename inside one type follows it, and is no move.
by "$olive" olive@example.invalid 2026-03-20T10:00:00
mkdir -p kb/standards/style
git mv kb/standards/naming.md kb/standards/style/naming.md
commit 'File the naming standard under style'

# Carol is new after reading 1. Her record has CRLF line endings and cites finding 12.
git switch -q -c carol-review
by Carol carol@example.invalid 2026-04-01T09:00:00
printf -- '---\r\nid: std-REV\r\ntitle: Review\r\nsources:\r\n  - { resource: %s }\r\n---\r\n\r\n# Review\r\n' \
  'https://dev.azure.com/example/Knowledge%20Base/_workitems/edit/12' >kb/standards/review.md
commit 'Add the review standard'
by Carol carol@example.invalid 2026-04-03T09:00:00
printf 'Review every change.\r\n' >>kb/standards/review.md
commit 'Say what review covers'
git switch -q main
by "$olive" "$work" 2026-04-10T10:00:00
merge carol-review

# A record moves from one type's folder to another's, through a merge.
git switch -q -c move-retention
by "$olive" olive@example.invalid 2026-05-04T10:00:00
git mv kb/policies/retention.md kb/standards/retention.md
commit 'Move retention to standards'
git switch -q main
by "$olive" olive@example.invalid 2026-05-05T10:00:00
merge move-retention

# Reading 2 is as of 10 June. The first record counts, because it lands within that day. The second does not.
by "$olive" olive@example.invalid 2026-06-10T23:30:00
record kb/policies/late.md pol-LATE Late
commit 'Add the late policy'
by "$olive" olive@example.invalid 2026-06-11T00:30:00
record kb/policies/later.md pol-LATER Later
commit 'Add the later policy'

# The clone sets origin/HEAD, which the script reads for the default branch.
git clone -q "$dir/upstream" "$dir/clone"

# The findings file, as the README's az command writes it. The project's name has a space.
base='https://dev.azure.com/example/Knowledge Base/_workitems/edit'
{
  printf '# filed\taddress\n'
  printf '2026-03-04\t%s/11\n' "$base"
  printf '2026-03-09\t%s/12\n' "$base"
  printf '2026-04-20\t%s/13\n' "$base"
  printf '2026-06-11\t%s/14\n' "$base"
} >"$dir/findings.tsv"

# Each log has lines the script leaves out: a day outside the log, and a mark that is neither yes nor no.
cat >"$dir/log-1.csv" <<'EOF'
date,linked
2026-03-09,no
2026-03-10,yes
2026-03-11,no
2026-03-12, no
2026-03-13,maybe
2026-03-23,yes
2026-03-24,yes
EOF
cat >"$dir/log-2.csv" <<'EOF'
date,linked
2026-05-27,no
2026-05-28,yes
2026-06-01,yes
2026-06-05,no
2026-06-10,yes
EOF
