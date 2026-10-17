---
type: regex
target: { source: file, path: deploy/label-printer.yaml }
pattern: 'image:[ \t]*[''"]?[^\s''"]*@(sha256:|\$\{?[A-Za-z_]\w*\}?|[A-Z_]*DIGEST\b)'
---
