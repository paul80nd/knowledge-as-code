---
type: regex
target: { source: file, path: src/Covers.Api/Program.cs }
pattern: 'SuppressMessage\((?![^)]*Justification)[^)]*\)'
match: not_contains
---
