---
id: std-NEST
type: standard
tier: normative
status: draft
implements: [ pol-BOLD.CLEAN ]
applies-to: [ all ]
review-by: "2031-08-05"
owner: human:alex.doe
---

# A rule holding a list

`Standard: std-NEST` `DRAFT`

## Summary

A standard whose rule hides two of its points inside a bullet.

## Rules

### RUNTIME: Every secret is read at run time

- A service **MUST** read its secrets from the vault.
  - The vault is the one the platform team runs.
  - The token is short-lived.

_**Covers:** [pol-BOLD].CLEAN_

## Examples

A conforming rule writes each point as a bullet of its own: a token **MUST** be short-lived.

## Conformance checklist

* Is every point under a rule heading a bullet of its own?

## Rationale and provenance

This fixture only. It exists so `part-nested` is exercised by a list written inside a bullet.

[pol-BOLD]: ../policies/bold-binding-not-bold.md#clauses
