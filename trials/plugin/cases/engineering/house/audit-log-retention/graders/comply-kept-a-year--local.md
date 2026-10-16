---
type: regex
target: { source: file, path: infra/logging.tf }
pattern: '\b(\w+)[ \t]*=[ \t]*(?:36[5-9]|3[7-9][0-9]|[4-9][0-9]{2}|[1-9][0-9]{3,})\b[\s\S]*retention_in_days[ \t]*=[ \t]*local\.\1\b|retention_in_days[ \t]*=[ \t]*local\.(\w+)\b[\s\S]*\b\2[ \t]*=[ \t]*(?:36[5-9]|3[7-9][0-9]|[4-9][0-9]{2}|[1-9][0-9]{3,})\b'
---
