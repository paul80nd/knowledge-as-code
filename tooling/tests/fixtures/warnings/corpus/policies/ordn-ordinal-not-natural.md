---
id: pol-ORDN
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [A.8.29, A.8.7]
review-by: "2027-08-05"
owner: alex.doe
---

# A byte-wise sort is not alphabetical

`Policy: pol-ORDN` `DRAFT`

## Purpose

The `clauses:` list here is sorted the way a plain string comparison would have it, and warns. It is the mirror of
`pol-NATR`: between them they pin that `list-order` reads `A.8.7` as coming before `A.8.29`.

## Scope

This fixture only.

## Clauses

| Id       | Clause                                                                 | Alignment |
|----------|------------------------------------------------------------------------|-----------|
| `ORDER`  | **MUST** treat a byte-wise sort of numbered references as out of order | [ISO 27001:2022].A.8.29 |
| `ACCEPT` | **MUST NOT** accept it merely because it is consistent                 | [ISO 27001:2022].A.8.7  |

[ISO 27001:2022]: ../frameworks.md#iso-27001

