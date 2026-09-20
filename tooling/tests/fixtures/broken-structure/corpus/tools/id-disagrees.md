---
id: tol-names-another-tool
type: tool
tier: descriptive
status: approved
homepage: https://example.invalid/tool
decided-on: "2026-01-05"
review-by: "2030-01-01"
owner: human:alex.doe
---

# Slug id disagreeing with the filename

`Tool: tol-names-another-tool` `APPROVED`

## What we use it for

Covering `id-matches-filename` on a slug id. A well-formed slug that names a different tool than the file it sits in.

## Status

Intentionally broken. The whole slug is compared, not a leading segment of it, so the disagreement is reported once
against the id.
