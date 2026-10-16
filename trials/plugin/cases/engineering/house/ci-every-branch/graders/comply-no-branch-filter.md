---
type: regex
target: { source: file, path: .github/workflows/ci.yml }
pattern: '\n([ \t]+)push:[ \t]*\n(?:\1[ \t]+[^\n]*\n)*?\1[ \t]+branches:(?![ \t]*\[?[ \t]*[''"]?\*\*)(?![ \t]*\n[ \t]+-[ \t]*[''"]?\*\*)'
match: not_contains
---
