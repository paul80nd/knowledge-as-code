---
id: pol-RTNT
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.5.33
  - ISO27001:2022 A.5.34
review-by: "2030-01-01"
owner: mira.okonjo
tags: [ retention ]
---

# Data is kept for as long as somebody needs it

`Policy: pol-RTNT` `DRAFT`

## Purpose

We keep a record for as long as somebody needs it, and we delete it once nobody does. A store nobody prunes grows into
a liability at the same rate it grows in size.

## Scope

Every store we operate, whatever holds it.

## Clauses

| Id        | Clause                                                                   | Alignment             |
|-----------|--------------------------------------------------------------------------|-----------------------|
| `TIMEBOX` | **MUST** set a retention period for every store, and delete against it   | ISO 27001:2022.A.5.33 |
| `HOLD`    | **MUST** record a legal hold against whatever [gls-estate] calls a store | ISO 27001:2022.A.5.34 |
| `PRUNE`   | SHOULD prune on a schedule, rather than when a store runs out of space   |                       |

## Exceptions

A store under a legal hold outlives its retention period. We record the hold against the store, and the period resumes
on the day the hold lifts.

[gls-estate]: ../glossary/estate.md
