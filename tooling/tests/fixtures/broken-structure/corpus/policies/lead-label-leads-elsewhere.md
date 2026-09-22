---
id: pol-LEAD
type: policy
tier: normative
status: draft
review-by: "2027-08-05"
owner: human:alex.doe
---

# Labels leading somewhere other than the id they show

`Policy: pol-LEAD` `DRAFT`

## Purpose

Every definition below leads to a document whose id is `pol-CATS`, so the path is real and `link-resolves` passes. One
label is shaped like an id and names a policy nothing here carries, [pol-DEVI]. The second is longer than the mnemonic
width, so nothing reads it as an id at all, [pol-CATEGORY]. The third is read through a
[full reference][pol-WRITTEN], which displays its own words.

## Scope

This fixture only. A label a reader sees is flagged twice, once where it is read and once where it is defined, and
each finding names the id the document actually carries. The label behind a full reference reaches no reader, so it is
flagged at its definition alone.

## Clauses

| Id      | Clause                                                         | Alignment |
|---------|----------------------------------------------------------------|-----------|
| `LEADS` | **MUST** trigger `label-canonical` five times and nothing else |           |

[pol-CATEGORY]: cats-category-written.md
[pol-DEVI]: cats-category-written.md
[pol-WRITTEN]: cats-category-written.md
