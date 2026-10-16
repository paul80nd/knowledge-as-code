#!/bin/sh
# Takes one reading of the capture trial: every count, from git and the corpus's tracker, as of one date.
# shellcheck disable=SC2016 # count() takes an awk condition, so each $n in one is an awk field
set -eu

usage() {
  cat >&2 <<'EOF'
usage: capture.sh -c <corpus> -d <as-of> -r <1|2> -o <owner-emails> [-x <excluded-emails>]
                  [-b <branch>] [-f <findings-file>] [-l <question-log>] <empty-directory>
EOF
  exit 2
}

fail() { echo "capture.sh: $*" >&2; exit 2; }

# Every path git prints is printed as it is spelled, so a name outside ASCII matches itself.
git() { command git -c core.quotePath=false "$@"; }

corpus='' asof='' reading='' owners='' excluded='' branch='' findings_file='' log_file=''
while getopts c:d:r:o:x:b:f:l: opt; do
  case $opt in
    c) corpus=$OPTARG ;;
    d) asof=$OPTARG ;;
    r) reading=$OPTARG ;;
    o) owners=$OPTARG ;;
    x) excluded=$OPTARG ;;
    b) branch=$OPTARG ;;
    f) findings_file=$OPTARG ;;
    l) log_file=$OPTARG ;;
    *) usage ;;
  esac
done
shift $((OPTIND - 1))
[ $# -eq 1 ] || usage
out=$1

[ -n "$corpus" ] && [ -n "$asof" ] && [ -n "$reading" ] && [ -n "$owners" ] || usage
case $reading in 1|2) ;; *) fail "-r takes 1 or 2" ;; esac
command -v jq >/dev/null || fail "jq is not installed"
command -v git >/dev/null || fail "git is not installed"

