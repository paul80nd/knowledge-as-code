---
id: fix-0005
type: fix
tier: normative
status: draft
symptom-keywords: [cache, purge, stale]
review-by: "2020-06-01"
owner: human:alex.doe
---

# A fix nobody has re-read since 2020

`Fix: fix-0005` `DRAFT`

## Symptom

A purged cache still serves the old page.

## Environment

The edge cache, after a manual purge.

## Cause

Nothing, in itself. The record is here so `fix-in-date` has a document to report: `review-by` fell in
2020, and a fix written against a system that has moved is worse than no fix.

## Resolution

Purge by tag rather than by path.
