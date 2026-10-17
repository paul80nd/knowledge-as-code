---
type: regex
target: { source: file, path: tests/test_holds.py }
pattern: '[Dd]eviation[^\n]*(DEV-[0-9]+|issues/[0-9]+|#[0-9]+)|(DEV-[0-9]+|issues/[0-9]+|#[0-9]+)[^\n]*[Dd]eviation'
---
