---
id: pol-LEAD
type: policy
tier: normative
status: draft
review-by: "2027-08-05"
owner: human:alex.doe
---

# Shortcut labels leading somewhere other than the id they show

`Policy: pol-LEAD` `DRAFT`

## Purpose

Both definitions below lead to a document whose id is `pol-CATS`, so the path is real and `link-resolves` passes. One
label is shaped like an id and names a policy nothing here carries, [pol-DEVI]. The other is longer than the mnemonic
width, so nothing reads it as an id at all, [pol-CATEGORY].

## Scope

This fixture only. Each label is flagged twice, once where it is read and once where it is defined, and each finding
names the id the document actually carries.

## Clauses

| Id      | Clause                                                         | Alignment |
|---------|----------------------------------------------------------------|-----------|
| `LEADS` | **MUST** trigger `label-canonical` four times and nothing else |           |

[pol-CATEGORY]: cats-category-written.md
[pol-DEVI]: cats-category-written.md
