#!/bin/sh
# Runs the plugin trial's scripts over a small fixture, once with jq as installed and once with a jq writing CRLF.
# Each script must give the same output both times. The result files contain Windows paths, the fixture sits in a folder
# whose name has a space, and regrade.mjs reads a grader and a trace with CRLF line endings.
# shellcheck disable=SC2016 # each $name in a jq program is a jq variable, and each `name` an expected line of Markdown
set -eu

here=$(cd "$(dirname "$0")" && pwd)
scripts=$here/..
dir=$(mktemp -d "${TMPDIR:-/tmp}/plugin-fixture.XXXXXX")
trap 'rm -rf "$dir"' EXIT
fx="$dir/with space"
mkdir -p "$fx"
failed=0

pass() { echo "pass $1"; }
fail() { echo "FAIL $1"; failed=1; }

# A jq that ends every line with CRLF, as a Windows build does. It keeps every byte jq wrote, and jq's exit status.
path=$PATH
mkdir "$dir/crlf-bin"
real_jq=$(command -v jq)
cat >"$dir/crlf-bin/jq" <<EOF
#!/bin/sh
out=\$(mktemp) || exit
"$real_jq" "\$@" >"\$out" || { status=\$?; rm -f "\$out"; exit "\$status"; }
awk '{ printf "%s\r\n", \$0 }' "\$out"
rm -f "\$out"
EOF
chmod +x "$dir/crlf-bin/jq"
crlf_jq() { PATH=$dir/crlf-bin:$path; }
real_jq() { PATH=$path; }

# A corpus export of two types. One standard is the corpus's own, and one a layer brought in. Glossary has no parts.
export_dir=$fx/plugin/corpus
mkdir -p "$fx/plugin/.claude-plugin" "$fx/plugin/hooks" "$export_dir/standards/eng" "$export_dir/glossary"
printf '{ "name": "fixture", "version": "0.1.0" }\n' >"$fx/plugin/.claude-plugin/plugin.json"
printf '{}\n' >"$fx/plugin/hooks/build-index.json"
cat >"$export_dir/manifest.json" <<'EOF'
{ "corpus": "fixture", "about": { "displayName": "Fixture" },
  "types": [
    { "type": "standard", "dir": "standards", "parts": 2, "partsFile": "standards-parts.jsonl",
      "recordKey": "record", "frameworksFile": "frameworks.json" },
    { "type": "glossary", "dir": "glossary", "parts": 0 } ] }
EOF
cat >"$export_dir/standards/std-A.json" <<'EOF'
{ "fields": { "id": "std-A", "title": "Rule A", "status": "active", "owner": "olive", "tags": ["one", "two"] },
  "links": { "human": "https://example.invalid/std-A" }, "sections": { "Rule": "Do A." } }
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

# Two cases, one of each kind, and a third that excluded.tsv leaves out.
for c in house/case-a control/case-b house/case-c; do
  mkdir -p "$fx/cases/$c/graders"
  printf 'Answer the question.\n' >"$fx/cases/$c/prompt.md"
  printf 'name: %s\n' "${c#*/}" >"$fx/cases/$c/case.yaml"
done
printf 'house/case-c\tleft out by the fixture\n' >"$fx/cases/excluded.tsv"

# Runs a script with jq as installed and with the CRLF jq, and checks both runs succeed with the same output.
# $1 names the check, $2 the folders the two runs write in, and the rest is the command. {out} stands for each folder.
both() {
  what=$1 name=$2; shift 2
  for jq_kind in lf crlf; do
    if [ "$jq_kind" = lf ]; then real_jq; else crlf_jq; fi
    out=$dir/$name-$jq_kind
    mkdir -p "$out"
    if ! run "$out" "$@" >"$out.stdout" 2>"$out.stderr"; then
      real_jq
      fail "$what with a jq writing $jq_kind: the script stopped"; cat "$out.stderr"; return
    fi
    real_jq
  done
  if diff -r "$dir/$name-lf" "$dir/$name-crlf" >"$dir/$name.diff"; then
    pass "$what gives the same output with a jq writing CRLF"
  else
    fail "$what gives different output with a jq writing CRLF"; head -20 "$dir/$name.diff"
  fi
}

# Runs the command with every {out} replaced by the folder given first.
run() {
  out=$1; shift
  for arg in "$@"; do
    shift
    case $arg in
      *'{out}'*) set -- "$@" "${arg%%\{out\}*}$out${arg#*\{out\}}" ;;
      *) set -- "$@" "$arg" ;;
    esac
  done
  "$@"
}

