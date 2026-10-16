---
type: regex
target: { source: file, path: infra/logging.tf }
pattern: 'retention_in_days[ \t]*=[ \t]*(36[5-9]|3[7-9][0-9]|[4-9][0-9]{2}|[1-9][0-9]{3,})\b'
---
