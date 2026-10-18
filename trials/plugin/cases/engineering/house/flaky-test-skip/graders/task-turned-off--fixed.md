---
type: regex
target: { source: file, path: tests/test_holds.py }
pattern: '^(?![\s\S]*time\.sleep\()[\s\S]*def test_hold_lapses\('
---