# Checks a file contains a line.
has() {
  if grep -qF -- "$2" "$1"; then pass "$3"; else fail "$3: $(basename "$1") has no line '$2'"; fi
}

# prose-claude-md.sh
both 'prose-claude-md.sh' prose sh "$scripts/prose-claude-md.sh" "$export_dir" '{out}/CLAUDE.md'
has "$dir/prose-crlf/CLAUDE.md" '### Rule A (`std-A`)' 'prose-claude-md.sh writes a record'
has "$dir/prose-crlf/CLAUDE.md" '#### `eng:std-B.TWO`' "prose-claude-md.sh writes the part of a layer's record"
if grep -q "$(printf '\r')" "$dir/prose-crlf/CLAUDE.md"; then
  fail 'prose-claude-md.sh leaves a carriage return in CLAUDE.md'
else
  pass 'prose-claude-md.sh leaves no carriage return in CLAUDE.md'
fi

# stage.sh
both 'stage.sh' stage sh "$scripts/stage.sh" "$fx/plugin" "$fx/cases" '{out}/stage'
chars=$(wc -m <"$dir/prose-lf/CLAUDE.md" | tr -d ' ')
has "$dir/stage-crlf.stdout" "the prose arm's CLAUDE.md is $chars characters" 'stage.sh prints the size of CLAUDE.md'
if [ -s "$dir/stage-crlf.stderr" ]; then
  fail 'stage.sh warns on a CLAUDE.md under 40,000 characters'
else
  pass 'stage.sh gives no warning on a CLAUDE.md under 40,000 characters'
fi

big=$dir/big
cp -R "$fx/plugin" "$big"
awk 'BEGIN { for (i = 0; i < 41000; i++) s = s "a"
  printf "{ \"fields\": { \"id\": \"gl-long\" }, \"sections\": { \"Text\": \"%s\" } }\n", s }' \
  >"$big/corpus/glossary/gl-long.json"
crlf_jq
sh "$scripts/stage.sh" "$big" "$fx/cases" "$dir/big-stage" >/dev/null 2>"$dir/big-stage.stderr" ||
  fail 'stage.sh stopped on a CLAUDE.md over 40,000 characters'
real_jq
has "$dir/big-stage.stderr" "is over 40,000 characters" 'stage.sh warns on a CLAUDE.md over 40,000 characters'

# The result files score.sh and merge.sh read. The plugin stage has Windows paths with backslashes, the prose stage
# Windows paths with forward slashes, and the hook stage POSIX paths.
results=$fx/results
mkdir -p "$results"
# Writes one stage's result file: $1 the file, $2 the ablation, $3 the stage folder, $4 its path separator, $5 the
# cost, then a verdict per case and arm, as task comply cite fired for case-a then case-b.
stage_result() {
  jq -n --arg ablation "$2" --arg root "$3" --arg sep "$4" --argjson cost "$5" \
    --arg a_with "$6" --arg a_without "$7" --arg b_with "$8" --arg b_without "$9" '
    def run($v): ($v | split(" ") | map(. == "1")) as [$task, $comply, $cite, $fired]
      | { costUsd: 0.25, turns: 3, error: null, tracePath: ($root + $sep + "kept" + $sep + "trace.jsonl"),
          graders: [ { name: "task-answers", passed: $task }, { name: "comply-owner", passed: $comply },
                     { name: "cite-record", passed: $cite }, { name: "fired-lookup", passed: $fired } ] };
    def arms($with; $without): if $without == "" then { with: [run($with)] }
      else { with: [run($with)], without: [run($without)] } end;
    { partial: false, claudeVersion: "2.1.292", costUsd: $cost,
      suite: { root: $root, ablation: $ablation, modelOverride: "claude-opus-5-5",
               plugins: [ { name: "fixture", version: "0.1.0", path: $root } ] },
      cases: [ { name: "case-a", dir: ($root + $sep + "evals" + $sep + "house" + $sep + "case-a"), graders: [],
                 arms: arms($a_with; $a_without) },
               { name: "case-b", dir: ($root + $sep + "evals" + $sep + "control" + $sep + "case-b"), graders: [],
                 arms: arms($b_with; $b_without) } ] }' >"$1"
}
stage_result "$results/plugin.json" with-without 'C:\Users\Some One\stage\plugin' "\\" 1.25 \
  '1 1 1 1' '1 0 0 0' '1 1 1 1' '1 1 0 0'
