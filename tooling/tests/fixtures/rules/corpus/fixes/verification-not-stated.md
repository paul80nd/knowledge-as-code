---
id: fix-0002
type: fix
tier: normative
status: active
symptom-keywords: [reindex, search, stale]
review-by: "2026-12-31"
owner: human:alex.doe
---

# Verification not stated

`Fix: fix-0002` `ACTIVE`

## Symptom

Search results stay stale after a reindex.

## Cause

Nothing, in itself. It exists so that `required-when: 'status != draft'` has a record that trips it. The status is
`active`, so a verification must be listed, and none is.

## Resolution

Add a `verified` line, or move the status to `draft`.
