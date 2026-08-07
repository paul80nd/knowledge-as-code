---
id: pol-AGNT
tier: normative
category: governance
status: draft
review-by: "2027-08-05"
owner: alex.doe
---

# An identity line of the wrong shape

`pol-AGNT` `DRAFT`

## Purpose

A line beneath the title made of code spans, but not the two the identity line is: the type name is missing, so there
is no colon to split on and nothing to compare against the frontmatter.

## Scope

This fixture only. It exists so the malformed-line branch of `identity` is exercised as well as the absent-line branch,
which `obsv-no-identity-line.md` covers. A malformed line is reported once, against the line, rather than as a cascade
of complaints about parts that were never written.

## Commitments

* We **will** trigger `identity` and nothing else.
