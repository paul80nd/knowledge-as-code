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

# Takes one reading of a corpus, and compares its aggregate with an expected file.
check() {
  what=$1 expected=$2; shift 2
  rm -rf "$dir/out"
  sh "$capture" "$@" "$dir/out" 2>"$dir/stderr" >/dev/null || {
    echo "FAIL $what: capture.sh stopped"; cat "$dir/stderr"; failed=1; return
  }
  if diff -u "$here/expected/$expected.md" "$dir/out/aggregate.md"; then
    echo "pass $what"
  else
    echo "FAIL $what: the aggregate differs from expected/$expected.md"; failed=1
  fi
}
# Reading 1 takes the fixture's fault log, and reading 2 the empty one beside capture.sh.
reading() {
  faults=$fx/fault-log-1.csv
  [ "$1" = 1 ] || faults=$here/../fault-log.csv
  check "${3:-reading $1}" "reading-$1" -c "$fx/clone/kb" -d "$2" -r "$1" -o "$owners" -x bot@example.invalid \
    -f "$fx/findings.tsv" -l "$fx/log-$1.csv" -k "$faults"
}

# Runs capture.sh where it must stop, and checks that it does with the message given.
refuses() {
  what=$1 message=$2; shift 2
  if sh "$capture" "$@" "$dir/refused" >/dev/null 2>"$dir/stderr"; then
    echo "FAIL $what: capture.sh did not stop"; failed=1
  elif [ -f "$dir/refused/aggregate.md" ]; then
    echo "FAIL $what: capture.sh stopped, and still wrote aggregate.md"; failed=1
  elif grep -qF -- "$message" "$dir/stderr"; then
    echo "pass $what"
  else
    echo "FAIL $what: expected '$message', got:"; cat "$dir/stderr"; failed=1
  fi
  rm -rf "$dir/refused"
}

reading 1 2026-03-10
reading 2 2026-06-10
check 'a corpus at the repository root' root-1 \
  -c "$fx/root" -d 2026-03-10 -r 1 -o olive@example.invalid -f "$fx/root-findings.tsv"

# A jq that ends every line with CRLF, as a Windows build can. Reading 2 must not change.
path=$PATH
mkdir "$dir/crlf-bin"
real_jq=$(command -v jq)
cat >"$dir/crlf-bin/jq" <<EOF
#!/bin/sh
out=\$("$real_jq" "\$@") || exit
[ -z "\$out" ] || printf '%s\n' "\$out" | awk '{ printf "%s\r\n", \$0 }'
EOF
chmod +x "$dir/crlf-bin/jq"
PATH=$dir/crlf-bin:$path
reading 2 2026-06-10 'reading 2 with a jq writing CRLF'
PATH=$path

# A git older than 2.31.
mkdir "$dir/old-bin"
real_git=$(command -v git)
cat >"$dir/old-bin/git" <<EOF
#!/bin/sh
[ "\$1" = --version ] && { echo 'git version 2.30.2'; exit 0; }
exec "$real_git" "\$@"
EOF
chmod +x "$dir/old-bin/git"
PATH=$dir/old-bin:$path
refuses 'a git older than 2.31' 'The script needs git 2.31 or later' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners" -f "$fx/findings.tsv"
PATH=$path

today=$(date -u +%Y-%m-%d)
refuses 'a run before the log has ended' 'Run the script on or after that day' \
  -c "$fx/clone/kb" -d "$today" -r 2 -o "$owners" -f "$fx/findings.tsv" -l "$fx/log-2.csv"
refuses 'a run before the fault log has ended' 'Run the script on or after that day' \
  -c "$fx/clone/kb" -d "$today" -r 2 -o "$owners" -f "$fx/findings.tsv" -k "$here/../fault-log.csv"
