---
id: pol-INTC
type: policy
tier: normative
status: draft
review-by: "2031-08-05"
owner: human:alex.doe
---

# Shortcut labels that are not the canonical id

`Policy: pol-INTC` `DRAFT`

## Purpose

Reference and definition are matched case-insensitively, so every reference below resolves and nothing except
`label-canonical` would ever notice them. One mis-cases a mnemonic, [pol-vurm]; the second mis-cases an ADR,
[ADR-0004]; the third mis-cases only where it is read, [POL-cats], and is defined canonically.

## Scope

This fixture only. A label mis-cased in both places is flagged twice, once where it is read and once where it is
defined, because fixing only one of the two leaves the reader still looking at an id that does not exist. A label
mis-cased only where it is read is flagged there alone.

## Clauses

| Id      | Clause                                                         | Alignment |
|---------|----------------------------------------------------------------|-----------|
| `CLEAN` | **MUST** trigger `label-canonical` five times and nothing else |           |

[ADR-0004]: /adrs/0004-missing-consequences.md
[pol-CATS]: cats-category-written.md
[pol-vurm]: vurm-bad-id-width.md
