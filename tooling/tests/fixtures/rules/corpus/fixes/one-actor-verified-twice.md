---
id: fix-0004
type: fix
tier: normative
status: active
symptom-keywords: [log, provenance, verification]
verified:
  - { at: 2026-06-12T09:00:00Z, by: human:alex.doe }
  - { at: 2026-09-04T11:30:00Z, by: human:alex.doe }
review-by: "2026-12-31"
owner: human:alex.doe
---

# One actor verified twice

`Fix: fix-0004` `ACTIVE`

## Symptom

Covering `one-verification-per-actor`. The same person states two verifications, which is the list used as a log of
every re-check rather than as the set of actors who have checked.

## Environment

Any. The finding is about the field, and nothing in the environment bears on it.

## Cause

`verified` says who has checked the record, and the trust tier reads the actor rather than the count. What git already
keeps is when each of them checked before.

## Resolution

Move the `at` on the entry the actor has. Two actors reading the same record are two entries, and the rule stays quiet
on those.
