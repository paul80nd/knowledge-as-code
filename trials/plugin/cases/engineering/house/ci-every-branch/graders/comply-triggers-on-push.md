---
type: regex
target: { source: file, path: .github/workflows/ci.yml }
pattern: '(^|\n)["'']?on["'']?:[ \t]*(push\b|\[[^\]\n]*\bpush\b|\n(?:[ \t]+[^\n]*\n)*?[ \t]+push:)'
---
