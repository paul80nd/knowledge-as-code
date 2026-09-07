---
id: faq-0002
type: faq
tier: normative
status: active
symptom-keywords: [confirmed-by, prefix, role]
confirmed-by: role:head-of-engineering
confirmed-on: "2026-06-12"
review-by: "2026-12-31"
owner: role:head-of-engineering
---

# A role in the field that records who confirmed the answer

`FAQ: faq-0002` `ACTIVE`

## Symptom

`confirmed-by` declares `pattern: '^human:[a-z0-9.-]+$'`, so it refuses the `role:` its own `owner` accepts. One
document carrying both values shows where the two patterns differ.

## Cause

A post cannot read an answer. The person who read it is who the field records, and that person stays named after the
post changes hands.

## Fix

Name the person. `owner` above is untouched and fires nothing, which is what makes the finding readable as a
statement about `confirmed-by` alone.
