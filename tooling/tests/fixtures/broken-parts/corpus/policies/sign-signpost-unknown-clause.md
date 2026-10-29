---
id: pol-SIGN
type: policy
tier: normative
status: draft
review-by: "2031-08-05"
owner: human:alex.doe
---

# A signpost to a clause that does not exist

`Policy: pol-SIGN` `DRAFT`

## Purpose

A clause closes with a signpost to the other side of a shared obligation. This one sends the reader to a policy that
exists and has no clause of that id.

## Scope

This fixture only. It exercises `part-ref` on a signpost inside a clause.

## Clauses

| Id     | Clause                                                                              | Alignment |
|--------|-------------------------------------------------------------------------------------|-----------|
| `SIGN` | **MUST** trigger `part-ref` and nothing else ([pol-CREF].GONE states the same duty) |           |

[pol-CREF]: cref-unknown-clause.md
