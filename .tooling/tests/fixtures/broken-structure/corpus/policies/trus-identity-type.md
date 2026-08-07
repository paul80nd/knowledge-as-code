---
id: pol-TRUS
tier: normative
category: security
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An identity line naming the wrong type

`Standard: pol-TRUS` `DRAFT`

## Purpose

The right id and the right status, under a type name that belongs to a different part of the taxonomy. Copying a
template from the wrong folder is how this happens, and the id alone would not catch it.

## Scope

This fixture only. It exists so `identity-type` is exercised — the check that holds the line's type name to the
`label` the folder's schema declares.

## Commitments

* We **will** trigger `identity-type` and nothing else.
