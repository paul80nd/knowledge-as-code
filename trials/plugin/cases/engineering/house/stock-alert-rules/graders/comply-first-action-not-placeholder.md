---
type: regex
target: { source: file, path: monitoring/alerts.yml }
pattern: '([Rr]unbook\w*|(\b|_)[Aa]ction)[''"]?[ \t]*:[ \t]*(>-?|\|-?)?\s*[''"]?(TODO|TBD|FIXME|<)'
match: not_contains
---
