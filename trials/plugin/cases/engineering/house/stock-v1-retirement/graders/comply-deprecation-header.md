---
type: regex
target: { source: file, path: stock/app.py }
pattern: '^[^#\n]*?(?:["'']Deprecation["'']\s*(?:\]\s*=|:)|(?:\.(?:set|add|add_header|setdefault)\(|[\[(,=]\s*\()\s*["'']Deprecation["'']\s*,)\s*\\?\s*\(?\s*(?:[rfbu]{0,2}["''](?:[^"''\n]*?(?:\b20\d\d\b|@|\{)|[^"''\n]*["'']\s*(?:[%+]|\.format\())|(?!(?:True|False|None)\b)[A-Za-z_][\w.]*)'
flags: im
---
