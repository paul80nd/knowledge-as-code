---
id: rpt-verified-by-a-post
type: report
tier: descriptive
status: draft
owner: role:head-of-engineering
generated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }
sources:
  - { resource: fixture-corpus, version: "0.2.0" }
verified:
  - { at: 2026-09-08T11:00:00Z, by: role:head-of-engineering }
---

# A post in the key that records who read the verdicts

`Report: rpt-verified-by-a-post` `DRAFT`

`verified` takes a person or an agent, and `verified-by-a-known-actor` refuses the `role:` that this record's own
`owner` accepts. One document carrying both values shows where the two patterns differ.

A post cannot read an answer. The person who did read it is who the key records, and that person stays named after
the post changes hands. The exported `trust` tier reads the prefix, so an unrecognised one weighs the answer wrongly
rather than only reading oddly.
