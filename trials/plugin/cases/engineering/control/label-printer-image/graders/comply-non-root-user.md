---
type: regex
target: { source: file, path: Dockerfile }
pattern: '(^|\n)[ \t]*USER[ \t]+(?!root\b|0\b|0:)\S+(?![\s\S]*\n[ \t]*(FROM|USER)\b)'
---
