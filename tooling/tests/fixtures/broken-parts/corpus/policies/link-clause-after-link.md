---
id: pol-LINK
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# A citation that addresses its clause beside the link

`Policy: pol-LINK` `DRAFT`

## Purpose

A link resolves against a page, so a part id written beside it reaches nothing that would judge it. This
document cites [pol-CREF].MISSING, and a reader following the link lands on a policy that exists and
carries no such clause.

## Scope

This fixture only. It exercises `part-ref` where the citation is a link and a part id beside it.

## Clauses

| Id      | Clause                                       | Alignment |
|---------|----------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `part-ref` and nothing else |           |

[pol-CREF]: cref-unknown-clause.md
