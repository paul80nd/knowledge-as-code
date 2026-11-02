#!/bin/sh
# Runs the plugin trial's scripts over a small fixture, once as written and once with every file ending its lines with
# CRLF, as a Windows checkout without `* text=auto eol=lf` does. Each script must give the same output both times. The
# result files contain Windows paths, the fixture sits in a folder whose name has a space, and regrade.mjs reads a
# grader and a trace with CRLF line endings. It needs Node.js.
# shellcheck disable=SC2016 # each `name` is an expected line of Markdown
set -eu

if ! command -v node >/dev/null; then
  echo "FAIL node is not installed, and every script but stage.sh runs on it"
  exit 1
fi

here=$(cd "$(dirname "$0")" && pwd)
scripts=$here/..
dir=$(mktemp -d "${TMPDIR:-/tmp}/plugin-fixture.XXXXXX")
trap 'rm -rf "$dir"' EXIT
fx="$dir/with space"
lf=$fx/lf
mkdir -p "$lf"
failed=0

pass() { echo "pass $1"; }
fail() { echo "FAIL $1"; failed=1; }

# A corpus export of two types. One standard is the corpus's own, and one a layer brought in. Glossary has no parts.
# A section of std-A contains a carriage return, which CLAUDE.md must not.
export_dir=$lf/plugin/corpus
mkdir -p "$lf/plugin/.claude-plugin" "$lf/plugin/hooks" "$export_dir/standards/eng" "$export_dir/glossary"
printf '{ "name": "fixture", "version": "0.1.0" }\n' >"$lf/plugin/.claude-plugin/plugin.json"
printf '{}\n' >"$lf/plugin/hooks/build-index.json"
cat >"$export_dir/manifest.json" <<'EOF'
{ "corpus": "fixture", "about": { "displayName": "Fixture" },
  "types": [
    { "type": "standards", "dir": "standards", "parts": 2, "partsFile": "standards-parts.jsonl",
      "recordKey": "record", "frameworksFile": "frameworks.json" },
    { "type": "glossary", "dir": "glossary", "parts": 0 } ] }
EOF
cat >"$export_dir/standards/std-A.json" <<'EOF'
{ "fields": { "id": "std-A", "title": "Rule A", "status": "active", "owner": "olive", "tags": ["one", "two"] },
  "links": { "human": "https://example.invalid/std-A" }, "sections": { "Rule": "Do A.\r\nThen B." } }
EOF
cat >"$export_dir/standards/eng/std-B.json" <<'EOF'
{ "fields": { "id": "std-B", "title": "Rule B", "status": "active" }, "sections": {} }
EOF
cat >"$export_dir/standards-parts.jsonl" <<'EOF'
{ "id": "std-A.ONE", "record": "std-A", "title": "One", "text": "The first clause." }
{ "id": "eng:std-B.TWO", "record": "eng:std-B", "text": "The second clause." }
EOF
printf '{ "name": "OWASP ASVS", "version": "4.0.3" }\n' >"$export_dir/frameworks.json"
cat >"$export_dir/glossary/gl-term.json" <<'EOF'
{ "fields": { "id": "gl-term", "title": "Term", "definition": "A word." } }
EOF

