---
id: fix-0002
type: fix
tier: normative
status: active
symptom-keywords: [confirmed, prefix, role]
confirmed:
  - { at: 2026-06-12T09:00:00Z, by: role:head-of-engineering }
review-by: "2026-12-31"
owner: role:head-of-engineering
---

# A role in the key that records who confirmed the answer

`Fix: fix-0002` `ACTIVE`

## Symptom

`confirmed` takes the shared `event` shape, whose `by` is a plain string. The fix type holds it to a person with the
`confirmed-by-a-person` rule, so it refuses the `role:` its own `owner` accepts. One document carrying both values
shows where the two patterns differ.

## Cause

A post cannot read an answer. The person who read it is who the key records, and that person stays named after the
post changes hands.

## Resolution

Name the person. `owner` above is untouched and fires nothing, which is what makes the finding readable as a
statement about `confirmed.by` alone. The rule asks one question of the whole document, so the message names the key
rather than the entry, and a list of several confirmations reports once.
