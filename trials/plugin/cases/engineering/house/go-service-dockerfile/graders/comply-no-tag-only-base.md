---
type: regex
target: { source: file, path: Dockerfile }
pattern: '(^|\n)[ \t]*FROM[ \t]+(--platform=\S+[ \t]+)?(?!\S*@sha256:)[^\s]*[:/.][^\s]*|(^|\n)[ \t]*ARG[ \t]+(\w+)=(?!\S*@sha256:)\S*[:/.]\S*[\s\S]*\n[ \t]*FROM[ \t]+(--platform=\S+[ \t]+)?\$\{?\4\}?(\s|$)'
match: not_contains
---
