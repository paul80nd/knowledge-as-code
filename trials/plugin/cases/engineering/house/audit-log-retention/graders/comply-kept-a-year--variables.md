---
type: regex
target: { source: file, path: infra/variables.tf }
pattern: 'retention[\s\S]{0,300}?default[ \t]*=[ \t]*(36[5-9]|3[7-9][0-9]|[4-9][0-9]{2}|[1-9][0-9]{3,})\b'
---
