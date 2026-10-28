---
type: regex
target: { source: file, path: .github/workflows/ci.yml }
pattern: '^(?=[\s\S]*NETTST[\]`.\s]*COVER)[\s\S]*(--collect|--coverage|CollectCoverage|dotnet-coverage)'
---
