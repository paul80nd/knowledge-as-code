---
id: pol-BKUP
tier: normative
status: active
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.8.13 ]
review-by: "2030-06-01"
owner: mira.okonjo
tags: [ recovery ]
---

# Every store we run can be brought back

`Policy: pol-BKUP` `ACTIVE`

## Purpose

A store we cannot restore is a store we have already lost. We hold a copy of each one away from what it serves, and we
prove the copy reads back before the day we need it to.

A copy nobody has read is a guess. The clauses below turn the guess into something we have seen work, on a cycle
short enough that the last proof is still worth trusting.

## Scope

Every store we operate, in every environment.

## Clauses

| Id        | Clause                                                                      | Alignment               |
|-----------|-----------------------------------------------------------------------------|-------------------------|
| `COPY`    | **MUST** hold a copy of every store away from what the store itself runs on | [ISO 27001:2022].A.8.13 |
| `RESTORE` | **MUST** prove a restore of what `pol-BKUP.COPY` covers, once a quarter     |                         |
| `SHARED`  | **MUST NOT** keep the only copy of a store on the host that serves it       | [ISO 27001:2022].A.8.13 |
| `DRILL`   | SHOULD rehearse a whole-service recovery once a year                        |                         |
| `MEASURE` | COULD publish how long each restore took, so a target can be set against it |                         |

## Exceptions

A store we can rebuild from another store carries no copy of its own. We record what rebuilds it, and the clauses above
bind whatever we rebuild it from.

[ISO 27001:2022]: ../frameworks.md#iso-27001