# Four cases, each tagged with its kind and the record its rule sits in. Two house cases test standards, one of them a
# layer's, and one tests the glossary. A fifth, which excluded.tsv leaves out, tags a record the export does not contain.
for c in house/case-a:std-A control/case-b:std-A house/case-c:gl-gone house/case-d:eng:std-B house/case-e:gl-term; do
  kase=${c%%:*} record=${c#*:}
  mkdir -p "$lf/cases/$kase/graders"
  printf -- '---\nname: %s\ntags: [%s, %s]\n---\n\nAnswer the question.\n' "${kase#*/}" "${kase%/*}" "$record" \
    >"$lf/cases/$kase/prompt.md"
  printf 'name: %s\n' "${kase#*/}" >"$lf/cases/$kase/case.yaml"
done
printf 'house/case-c\tleft out by the fixture\n' >"$lf/cases/excluded.tsv"

# The plugin stage as stage.sh lays it out, which score.mjs reads each case's record type from.
mkdir -p "$lf/stage/plugin/evals"
cp -R "$lf/plugin/." "$lf/stage/plugin/"
cp -R "$lf/cases/." "$lf/stage/plugin/evals/"

# The result files score.mjs and merge.mjs read. The plugin stage has the fixture's own paths, the prose stage
# Windows paths with backslashes, and the hook stage Windows paths with forward slashes. Each stage gives its file, its
# ablation, its folder, its path separator and its cost, then a verdict per case and arm, as task comply cite fired.
mkdir -p "$lf/results"
node -e '
const { writeFileSync } = require("node:fs");
function stage(file, ablation, root, sep, cost, verdicts) {
  const run = (v) => {
    const [task, comply, cite, fired] = v.split(" ").map((x) => x === "1");
    return { costUsd: 0.25, turns: 3, error: null, tracePath: [root, "kept", "trace.jsonl"].join(sep),
      graders: [{ name: "task-answers", passed: task }, { name: "comply-owner", passed: comply },
        { name: "cite-record", passed: cite }, { name: "fired-lookup", passed: fired }] };
  };
  const arms = ([w, wo]) => (wo === undefined ? { with: [run(w)] } : { with: [run(w)], without: [run(wo)] });
  const result = { partial: false, claudeVersion: "2.1.292", costUsd: cost,
    suite: { root, ablation, modelOverride: "claude-opus-5-5", plugins: [{ name: "fixture", version: "0.1.0", path: root }] },
    cases: Object.entries(verdicts).map(([path, v]) => ({ name: path.split("/")[1],
      dir: [root, "evals", ...path.split("/")].join(sep), graders: [], arms: arms(v) })) };
  writeFileSync(`${process.argv[1]}/${file}`, JSON.stringify(result, null, 2) + "\n");
}
stage("plugin.json", "with-without", process.argv[2], "/", 1.25, { "house/case-a": ["1 1 1 1", "1 0 0 0"],
  "control/case-b": ["1 1 1 1", "1 1 0 0"], "house/case-d": ["1 1 0 1", "1 0 0 0"],
  "house/case-e": ["1 0 1 0", "1 1 0 0"] });
stage("prose.json", "none", "C:\\Users\\Some One\\stage\\prose", "\\", 0.5, { "house/case-a": ["1 0 0 0"],
  "control/case-b": ["1 1 0 0"], "house/case-d": ["1 0 0 0"], "house/case-e": ["1 0 0 0"] });
stage("hook.json", "none", "C:/Users/Some One/stage/hook", "/", 0.25, { "house/case-a": ["1 1 1 1"],
  "control/case-b": ["1 1 1 1"], "house/case-d": ["1 1 1 1"], "house/case-e": ["1 1 1 1"] });
' "$lf/results" "$lf/stage/plugin"
printf 'case\tarm\trun\tgraders\tadjudicated\treason\ncase-a\tnone\t1\tfail\tfail\tThe file names no owner.\n' \
  >"$lf/adjudication.tsv"

# The same fixture with every line ending in CRLF.
cp -R "$lf" "$fx/crlf"
find "$fx/crlf" -type f | while IFS= read -r f; do
  awk '{ printf "%s\r\n", $0 }' "$f" >"$f.crlf"
  mv "$f.crlf" "$f"
done
sed 's|with space/lf/|with space/crlf/|g' "$fx/crlf/results/plugin.json" >"$fx/crlf/results/plugin.json.moved"
mv "$fx/crlf/results/plugin.json.moved" "$fx/crlf/results/plugin.json"

# Runs a script over the fixture as written and with CRLF, and checks both runs succeed with the same output.
# $1 names the check, $2 the folders the two runs write in, and the rest is the command. {in} stands for each fixture,
# and {out} for each folder.
both() {
  what=$1 name=$2; shift 2
  for kind in lf crlf; do
    out=$dir/$name-$kind
    mkdir -p "$out"
    if ! run "$out" "$fx/$kind" "$@" >"$out.stdout" 2>"$out.stderr"; then
      fail "$what over files ending in $kind: the script stopped"; cat "$out.stderr"; return
    fi
  done
  if diff -r "$dir/$name-lf" "$dir/$name-crlf" >"$dir/$name.diff"; then
    pass "$what gives the same output from files ending in CRLF"
  else
    fail "$what gives different output from files ending in CRLF"; head -20 "$dir/$name.diff"
  fi
}

# Runs the command with every {out} replaced by the folder given first, and every {in} by the fixture given second.
run() {
  out=$1 in=$2; shift 2
  for arg in "$@"; do
    shift
    case $arg in *'{out}'*) arg="${arg%%\{out\}*}$out${arg#*\{out\}}" ;; esac
    case $arg in *'{in}'*) arg="${arg%%\{in\}*}$in${arg#*\{in\}}" ;; esac
    set -- "$@" "$arg"
  done
  "$@"
}

# Checks a file contains a line.
has() {
  if grep -qF -- "$2" "$1"; then pass "$3"; else fail "$3: $(basename "$1") has no line '$2'"; fi
}