stage_result "$results/prose.json" none 'C:/Users/Some One/stage/prose' / 0.5 '1 0 0 0' '' '1 1 0 0' ''
stage_result "$results/hook.json" none '/Users/someone/stage/hook' / 0.25 '1 1 1 1' '' '1 1 1 1' ''

# score.sh
both 'score.sh' score sh "$scripts/score.sh" -k "$results/hook.json" "$results/plugin.json" "$results/prose.json" \
  '{out}'
aggregate=$dir/score-crlf/aggregate.md
has "$aggregate" '| `claude_version` | 2.1.292 |' 'score.sh reads the Claude Code version'
has "$aggregate" '| `cases` | 2 |' 'score.sh counts the cases'
has "$aggregate" '| `house.plugin.sessions` | 1 |' 'score.sh reads a case kind from a path with backslashes'
has "$aggregate" '| `control.prose.sessions` | 1 |' 'score.sh reads a case kind from a Windows path with slashes'
has "$aggregate" '| `house.hook.sessions` | 1 |' 'score.sh reads a case kind from a POSIX path'
has "$aggregate" '| `house.lead_over_prose` | 1.00 |' "score.sh gives the plugin arm's lead over prose"
has "$aggregate" '| `house.lead_over_none` | 1.00 |' "score.sh gives the plugin arm's lead over none"
has "$aggregate" '| `control.lead_over_none` | 0.00 |' "score.sh gives the plugin arm's lead on the controls"
has "$aggregate" '| `cost_usd` | 2.00 |' 'score.sh adds the cost of every stage'
stripped=$dir/score-crlf/results
if ! [ -f "$stripped/plugin-stage.json" ] || ! [ -f "$stripped/prose-stage.json" ] ||
  ! [ -f "$stripped/hook-stage.json" ]; then
  fail 'score.sh wrote no result files'
elif grep -l -e 'Some One' -e 'someone' -e 'C:' "$stripped/"*.json; then
  fail 'score.sh leaves a path naming the user in the result files above'
else
  pass 'score.sh cuts every Windows and POSIX path to its last part'
fi

# merge.sh, over two batches of the plugin stage.
both 'merge.sh' merge sh "$scripts/merge.sh" '{out}/merged.json' "$results/plugin.json" "$results/plugin.json"
if [ "$(jq '.cases[0].arms.with | length' "$dir/merge-crlf/merged.json")" = 2 ]; then
  pass 'merge.sh gives each case the runs of both batches'
else
  fail 'merge.sh does not give each case the runs of both batches'
fi

# regrade.mjs, with a grader and a trace that have CRLF line endings, and a case folder named with backslashes.
if command -v node >/dev/null; then
  kept=$fx/kept/run-1
  mkdir -p "$kept/sealed/home/cwd" "$kept/trace" "$fx/regrade-cases/house/case-a/graders"
  printf -- "---\ntype: regex\npattern: 'std-A'\n---\n\nCites the record.\n" | awk '{ printf "%s\r\n", $0 }' \
    >"$fx/regrade-cases/house/case-a/graders/cite-record.md"
  printf '%s\n' '{"type":"assistant","message":{"content":[{"type":"text","text":"See std-A."}]}}' \
    '{"type":"result","result":"See std-A."}' | awk '{ printf "%s\r\n", $0 }' >"$kept/trace/trace.jsonl"
  jq -n --arg trace "$kept/trace/trace.jsonl" '{ suite: { ablation: "none" }, cases: [ {
      name: "case-a", dir: "C:\\Users\\Some One\\stage\\prose\\evals\\house\\case-a",
      graders: [ { name: "cite-record", type: "regex",
                   config: { target: "last_message", pattern: "std-A", flags: "", match: "contains" } } ],
      arms: { with: [ { tracePath: $trace, graders: [ { name: "cite-record", passed: true } ] } ] } } ] }' \
    >"$fx/regrade-in.json"
  if node "$scripts/regrade.mjs" "$fx/regrade-cases" "$fx/regrade-in.json" "$dir/regrade-out.json" \
    >"$dir/regrade.stdout" 2>&1; then
    has "$dir/regrade.stdout" '1 unchanged verdict(s) agree with the tool, 0 disagree; 0 grader definition(s)' \
      'regrade.mjs reads a CRLF grader and agrees with the tool'
  else
    fail 'regrade.mjs stopped'; cat "$dir/regrade.stdout"
  fi
else
  echo "skip regrade.mjs: node is not installed"
fi

[ "$failed" = 0 ] && echo "every case passed"
exit "$failed"
