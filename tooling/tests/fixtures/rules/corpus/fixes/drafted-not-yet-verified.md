---
id: fix-0003
type: fix
tier: normative
status: draft
symptom-keywords: [lock, migration, timeout]
review-by: "2026-12-31"
owner: human:alex.doe
---

# Drafted and not yet verified

`Fix: fix-0003` `DRAFT`

## Symptom

A migration times out waiting for a lock.

## Cause

Nothing, in itself. It is the other side of `fix-0002`. The status is `draft`, so `verified` is not required and
this record validates clean without it.

## Resolution

Reserve the lock before the migration runs.
