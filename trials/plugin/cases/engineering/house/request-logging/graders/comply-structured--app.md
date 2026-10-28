---
type: regex
target: { source: file, path: catalogue/app.py }
pattern: '(json\.dumps\(|structlog\.\w|JSONRenderer|J[Ss][Oo][Nn]\w*Formatter|jsonlogger|[Ll]ogfmt\w*\(|\b(method|path|status)=(%|\{))'
---
