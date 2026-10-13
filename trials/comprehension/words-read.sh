#!/bin/sh
# Counts the words a reader agent was handed by its tools, from its Claude Code transcript.
set -eu

if [ $# -ne 1 ]; then
  echo "usage: words-read.sh <agent-transcript.jsonl>" >&2
  exit 2
fi

# Every tool result counts as read, less the line number and tab the Read tool puts before each line.
tab=$(printf '\t')
jq -r '
  select(.type == "user") | .message.content
  | if type == "array" then .[] else empty end
  | select(.type == "tool_result") | .content
  | if type == "string" then . else (map(select(.type == "text") | .text) | join("\n")) end
' "$1" | sed -E "s/^ *[0-9]+(${tab}|→)//" | wc -w | awk '{ print $1 }'
