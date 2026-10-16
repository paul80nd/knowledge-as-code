---
type: regex
target: { source: file, path: .github/workflows/ci.yml }
pattern: '(--locked|uv lock --check|UV_LOCKED)'
---
