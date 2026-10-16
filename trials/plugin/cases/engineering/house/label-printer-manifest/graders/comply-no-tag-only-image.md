---
type: regex
target: { source: file, path: deploy/label-printer.yaml }
pattern: 'image:[ \t]*[''"]?(?![^\s''"]*@sha256:)[^\s''"]+'
match: not_contains
---