# From git 2.31, `git log --first-parent` lists what a merge changed, which the landings and the moves read.
git_version=$(command git --version | tr -d '\r')
git_number=$(echo "$git_version" | sed -E 's/^git version ([0-9]+)\.([0-9]+).*$/\1 \2/')
git_major=${git_number%% *} git_minor=${git_number#* }
{ [ "$git_major" -gt 2 ] || { [ "$git_major" -eq 2 ] && [ "$git_minor" -ge 31 ]; }; } 2>/dev/null ||
  fail "this is $git_version. The script needs git 2.31 or later. Update git, and run it again."

work=$(mktemp -d "${TMPDIR:-/tmp}/capture.XXXXXX")
trap 'rm -rf "$work"' EXIT

# Prints what a command wrote with every carriage return removed, because a Windows jq or gh can write CRLF.
lf() { "$@" >"$work/lf" || return; tr -d '\r' <"$work/lf"; }
jq() { lf command jq "$@"; }

# Every date is a UTC calendar day, and the reading includes the whole of its as-of day.
day_of() { echo $(($1 / 86400)); }
date_of() { jq -rn --argjson e "$1" '$e | todate[:10]'; }
epoch_of() {
  echo "$1" | grep -Eq '^[0-9]{4}-[0-9]{2}-[0-9]{2}$' || return 1
  e=$(jq -n --arg d "$1" '($d + "T00:00:00Z") | fromdateiso8601' 2>/dev/null) || return 1
  [ "$(date_of "$e")" = "$1" ] || return 1
  echo "$e"
}
asof_start=$(epoch_of "$asof") || fail "-d takes a real date as YYYY-MM-DD"
asof_day=$(day_of "$asof_start")

# The question log runs for the 14 days starting on the as-of day, so the script waits for its last day.
log_last=$(date_of $((asof_start + 13 * 86400)))
if [ -n "$log_file" ] && [ "$(day_of "$(date -u +%s)")" -lt $((asof_day + 13)) ]; then
  fail "the question log runs to $log_last, a day in UTC. Run the script on or after that day."
fi

[ -f "$corpus/.corpus.yaml" ] || fail "no .corpus.yaml in $corpus"
[ -z "$findings_file" ] || [ -f "$findings_file" ] || fail "no findings file at $findings_file"
[ -z "$log_file" ] || [ -f "$log_file" ] || fail "no question log at $log_file"

mkdir -p "$out"
[ -z "$(ls -A "$out")" ] || fail "$out is not empty"
out=$(cd "$out" && pwd)
[ -z "$findings_file" ] || findings_file=$(cd "$(dirname "$findings_file")" && pwd)/$(basename "$findings_file")
[ -z "$log_file" ] || log_file=$(cd "$(dirname "$log_file")" && pwd)/$(basename "$log_file")
raw=$out/raw
mkdir "$raw"

top=$(git -C "$corpus" rev-parse --show-toplevel) || fail "$corpus is not inside a git repository"
prefix=$(git -C "$corpus" rev-parse --show-prefix)
spec=${prefix:-.}
cd "$top"

if [ -z "$branch" ]; then
  branch=$(git symbolic-ref --quiet --short refs/remotes/origin/HEAD) ||
    fail "origin/HEAD is not set. Run 'git remote set-head origin --auto', or pass -b origin/<branch>."
fi
git rev-parse --verify --quiet "$branch^{commit}" >/dev/null || fail "no branch $branch. Pass -b origin/<branch>."

snapshot=$(git rev-list -1 --first-parent --before="$asof 23:59:59 +0000" "$branch") || fail "git rev-list failed"
[ -n "$snapshot" ] || fail "$branch has no commit on or before $asof"
git cat-file -e "$snapshot:${prefix}.corpus.yaml" 2>/dev/null || fail "the corpus has no .corpus.yaml at $asof"

# Reads a corpus descriptor on stdin and prints one key and value per line, tab-separated.
parse_descriptor() {
  awk '
    function clean(v) {
      sub(/[ \t]+#.*$/, "", v); gsub(/^[ \t]+|[ \t]+$/, "", v)
      gsub(/^["\047]|["\047]$/, "", v); return v
    }
    function emit(section, key, v) {
      v = clean(v)
      if (section == "types") { if (v ~ /^[a-z0-9-]+$/) print "type\t" v; else wrong(); return }
      if (key == "target" || key == "base") print section "-" key "\t" v
    }
    # A line under types: that is not a list of type names, which the run stops at.
    function wrong() { print "error\t" NR "\t" $0 }
    function flow(section, v,   n, parts, i, at) {
      v = clean(v)
      if (section == "types" && v !~ /^\[.*\]$/) { wrong(); return }
      gsub(/^[\[{][ \t]*|[ \t]*[\]}]$/, "", v)
      n = split(v, parts, ",")
      for (i = 1; i <= n; i++) {
        if (section == "types") { emit(section, "", parts[i]); continue }
        at = match(parts[i], /:[ \t]/)
        if (at) emit(section, clean(substr(parts[i], 1, at - 1)), substr(parts[i], at + 1))
      }
    }
    { sub(/\r$/, "") }
    /^[ \t]*#/ || /^[ \t]*$/ { next }
    /^[a-z-]+:/ {
      at = index($0, ":"); key = substr($0, 1, at - 1); value = clean(substr($0, at + 1)); section = ""
      if (key == "corpus" || key == "publishing-target") { print key "\t" value; next }
      if (key == "publishing" || key == "tracker" || key == "types") {
        section = key
        if (value != "") { flow(section, value); section = "" }
      }
      next
    }
    section == "types" && /^[ \t]+- / { v = $0; sub(/^[ \t]+- /, "", v); emit("types", "", v); next }
    section == "types" { wrong(); next }
    section != "" && /^[ \t]+[a-z-]+:/ {
      v = $0; sub(/^[ \t]+/, "", v); at = index(v, ":")
      emit(section, substr(v, 1, at - 1), substr(v, at + 1))
    }
  '
}

git show "$snapshot:${prefix}.corpus.yaml" >"$work/descriptor" || fail "git could not read the descriptor at $asof"
parse_descriptor <"$work/descriptor" >"$raw/descriptor.tsv"
parse_descriptor <"$top/${prefix}.corpus.yaml" >"$raw/descriptor-today.tsv"
field() { awk -F'\t' -v k="$1" '$1 == k { print $2; exit }' "$raw/descriptor-today.tsv"; }
corpus_name=$(field corpus)
bad_type=$(awk -F'\t' '$1 == "error" { print "line " $2 ": " $3; exit }' "$raw/descriptor.tsv")
[ -z "$bad_type" ] || fail "the descriptor at $asof has $bad_type. List each type under types: as '  - <name>', \
in lower-case letters, digits and hyphens."
types=$(awk -F'\t' '$1 == "type" { print $2 }' "$raw/descriptor.tsv" | sort -u)
[ -n "$types" ] || fail "the corpus adopted no types at $asof"

# Every type any version of the descriptor adopted, so a move out of a type later dropped still counts.
git rev-list --first-parent "$snapshot" -- "${prefix}.corpus.yaml" >"$work/versions" || fail "git rev-list failed"
: >"$work/all-types"
while read -r c; do
  git show "$c:${prefix}.corpus.yaml" >"$work/version" 2>/dev/null || continue
  parse_descriptor <"$work/version" | awk -F'\t' '$1 == "type" { print $2 }' >>"$work/all-types"
done <"$work/versions"
all_types=$(sort -u "$work/all-types")

# The tracker the descriptor states today, from its tracker block or derived from publishing as `kac export` does.
project_of() { echo "$1" | sed -E 's#/_(git|wiki)/.*$##; s#/+$##'; }
pub_target=$(field publishing-target)
pub_base=$(field publishing-base)
tracker_target=$(field tracker-target)
tracker_base=$(field tracker-base)
case $pub_target in
  github) derived=github derived_base=$pub_base ;;
  azure-devops|azure-devops-wiki) derived=azure-devops derived_base=$(project_of "$pub_base") ;;
  *) derived=none derived_base='' ;;
