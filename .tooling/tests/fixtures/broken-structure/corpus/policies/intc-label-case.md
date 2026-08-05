---
id: pol-INTC
tier: normative
category: delivery
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# Policy: Shortcut labels that are not the canonical id

## Purpose

Both references below resolve — reference and definition are matched case-insensitively — so nothing except
`label-canonical` would ever notice them. One mis-cases a mnemonic, [pol-vurm]; the other mis-cases an ADR,
[ADR-0004].

## Scope

This fixture only. Each mis-cased label is flagged twice, once where it is read and once where it is defined, because
fixing only one of the two leaves the reader still looking at an id that does not exist.

## Commitments

* We **will** trigger `label-canonical` four times and nothing else.

[ADR-0004]: /adrs/0004-missing-consequences.md
[pol-vurm]: vurm-bad-id-width.md
