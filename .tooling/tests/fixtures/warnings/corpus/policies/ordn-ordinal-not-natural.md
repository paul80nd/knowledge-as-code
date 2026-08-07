---
id: pol-ORDN
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.8.29
  - ISO27001:2022 A.8.7
review-by: "2027-08-05"
owner: alex.doe
---

# A byte-wise sort is not alphabetical

`Policy: pol-ORDN` `DRAFT`

## Purpose

`aligns-with` here is sorted the way a plain string comparison would have it, and warns. It is the mirror of
`pol-NATR`: between them they pin that `list-order` reads `A.8.7` as coming before `A.8.29`.

## Scope

This fixture only.

## Clauses

| Id       | Clause                                                                 |
|----------|------------------------------------------------------------------------|
| `ORDER`  | **MUST** treat a byte-wise sort of numbered references as out of order |
| `ACCEPT` | **MUST NOT** accept it merely because it is consistent                 |