refuses 'an -o matching no commit' '-o gives nobody@example.invalid, which made no commit to the corpus' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o nobody@example.invalid -f "$fx/findings.tsv"
refuses 'a mistyped second -o' '-o gives olive.wrok@example.invalid, which made no commit to the corpus' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o olive@example.invalid,olive.wrok@example.invalid -f "$fx/findings.tsv"
refuses 'a mistyped -x' '-x gives bot@example.invald, which made no commit to the corpus' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners" -x bot@example.invald -f "$fx/findings.tsv"
refuses 'an -o with no commit by the as-of date' 'no commit to the corpus up to 2026-03-10 is by an address under -o' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o carol@example.invalid -f "$fx/findings.tsv"
refuses 'an azure-devops tracker with no -f' 'the tracker is on Azure DevOps, which the script cannot query' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners"
refuses 'a derived azure-devops tracker with no -f' 'the tracker is on Azure DevOps, which the script cannot query' \
  -c "$fx/root" -d 2026-03-10 -r 1 -o olive@example.invalid
printf '2026-03-04 no tab here\n' >"$dir/bad-findings.tsv"
refuses 'a findings line with no tab' 'each line of the findings file takes a date, a tab and an address' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners" -f "$dir/bad-findings.tsv"

# A log a spreadsheet saved, with its dates rewritten. The run stops, and gives the line's number.
printf 'date,linked\r\n10/03/2026,yes\r\n11/03/2026,no\r\n' >"$dir/excel-log.csv"
for message in 'line 2 of the question log: 10/03/2026,yes' 'A spreadsheet may have rewritten the dates'; do
  refuses "a log with Excel's dates: $message" "$message" \
    -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -l "$dir/excel-log.csv"
done
printf 'date,linked\n2026-03-10,yes\n2026-03-11,maybe\n' >"$dir/maybe-log.csv"
refuses 'a log line marked neither yes nor no' 'line 3 of the question log: 2026-03-11,maybe' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -l "$dir/maybe-log.csv"
refuses 'a log with no line inside its days' 'no line of the question log is dated 2026-03-10 to 2026-03-23' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -l "$fx/log-2.csv"

# A fault log line that is not a date, a rule id, and record or tool. The run stops, and gives the line's number.
printf 'date,rule,in\n2026-03-10,ref-resolves,record\n2026-03-11,ref-resolves,content\n' >"$dir/content-faults.csv"
refuses 'a fault line marked neither record nor tool' 'line 3 of the fault log: 2026-03-11,ref-resolves,content' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -k "$dir/content-faults.csv"
printf 'date,rule,in\n2026-03-11,record\n' >"$dir/no-rule-faults.csv"
refuses 'a fault line with no rule id' 'line 2 of the fault log: 2026-03-11,record' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -k "$dir/no-rule-faults.csv"
printf 'date,rule,in\n2026-03-11,Ref-Resolves,record\n' >"$dir/capital-faults.csv"
refuses 'a fault line with a rule id in capitals' 'the rule id kac printed' \
  -c "$fx/clone/kb" -d 2026-03-10 -r 1 -o "$owners" -f "$fx/findings.tsv" -k "$dir/capital-faults.csv"
refuses 'a fault log with no line inside its days' 'no line of the fault log is dated 2026-06-10 to 2026-06-23' \
  -c "$fx/clone/kb" -d 2026-06-10 -r 2 -o "$owners" -f "$fx/findings.tsv" -k "$fx/fault-log-1.csv"

# A types block that is not a list of type names, on each day of the third corpus.
refuses 'a types block that is a mapping' 'the descriptor at 2026-03-01 has line 5:   standards: 2.' \
  -c "$fx/mapping" -d 2026-03-01 -r 1 -o olive@example.invalid
refuses 'a types block that is a flow mapping' 'the descriptor at 2026-03-02 has line 4: types: { standards: 2 }.' \
  -c "$fx/mapping" -d 2026-03-02 -r 1 -o olive@example.invalid
refuses 'a type name in capitals' 'the descriptor at 2026-03-03 has line 5:   - Standards.' \
  -c "$fx/mapping" -d 2026-03-03 -r 1 -o olive@example.invalid

[ "$failed" = 0 ] && echo "every case passed"
exit "$failed"
