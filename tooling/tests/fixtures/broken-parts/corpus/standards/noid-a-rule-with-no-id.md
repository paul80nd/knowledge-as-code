---
id: std-NOID
type: standard
tier: normative
status: draft
implements: [ pol-BOLD.CLEAN ]
applies-to: [ all ]
review-by: "2031-08-05"
owner: human:alex.doe
---

# A rule with no id

`Standard: std-NOID` `DRAFT`

## Summary

A standard whose rule heading opens with no id, so nothing can cite it.

## Rules

### Every secret is read at run time

- A service **MUST** read its secrets from the vault.

_**Covers:** [pol-BOLD].CLEAN_

## Examples

A conforming heading opens with its id: `### RUNTIME: Every secret is read at run time`.

## Conformance checklist

* Does every rule heading open with its id?

## Rationale and provenance

This fixture only. It exists so `part-id-format` is exercised by a rule heading with no id.

[pol-BOLD]: ../policies/bold-binding-not-bold.md#clauses
