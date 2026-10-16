---
type: regex
target: { source: file, path: Dockerfile }
pattern: '(^|\n)[ \t]*FROM[ \t]+(--platform=\S+[ \t]+)?(?!\S*@sha256:)[^\s]*[:/.][^\s]*'
match: not_contains
---
