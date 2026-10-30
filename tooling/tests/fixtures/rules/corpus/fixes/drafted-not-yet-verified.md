---
id: fix-drafted-not-yet-verified
type: fix
tier: normative
status: draft
symptom-keywords: [lock, migration, timeout]
review-by: "2030-12-31"
owner: human:alex.doe
---

# Drafted and not yet verified

`Fix: fix-drafted-not-yet-verified` `DRAFT`

## Symptom

A migration times out waiting for a lock.

## Environment

The migration runner, against the primary database.

## Cause

Nothing, in itself. It is the other side of `fix-verification-not-stated`. The status is `draft`, so `verified` is not
required and this record validates clean without it.

## Resolution

Reserve the lock before the migration runs.
