---
type: regex
target: { source: file, path: Dockerfile }
pattern: '(^|\n)[ \t]*ARG[ \t]+(\w+)=\S*@sha256:[\s\S]*\n[ \t]*FROM[ \t]+(--platform=\S+[ \t]+)?\$\{?\2\}?(\s|$)'
---
