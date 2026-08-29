---
id: pol-ENTR
tier: normative
status: active
aligns-with:
  - ISO 27001:2022
  - frameworks: ISO 27001:2022
review-by: "2026-12-31"
owner: alex.doe
---

# An object list written the wrong way

`Policy: pol-ENTR` `ACTIVE`

## Purpose

To write `aligns-with` as the flat list of strings it used to be, and then to misname the key that
carries the framework. Neither entry says which framework it means in a way the schema can read, so
neither reaches the reconciliation that would otherwise report them again.

## Scope

Every service in the estate.

## Clauses

| Id      | Clause                                                     | Alignment |
|---------|------------------------------------------------------------|-----------|
| `SHAPE` | **MUST** write each entry as the mapping the schema names. |           |
