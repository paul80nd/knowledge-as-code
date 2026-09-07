---
id: faq-0002
type: faq
tier: normative
status: active
symptom-keywords: [confirmed, prefix, role]
confirmed:
  - { at: 2026-06-12T09:00:00Z, by: role:head-of-engineering }
review-by: "2026-12-31"
owner: role:head-of-engineering
---

# A role in the key that records who confirmed the answer

`FAQ: faq-0002` `ACTIVE`

## Symptom

`confirmed` declares `pattern: '^human:[a-z0-9.-]+$'` on its `by` key, so it refuses the `role:` its own `owner`
accepts. One document carrying both values shows where the two patterns differ.

## Cause

A post cannot read an answer. The person who read it is who the key records, and that person stays named after the
post changes hands.

## Fix

Name the person. `owner` above is untouched and fires nothing, which is what makes the finding readable as a
statement about `confirmed.by` alone. The message names the key inside the entry, so an author reads which half of
the pair is wrong.