esac
[ -n "$tracker_target" ] || tracker_target=$derived
if [ -z "$tracker_base" ] && [ "$tracker_target" = "$derived" ]; then tracker_base=$derived_base; fi
case $tracker_target in
  github|none) ;;
  azure-devops)
    tracker_base=$(project_of "$tracker_base")
    [ -n "$findings_file" ] ||
      fail "the tracker is on Azure DevOps, which the script cannot query. Pass -f, as the README says."
    ;;
  *) fail "the descriptor states a tracker target of '$tracker_target'. It takes github, azure-devops or none." ;;
esac

# A record is a Markdown file in an adopted type's folder whose name does not start with an underscore.
is_record_awk='
  BEGIN { n = split(types, list, " "); for (i = 1; i <= n; i++) adopted[list[i]] = 1; plen = length(prefix) }
  function type_of(path,   rest, slash, base) {
    if (substr(path, 1, plen) != prefix) return ""
    rest = substr(path, plen + 1); slash = index(rest, "/")
    if (!slash || !(substr(rest, 1, slash - 1) in adopted)) return ""
    base = rest; sub(/^.*\//, "", base)
    if (base !~ /\.md$/ || base ~ /^_/) return ""
    return substr(rest, 1, slash - 1)
  }
'
types_line=$(echo "$types" | tr '\n' ' ')
all_types_line=$(echo "$all_types" | tr '\n' ' ')
tab=$(printf '\t')
lower() { tr '[:upper:]' '[:lower:]'; }
owners_line=$(echo "$owners" | tr ',' ' ' | lower)
excluded_line=$(echo "$excluded" | tr ',' ' ' | lower)
role_of() {
  for e in $owners_line; do [ "$1" = "$e" ] && { echo owner; return; }; done
  for e in $excluded_line; do [ "$1" = "$e" ] && { echo excluded; return; }; done
  echo other
}
# Prints the last line git printed, in lower case, and stops the run where git failed.
last_author() { "$@" >"$work/author" || fail "git failed: $*"; tail -n 1 "$work/author" | lower; }

# Each address under -o and -x made a commit to the corpus on the branch, so a mistyped one stops the run.
git log --format='%aE' "$branch" -- "$spec" >"$work/ever" || fail "git log failed"
lower <"$work/ever" | sort -u >"$work/ever-lower"
for e in $owners_line; do
  grep -qxF "$e" "$work/ever-lower" ||
    fail "-o gives $e, which made no commit to the corpus. List the addresses as the README says."
done
for e in $excluded_line; do
  grep -qxF "$e" "$work/ever-lower" ||
    fail "-x gives $e, which made no commit to the corpus. List the addresses as the README says."
done

# Every commit to the corpus folder up to the snapshot, and a check that the owner made at least one of them.
git log --format='%aE %at' "$snapshot" -- "$spec" >"$work/commits" || fail "git log failed"
lower <"$work/commits" >"$work/commits-lower"
owner_found=''
for e in $owners_line; do
  awk -v e="$e" '$1 == e { f = 1 } END { exit !f }' "$work/commits-lower" && owner_found=yes
done
[ -n "$owner_found" ] || fail "no commit to the corpus up to $asof is by an address under -o."

# The records at the snapshot, each with the author of the commit that first added it, followed across renames.
# `git log --follow` also follows a copy, which would credit a copied record to the author of its source.
git log -M --diff-filter=R --name-status --format=@%h "$snapshot" -- "$spec" >"$work/all-renames" ||
  fail "git log failed"
awk -F'\t' '/^R/ { print $3 "\t" $2 }' "$work/all-renames" >"$work/renamed-from"
origin_of() {
  p=$1 i=0
  while [ $i -lt 100 ]; do
    from=$(awk -F'\t' -v p="$p" '$1 == p { print $2; exit }' "$work/renamed-from")
    [ -n "$from" ] && [ "$from" != "$p" ] || break
    p=$from i=$((i + 1))
  done
  echo "$p"
}
git ls-tree -r --name-only "$snapshot" -- "$spec" >"$work/tree" || fail "git ls-tree failed"
awk -v types="$types_line" -v prefix="$prefix" "$is_record_awk"'{ t = type_of($0); if (t != "") print t "\t" $0 }' \
  "$work/tree" >"$work/records"
printf 'type\tpath\tauthor\trole\n' >"$raw/records.tsv"
while IFS="$tab" read -r type path; do
  author=$(last_author git log --diff-filter=A --format=%aE "$snapshot" -- "$(origin_of "$path")")
  printf '%s\t%s\t%s\t%s\n' "$type" "${path#"$prefix"}" "$author" "$(role_of "$author")"
done <"$work/records" >>"$raw/records.tsv"

# Each record as it landed on the default branch, with the commit that added it and the commit that landed it.
git log --first-parent --diff-filter=A --name-only --format='@%H %ct' "$snapshot" -- "$spec" >"$work/added" ||
  fail "git log failed"
awk -v types="$types_line" -v prefix="$prefix" "$is_record_awk"'
  /^@/ { split(substr($0, 2), h, " "); next }
  NF { t = type_of($0); if (t != "") print h[1] "\t" h[2] "\t" t "\t" $0 }
' "$work/added" >"$work/landings"
printf 'landed\tauthor\ttype\tpath\n' >"$raw/landings.tsv"
while IFS="$tab" read -r commit landed type path; do
  if git rev-parse --verify --quiet "$commit^" >/dev/null; then
    author=$(last_author git log --diff-filter=A --format=%aE "$commit^..$commit" -- "$path")
  else
    author=$(last_author git log -1 --format=%aE "$commit")
  fi
  printf '%s\t%s\t%s\t%s\n' "$landed" "$author" "$type" "${path#"$prefix"}"
done <"$work/landings" >>"$raw/landings.tsv"

# Each person who committed to the corpus: their first commit to it, and their first record to land.
awk '{ if (!($1 in first) || $2 < first[$1]) first[$1] = $2 } END { for (a in first) print a, first[a] }' \
  "$work/commits-lower" | sort >"$work/firsts"
printf 'author\trole\tfirst_commit\tfirst_record\tdays\n' >"$raw/authors.tsv"
while read -r author first; do
  landed=$(awk -F'\t' -v a="$author" 'NR > 1 && $2 == a { print $1 }' "$raw/landings.tsv" | sort -n | head -n 1)
  if [ -n "$landed" ]; then
    printf '%s\t%s\t%s\t%s\t%s\n' "$author" "$(role_of "$author")" "$(date_of "$first")" "$(date_of "$landed")" \
      "$(($(day_of "$landed") - $(day_of "$first")))"
  else
    printf '%s\t%s\t%s\t\t\n' "$author" "$(role_of "$author")" "$(date_of "$first")"
  fi
done <"$work/firsts" >>"$raw/authors.tsv"

# The people who added or changed a record, from every commit reachable from the snapshot. Only the address is
# lowered, so a path matches a type's folder exactly as it does for the records.
git log --format='@%aE' --name-only "$snapshot" -- "$spec" >"$work/touched" || fail "git log failed"
awk -v types="$types_line" -v prefix="$prefix" "$is_record_awk"'
  /^@/ { a = tolower(substr($0, 2)); next }
  NF && type_of($0) != "" { print a }
' "$work/touched" | sort -u >"$work/record-authors"

# Each rename that took a record from one type's folder to another's, between consecutive commits on the branch.
git log --first-parent -M --diff-filter=R --name-status --format='@%ct' "$snapshot" -- "$spec" >"$work/renames" ||
  fail "git log failed"
awk -v types="$all_types_line" -v prefix="$prefix" "$is_record_awk"'
  /^@/ { when = substr($0, 2); next }
  /^R/ {
    split($0, f, "\t"); from = type_of(f[2]); to = type_of(f[3])
    if (from != "" && to != "" && from != to) print when "\t" from "\t" to "\t" f[2] "\t" f[3]
  }
' "$work/renames" >"$work/moves"
printf 'date\tfrom\tto\told\tnew\n' >"$raw/moves.tsv"
while IFS="$tab" read -r when from to old new; do
  printf '%s\t%s\t%s\t%s\t%s\n' "$(date_of "$when")" "$from" "$to" "${old#"$prefix"}" "${new#"$prefix"}"
done <"$work/moves" >>"$raw/moves.tsv"

# Every address a record cites as a resource in its frontmatter, normalised so two spellings of one issue match.
# An Azure DevOps address on <org>.visualstudio.com is written as its dev.azure.com/<org> form.
normalise() {
  lower | sed -E 's#^[a-z]+://##; s#^www\.##; s#^([a-z0-9-]+)\.visualstudio\.com/#dev.azure.com/\1/#
    s# #%20#g; s#/+$##; s#\.git$##; s#/+$##'
}
printf 'path\tresource\n' >"$raw/sources.tsv"
awk -F'\t' 'NR > 1 { print $2 }' "$raw/records.tsv" >"$work/paths"
while read -r path; do
  git show "$snapshot:$prefix$path" >"$work/record" || fail "git could not read $path"
  awk '
    { sub(/\r$/, "") }
    NR == 1 && $0 != "---" { exit }
    NR > 1 && $0 == "---" { exit }
    /resource:/ {
      v = $0; sub(/.*resource:[ \t]*/, "", v); sub(/[ \t]*[,}].*$/, "", v); gsub(/^["\047]|["\047]$/, "", v)
      if (v ~ /^https?:\/\//) print v
    }
  ' "$work/record" | normalise | sed "s#^#$path$tab#"
done <"$work/paths" >>"$raw/sources.tsv"

# The findings filed on or before the as-of day: from GitHub where the tracker is there, or from the file given.
printf 'created\turl\trecord\n' >"$raw/findings.tsv"
findings_source='none'
if [ -n "$findings_file" ]; then
  findings_source='file'
  tr -d '\r' <"$findings_file" | grep -Ev '^[[:space:]]*(#|$)' >"$work/file-lines" || true
  awk -F'\t' -v d="$asof" '
    function wrong() { bad = 1; print "line " NR ": " $0 > "/dev/stderr" }
    NF != 2 || $1 !~ /^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$/ { wrong(); next }
    $1 <= d { print $1 "\t" $2 }
    END { exit bad }
  ' "$work/file-lines" >"$work/filed" ||
    fail "each line of the findings file takes a date, a tab and an address. Fix the lines above."
elif [ "$tracker_target" = github ]; then
  command -v gh >/dev/null || fail "the tracker is on GitHub and gh is not installed. Install it, or pass -f."
  repo=$(echo "$tracker_base" | sed -E 's#^[a-z]+://(www\.)?##; s#/+$##; s#\.git$##; s#/+$##')
  echo "$repo" | grep -Eq '^[^/]+/[^/]+/[^/]+$' ||
    fail "the tracker base '$tracker_base' is not a repository address. Pass -f."
  host=${repo%%/*}
  gh auth status --hostname "$host" >/dev/null 2>&1 ||
    fail "gh is not signed in to $host. Run 'gh auth login --hostname $host', or pass -f."
  findings_source='github'
  lf gh issue list --repo "$repo" --state all --limit 1000 --label kac:finding \
    --search "created:<=$asof" --json url,createdAt,body >"$work/labelled" ||
    fail "gh could not list the issues on $repo. Check that you can read them, or pass -f."
  lf gh issue list --repo "$repo" --state all --limit 1000 --search "kac-finding in:body created:<=$asof" \
    --json url,createdAt,body >"$work/unlabelled" ||
    fail "gh could not search the issues on $repo. Check that you can read them, or pass -f."
  jq -rs --arg corpus "$corpus_name" '
    (.[0] + (.[1] | map(select(.body | test("^\\s*```yaml kac-finding"))))) | unique_by(.url) | .[]
    | ((.body | capture("```yaml kac-finding\\r?\\n(?<b>[\\s\\S]*?)```").b // "")
       | capture("(?m)^corpus:[ \\t]*(?<c>[^ \\t\\r\\n]+)").c // "") as $named
    | select($named == "" or $named == $corpus)
    | [.createdAt[:10], .url] | @tsv
  ' "$work/labelled" "$work/unlabelled" >"$work/filed" || fail "jq could not read what gh returned"
  for f in labelled unlabelled; do
    [ "$(jq length "$work/$f")" -lt 1000 ] || fail "gh returned 1000 issues, its limit, so some were left out. Pass -f."
  done
fi
if [ "$findings_source" != none ]; then
  sort "$work/filed" >"$work/filed-sorted"
  while IFS="$tab" read -r created url; do
    key=$(echo "$url" | normalise)
    record=$(awk -F'\t' -v k="$key" 'NR > 1 && $2 == k { print $1; exit }' "$raw/sources.tsv")
    printf '%s\t%s\t%s\n' "$created" "$url" "$record"
  done <"$work/filed-sorted" >>"$raw/findings.tsv"
fi

# The question log, kept for the 14 days from the as-of day. A spreadsheet can save a byte-order mark before line 1.
questions='' linked=''
if [ -n "$log_file" ]; then
  bom=$(printf '\357\273\277')
  tr -d ' \r' <"$log_file" | LC_ALL=C sed "1s/^$bom//" >"$work/log-lines"
  log_status=0
  awk -F, -v a="$asof" -v z="$log_last" '
    function say(what) { print "capture.sh: " what > "/dev/stderr" }
    /^#/ || /^$/ || /^date,/ { next }
    NF != 2 || $1 !~ /^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$/ || ($2 != "yes" && $2 != "no") {
      bad = 1; say("line " NR " of the question log: " $0); next
    }
    { seen++ }
    $1 < a || $1 > z { say("left out line " NR ", dated " $1 ", which is outside the log"); next }
    { kept++; print }
    END { if (bad) exit 3; if (seen && !kept) exit 4 }
  ' "$work/log-lines" >"$raw/question-log.csv" || log_status=$?
  case $log_status in
    0) ;;
    3) fail "each line of the question log takes a date as YYYY-MM-DD, a comma, and yes or no. Fix the lines above. \
A spreadsheet may have rewritten the dates, as Excel saves 10/03/2026, so edit the log in a text editor." ;;
    4) fail "no line of the question log is dated $asof to $log_last. Check -d, and that -l is this reading's log." ;;
    *) fail "awk could not read the question log" ;;
  esac
  questions=$(wc -l <"$raw/question-log.csv" | tr -d ' ')
  linked=$(grep -c ',yes$' "$raw/question-log.csv" || true)
fi

# The aggregate: counts and rates under the field names the README defines, and nothing else.
rate() { if [ "$2" -gt 0 ] 2>/dev/null; then awk -v a="$1" -v b="$2" 'BEGIN { printf "%.2f", a / b }'; fi; }
count() { awk -F'\t' "NR > 1 && ($1) { n++ } END { print n + 0 }" "$2"; }

first_landed=$(awk -F'\t' 'NR > 1 { print $1 }' "$raw/landings.tsv" | sort -n | head -n 1)
since_first=''
[ -z "$first_landed" ] || since_first=$((asof_day - $(day_of "$first_landed")))

records=$(count 1 "$raw/records.tsv")
by_others=$(count '$4 == "other"' "$raw/records.tsv")
types_adopted=$(echo "$types" | wc -l | tr -d ' ')
types_empty=0
for t in $types; do
  [ "$(count "\$1 == \"$t\"" "$raw/records.tsv")" -gt 0 ] || types_empty=$((types_empty + 1))
done

# The owner is one person however many addresses -o lists.
owner_wrote=0 authors_other=0
while read -r a; do
  case $(role_of "$a") in
    owner) owner_wrote=1 ;;
    other) authors_other=$((authors_other + 1)) ;;
  esac
done <"$work/record-authors"
authors=$((owner_wrote + authors_other))
new_authors=$(count '$2 == "other"' "$raw/authors.tsv")
new_with_record=$(count '$2 == "other" && $5 != ""' "$raw/authors.tsv")
stats=$(awk -F'\t' 'NR > 1 && $2 == "other" && $5 != "" { print $5 }' "$raw/authors.tsv" | sort -n |
  awk 'NF { v[++n] = $1 } END {
    if (!n) { print "\t\t"; exit }
    m = (n % 2) ? v[(n + 1) / 2] : (v[n / 2] + v[n / 2 + 1]) / 2
    print m "\t" v[1] "\t" v[n] }')

filed='' became=''
if [ "$findings_source" != none ]; then
  filed=$(count 1 "$raw/findings.tsv")
  became=$(count '$3 != ""' "$raw/findings.tsv")
fi

{
  printf 'trial\tcapture\n'
  printf 'method\t2\n'
  printf 'reading\t%s\n' "$reading"
  printf 'days_since_first_record\t%s\n' "$since_first"
  printf 'types_adopted\t%s\n' "$types_adopted"
  printf 'types_without_record\t%s\n' "$types_empty"
  printf 'records\t%s\n' "$records"
  for t in $types; do printf 'records.%s\t%s\n' "$t" "$(count "\$1 == \"$t\"" "$raw/records.tsv")"; done
  printf 'records_by_owner\t%s\n' "$(count '$4 == "owner"' "$raw/records.tsv")"
  printf 'records_by_others\t%s\n' "$by_others"
  printf 'records_by_excluded\t%s\n' "$(count '$4 == "excluded"' "$raw/records.tsv")"
  printf 'records_by_others_rate\t%s\n' "$(rate "$by_others" "$records")"
  printf 'authors\t%s\n' "$authors"
  printf 'authors_not_owner\t%s\n' "$authors_other"
  printf 'new_authors\t%s\n' "$new_authors"
  printf 'new_authors_with_record\t%s\n' "$new_with_record"
  printf 'days_to_first_record_median\t%s\n' "$(echo "$stats" | cut -f1)"
  printf 'days_to_first_record_min\t%s\n' "$(echo "$stats" | cut -f2)"
  printf 'days_to_first_record_max\t%s\n' "$(echo "$stats" | cut -f3)"
  printf 'tracker_target\t%s\n' "$tracker_target"
  printf 'findings_source\t%s\n' "$findings_source"
  printf 'findings_filed\t%s\n' "$filed"
  printf 'findings_became_records\t%s\n' "$became"
  printf 'findings_became_records_rate\t%s\n' "$(rate "${became:-0}" "${filed:-0}")"
  printf 'records_moved\t%s\n' "$(count 1 "$raw/moves.tsv")"
  awk -F'\t' 'NR > 1 { n[$2 "." $3]++ } END { for (k in n) print "moved." k "\t" n[k] }' "$raw/moves.tsv" | sort
  printf 'questions_logged\t%s\n' "$questions"
  printf 'questions_answered_with_link\t%s\n' "$linked"
  printf 'questions_answered_with_link_rate\t%s\n' "$(rate "${linked:-0}" "${questions:-0}")"
} >"$work/fields"
awk -F'\t' -v reading="$reading" '
  { k[NR] = "`" $1 "`"; v[NR] = $2; if (length(k[NR]) > w1) w1 = length(k[NR]); if (length($2) > w2) w2 = length($2) }
  END {
    if (w1 < 5) w1 = 5; if (w2 < 5) w2 = 5
    print "# Capture trial, reading " reading "\n"
    printf "| %-" w1 "s | %" w2 "s |\n", "Field", "Value"
    d1 = sprintf("%" (w1 + 2) "s", ""); gsub(/ /, "-", d1); d2 = sprintf("%" (w2 + 1) "s", ""); gsub(/ /, "-", d2)
    print "|" d1 "|" d2 ":|"
    for (i = 1; i <= NR; i++) printf "| %-" w1 "s | %" w2 "s |\n", k[i], v[i]
  }
' "$work/fields" >"$out/aggregate.md"

{
  printf 'branch\t%s\n' "$branch"
  printf 'snapshot\t%s\n' "$snapshot"
  printf 'as_of\t%s\n' "$asof"
  [ -z "$log_file" ] || printf 'log\t%s to %s\n' "$asof" "$log_last"
  [ -z "$first_landed" ] || printf 'first_record\t%s\n' "$(date_of "$first_landed")"
  printf 'corpus\t%s\n' "$corpus_name"
  printf 'tracker\t%s %s\n' "$tracker_target" "$tracker_base"
} >"$raw/reading.tsv"

echo "reading $reading as of $asof at ${snapshot%"${snapshot#????????}"}: wrote $out/aggregate.md and $out/raw/"
