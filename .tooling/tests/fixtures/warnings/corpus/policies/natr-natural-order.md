---
id: pol-NATR
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.5.7
  - ISO27001:2022 A.8.7
  - ISO27001:2022 A.8.8
  - ISO27001:2022 A.8.29
review-by: "2027-08-05"
owner: alex.doe
---

# Numbers in a list compare as numbers

`Policy: pol-NATR` `DRAFT`

## Purpose

`aligns-with` here is in the order a reader would put it, and the golden pins that it draws no finding. A plain
string comparison would demand `A.8.29` before `A.8.7`, which is why `list-order` compares digit runs numerically.

## Scope

This fixture only.

## Commitments

* We **will** keep ISO references in the order their numbers imply.
* We **will not** re-sort them to suit a byte-wise comparison.