# prose-claude-md.mjs
both 'prose-claude-md.mjs' prose node "$scripts/prose-claude-md.mjs" '{in}/plugin/corpus' '{out}/CLAUDE.md'
has "$dir/prose-crlf/CLAUDE.md" '### Rule A (`std-A`)' 'prose-claude-md.mjs writes a record'
has "$dir/prose-crlf/CLAUDE.md" '#### `eng:std-B.TWO`' "prose-claude-md.mjs writes the part of a layer's record"
if grep -q "$(printf '\r')" "$dir/prose-crlf/CLAUDE.md"; then
  fail 'prose-claude-md.mjs leaves a carriage return in CLAUDE.md'
else
  pass 'prose-claude-md.mjs leaves no carriage return in CLAUDE.md'
fi

# stage.sh, over the fixture as written and with CRLF.
chars=$(wc -m <"$dir/prose-lf/CLAUDE.md" | tr -d ' ')
for kind in lf crlf; do
  if ! sh "$scripts/stage.sh" "$fx/$kind/plugin" "$fx/$kind/cases" "$dir/stage-$kind" >"$dir/stage-$kind.stdout" \
    2>"$dir/stage-$kind.stderr"; then
    fail "stage.sh over files ending in $kind: the script stopped"; cat "$dir/stage-$kind.stderr"
  fi
  has "$dir/stage-$kind.stdout" 'staged 4 case(s)' "stage.sh leaves out the case excluded.tsv lists, in $kind"
done
has "$dir/stage-crlf.stdout" "the prose arm's CLAUDE.md is $chars characters" 'stage.sh prints the size of CLAUDE.md'
if [ -s "$dir/stage-crlf.stderr" ]; then
  fail 'stage.sh warns on a CLAUDE.md under 40,000 characters'
else
  pass 'stage.sh gives no warning on a CLAUDE.md under 40,000 characters'
fi

big=$dir/big
cp -R "$lf/plugin" "$big"
awk 'BEGIN { for (i = 0; i < 41000; i++) s = s "a"
  printf "{ \"fields\": { \"id\": \"gl-long\" }, \"sections\": { \"Text\": \"%s\" } }\n", s }' \
  >"$big/corpus/glossary/gl-long.json"
sh "$scripts/stage.sh" "$big" "$lf/cases" "$dir/big-stage" >/dev/null 2>"$dir/big-stage.stderr" ||
  fail 'stage.sh stopped on a CLAUDE.md over 40,000 characters'
has "$dir/big-stage.stderr" "is over 40,000 characters" 'stage.sh warns on a CLAUDE.md over 40,000 characters'

# score.mjs
both 'score.mjs' score node "$scripts/score.mjs" -a '{in}/adjudication.tsv' -k '{in}/results/hook.json' \
  '{in}/results/plugin.json' '{in}/results/prose.json' '{out}' '{in}/cases/excluded.tsv'
aggregate=$dir/score-crlf/aggregate.md
has "$aggregate" '| `claude_version` | 2.1.292 |' 'score.mjs reads the Claude Code version'
has "$aggregate" '| `method` | 7 |' 'score.mjs gives the version of the method'
has "$aggregate" '| `cases` | 4 |' 'score.mjs counts the cases'
has "$aggregate" '| `excluded_cases` | 1 |' 'score.mjs counts the cases excluded.tsv lists'
has "$aggregate" '| `adjudicated` | 1 |' 'score.mjs reads adjudication.tsv'
has "$aggregate" '| `house.plugin.sessions` | 3 |' 'score.mjs reads a case kind from a POSIX path'
has "$aggregate" '| `control.prose.sessions` | 1 |' 'score.mjs reads a case kind from a path with backslashes'
has "$aggregate" '| `house.hook.sessions` | 3 |' 'score.mjs reads a case kind from a Windows path with slashes'
has "$aggregate" '| `house.lead_over_prose` | 0.67 |' "score.mjs gives the plugin arm's lead over prose"
has "$aggregate" '| `house.lead_over_none` | 0.34 |' "score.mjs gives the plugin arm's lead over none"
has "$aggregate" '| `control.lead_over_none` | 0.00 |' "score.mjs gives the plugin arm's lead on the controls"
has "$aggregate" '| `house.plugin.type.standards.sessions` | 2 |' 'score.mjs counts a type over its cases'
has "$aggregate" '| `house.plugin.type.standards.cited` | 1 |' 'score.mjs counts the citations of a type'
has "$aggregate" '| `house.plugin.type.standards.complied` | 2 |' "score.mjs reads a layer's record from the manifest"
has "$aggregate" '| `house.plugin.type.glossary.complied` | 0 |' 'score.mjs counts the compliance of a type'
has "$aggregate" '| `house.none.type.standards.complied` | 0 |' 'score.mjs counts a type by the adjudicated verdict'
has "$aggregate" '| `house.none.type.glossary.complied` | 1 |' 'score.mjs counts a type in the none arm'
has "$aggregate" '| `house.prose.type.glossary.sessions` | 1 |' 'score.mjs counts a type in the prose arm'
has "$aggregate" '| `house.hook.type.standards.cited` | 2 |' 'score.mjs counts a type in the hook arm'
if grep -q '^| `control\.[a-z]*\.type\.' "$aggregate"; then
  fail 'score.mjs breaks the controls down by type'
