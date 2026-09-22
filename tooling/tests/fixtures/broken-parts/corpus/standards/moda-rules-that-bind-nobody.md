---
id: std-MODA
type: standard
tier: normative
status: draft
implements: [ pol-BOLD.CLEAN ]
applies-to: [ all ]
review-by: "2031-08-05"
owner: human:alex.doe
---

# Rules that bind nobody

`Standard: std-MODA` `DRAFT`

## Summary

A standard whose rules say nothing about how hard they bind.

## Rules

### A bullet naming no modal

- A service reads its secrets from the vault.

_**Covers:** [pol-BOLD].CLEAN_

### A keyword left in plain capitals

- A service MUST read its secrets from the vault.

### A heading gathering prose

Secrets come from the vault, and nothing says whether that is a rule.

## Examples

A conforming bullet reads: a service **MUST** read its secrets from the vault.

## Conformance checklist

* Does every bullet under a rule heading carry a modal in bold capitals?

## Rationale and provenance

This fixture only. It exists so `part-modal` is exercised on each of its three branches: a bullet naming
no modal, a keyword left in plain capitals, and a heading gathering no bullet at all.

[pol-BOLD]: ../policies/bold-binding-not-bold.md#clauses
