---
type: regex
target: { source: file, path: tests/test_holds.py }
pattern: '(skip|xfail|deselect)'
match: not_contains
---
