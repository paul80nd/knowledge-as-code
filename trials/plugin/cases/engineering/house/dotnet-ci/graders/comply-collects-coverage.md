---
type: regex
target: { source: file, path: .github/workflows/ci.yml }
pattern: '(--collect[: =]+[''"]?XPlat Code Coverage|--coverage-output-format[ =]+[''"]?cobertura)'
---