else
  pass 'score.mjs breaks only the house cases down by type'
fi
has "$aggregate" '| `cost_usd` | 2.00 |' 'score.mjs adds the cost of every stage'
stripped=$dir/score-crlf/results
if ! [ -f "$stripped/plugin-stage.json" ] || ! [ -f "$stripped/prose-stage.json" ] ||
  ! [ -f "$stripped/hook-stage.json" ]; then
  fail 'score.mjs wrote no result files'
elif grep -l -e 'Some One' -e 'with space' -e 'C:' "$stripped/"*.json; then
  fail 'score.mjs leaves a path naming the user in the result files above'
else
  pass 'score.mjs cuts every Windows and POSIX path to its last part'
fi
if grep -lq "$(printf '\r')" "$dir/score-crlf/"*.* "$stripped/"*.json; then
  fail 'score.mjs writes a carriage return'
else
  pass 'score.mjs writes no carriage return'
fi

# score.mjs, over a stage where a house case tags a record the export does not contain.
unknown=$fx/unknown
mkdir -p "$unknown/results" "$dir/score-unknown"
cp -R "$lf/stage" "$unknown/stage"
sed 's/std-A]/std-Z]/' "$lf/stage/plugin/evals/house/case-a/prompt.md" \
  >"$unknown/stage/plugin/evals/house/case-a/prompt.md"
sed 's|with space/lf/|with space/unknown/|g' "$lf/results/plugin.json" >"$unknown/results/plugin.json"
if node "$scripts/score.mjs" "$unknown/results/plugin.json" - "$dir/score-unknown" >/dev/null \
  2>"$dir/score-unknown.stderr"; then
  fail 'score.mjs scores a house case whose record the export does not contain'
else
  has "$dir/score-unknown.stderr" 'case case-a tags std-Z' 'score.mjs stops on a record the export does not contain'
fi

# merge.mjs, over two batches of the prose stage, whose paths are the same in both fixtures.
both 'merge.mjs' merge node "$scripts/merge.mjs" '{out}/merged.json' '{in}/results/prose.json' \
  '{in}/results/prose.json'
if node -e 'const r = JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8"));
  process.exit(r.cases[0].arms.with.length === 2 ? 0 : 1)' "$dir/merge-crlf/merged.json"; then
  pass 'merge.mjs gives each case the runs of both batches'
else
  fail 'merge.mjs does not give each case the runs of both batches'
fi

# regrade.mjs, with a grader and a trace that have CRLF line endings, and a case folder named with backslashes.
kept=$fx/kept/run-1
mkdir -p "$kept/sealed/home/cwd" "$kept/trace" "$fx/regrade-cases/house/case-a/graders"
printf -- "---\ntype: regex\npattern: 'std-A'\n---\n\nCites the record.\n" | awk '{ printf "%s\r\n", $0 }' \
  >"$fx/regrade-cases/house/case-a/graders/cite-record.md"
printf '%s\n' '{"type":"assistant","message":{"content":[{"type":"text","text":"See std-A."}]}}' \
  '{"type":"result","result":"See std-A."}' | awk '{ printf "%s\r\n", $0 }' >"$kept/trace/trace.jsonl"
node -e '
const result = { suite: { ablation: "none" }, cases: [{
  name: "case-a", dir: "C:\\Users\\Some One\\stage\\prose\\evals\\house\\case-a",
  graders: [{ name: "cite-record", type: "regex",
    config: { target: "last_message", pattern: "std-A", flags: "", match: "contains" } }],
  arms: { with: [{ tracePath: process.argv[1], graders: [{ name: "cite-record", passed: true }] }] } }] };
require("node:fs").writeFileSync(process.argv[2], JSON.stringify(result, null, 2) + "\n");
' "$kept/trace/trace.jsonl" "$fx/regrade-in.json"
if node "$scripts/regrade.mjs" "$fx/regrade-cases" "$fx/regrade-in.json" "$dir/regrade-out.json" \
  >"$dir/regrade.stdout" 2>&1; then
  has "$dir/regrade.stdout" '1 unchanged verdict(s) agree with the tool, 0 disagree; 0 grader definition(s)' \
    'regrade.mjs reads a CRLF grader and agrees with the tool'
else
  fail 'regrade.mjs stopped'; cat "$dir/regrade.stdout"
fi

[ "$failed" = 0 ] && echo "every case passed"
exit "$failed"
