---
type: regex
target: { source: file, path: stock/app.py }
pattern: '^[^#\n]*?(?:["'']Sunset["'']\s*(?:\]\s*=|:)|(?:\.(?:set|add|add_header|setdefault)\(|[\[(,=]\s*\()\s*["'']Sunset["'']\s*,)\s*\\?\s*\(?\s*(?:[rfbu]{0,2}["''](?:[^"''\n]*?(?:\b20\d\d\b|\{)|[^"''\n]*["'']\s*(?:[%+]|\.format\())|(?!(?:True|False|None)\b)[A-Za-z_][\w.]*)'
flags: im
---
