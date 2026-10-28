#!/bin/sh
# Writes the prose arm's CLAUDE.md from a corpus export: every record of every type the export contains.
#
#   prose-claude-md.sh <export dir> <output file>
#
# <export dir> is what `kac export` wrote, or the corpus/ folder of the plugin `kac bundle` built. The types, their
# folders and their parts files come from its manifest.json, so nothing here knows a type by name.
# shellcheck disable=SC2016 # each $name in a jq program is a jq variable
set -eu

if [ "$#" -ne 2 ]; then
  echo "usage: prose-claude-md.sh <export dir> <output file>" >&2
  exit 2
fi

export_dir=$1
out=$2

# Prints what a command wrote with every carriage return removed, because a Windows jq writes CRLF.
lf() { lf_out=$("$@") || return; [ -z "$lf_out" ] || printf '%s\n' "$lf_out" | tr -d '\r'; }

manifest="$export_dir/manifest.json"

if [ ! -f "$manifest" ]; then
  echo "prose-claude-md.sh: $export_dir has no manifest.json" >&2
  exit 1
fi

# The keys every line or record has for the tool's own use, which a reader of the prose does not need.
bookkeeping='["type", "path", "anchor", "links", "trust", "status", "reviewBy", "review-by", "record", "part", "id",
  "title", "shortcode"]'

# A field as one line of prose: a list joined with commas, an object as JSON, a null left out.
render_fields='def one: if type == "object" then tojson else tostring end;
  def show:
    if type == "array" and length > 0 and all(type == "object" and has("text"))
    then "\n" + (map("- \(.text)") | join("\n"))
    elif type == "array" then " " + (map(one) | join(", "))
    else " " + one end;
  to_entries
  | map(select(.value != null and (.key as $k | $skip | index($k) | not)))
  | map("\(.key):\(.value | show)") | .[]'

name=$(lf jq -r '.about.displayName // .corpus' "$manifest")

# The id of every part written, so a part no record claimed fails the run.
written=$(mktemp)
trap 'rm -f "$written"' EXIT

{
  printf '# The knowledge of %s\n\n' "$name"
  printf 'This is what the corpus records, type by type. Each heading gives the id to cite it by.\n'

  jq -c '.types[]' "$manifest" | tr -d '\r' | while IFS= read -r t; do
    type=$(lf jq -rn --argjson t "$t" '$t.type')
    dir="$export_dir/$(lf jq -rn --argjson t "$t" '$t.dir')"
    parts_file=$(lf jq -rn --argjson t "$t" '$t.partsFile // empty')
    record_key=$(lf jq -rn --argjson t "$t" '$t.recordKey // empty')
    frameworks_file=$(lf jq -rn --argjson t "$t" '$t.frameworksFile // empty')

    printf '\n## %s\n' "$type"

    find "$dir" -name '*.json' | sort | while IFS= read -r file; do
      id=$(lf jq -r '.fields.id' "$file")
      jq -r --argjson skip "$bookkeeping" "
        \"\n### \(.fields.title // .fields.id) (\`\(.fields.id)\`)\n\",
        \"Status: \(.fields.status // \"none\").\",
        (.fields | $render_fields),
        (.links.human // empty | \"Source: <\(.)>\"),
        (.sections // {} | to_entries[] | \"\n#### \(.key)\n\n\(.value)\")
      " "$file"

      # A record a layer brought in sits in a folder named for it, and its parts name it as <layer>:<id>.
      case ${file#"$dir"/} in
        */*) owner="${file#"$dir"/}" && owner="${owner%%/*}:$id" ;;
        *) owner=$id ;;
      esac

      if [ -n "$parts_file" ] && [ -n "$record_key" ]; then
        jq -r --arg key "$record_key" --arg owner "$owner" \
          'select(.[$key] == $owner) | .id' "$export_dir/$parts_file" >> "$written"
        jq -r --arg id "$owner" --arg key "$record_key" --argjson skip "$bookkeeping" "
          select(.[\$key] == \$id) |
          \"\n#### \(if .title then \"\(.title) (\`\(.id)\`)\" else \"\`\(.id)\`\" end)\n\",
          (. | $render_fields)
        " "$export_dir/$parts_file"
      fi
    done

    if [ -n "$frameworks_file" ]; then
      printf '\n### Frameworks this type cites\n\n'
      jq -r --argjson skip "$bookkeeping" "\"- \" + ([$render_fields] | join(\"; \"))" "$export_dir/$frameworks_file"
    fi
  done
} > "$out"

# Takes out the carriage returns a Windows jq writes, so the prose arm's text is the same on every machine.
tr -d '\r' < "$out" > "$out.lf"
mv "$out.lf" "$out"

expected=$(lf jq '[.types[].parts] | add' "$manifest")
actual=$(wc -l < "$written" | tr -d ' ')
if [ "$actual" -ne "$expected" ]; then
  echo "prose-claude-md.sh: the manifest counts $expected parts, and $actual were written" >&2
  exit 1
fi
