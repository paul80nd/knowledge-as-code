#!/bin/sh
# Writes the prose arm's CLAUDE.md from a corpus export: every standard, in the words the plugin contains.
#
#   prose-claude-md.sh <export dir> <output file>
#
# <export dir> is what `kac export` wrote, or the corpus/ folder of the plugin `kac bundle` built.
set -eu

if [ "$#" -ne 2 ]; then
  echo "usage: prose-claude-md.sh <export dir> <output file>" >&2
  exit 2
fi

export_dir=$1
out=$2
rules="$export_dir/standards/rules.jsonl"

if [ ! -f "$export_dir/manifest.json" ] || [ ! -f "$rules" ]; then
  echo "prose-claude-md.sh: $export_dir has no manifest.json or standards/rules.jsonl" >&2
  exit 1
fi

name=$(jq -r '.about.displayName // .corpus' "$export_dir/manifest.json")

{
  printf '# The standards of %s\n\n' "$name"
  printf 'These are the standards you build to here. Each rule heading gives the id to cite it by.\n'

  # A rule line's record is <shortcode>:<id> where another corpus wrote it, and its file sits under that shortcode.
  jq -r '.record' "$rules" | sort -u | while IFS= read -r record; do
    case $record in
      *:*) file="$export_dir/standards/${record%%:*}/${record#*:}.json" ;;
      *) file="$export_dir/standards/$record.json" ;;
    esac

    jq -r --arg id "$record" '
      "\n## \(.fields.title) (`\($id)`)\n",
      "Status: \(.fields.status). Applies to: \(.fields["applies-to"] // [] | join(", ")).\n",
      (.sections.Summary // empty | "\(.)\n"),
      (.links.human // empty | "Source: <\(.)>")
    ' "$file"

    jq -r --arg id "$record" 'select(.record == $id) |
      "\n### \(.title) (`\(.id)`)\n",
      (.obligations // [] | map("- \(.text)") | join("\n")),
      (.seeAlso // empty | "\nSee also: \(map("`\(.)`") | join(", "))")
    ' "$rules"

    jq -r '.sections["Conformance checklist"] // empty | "\n### Conformance checklist\n\n\(.)"' "$file"
  done
} > "$out"
