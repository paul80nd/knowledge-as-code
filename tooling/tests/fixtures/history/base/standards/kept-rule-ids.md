---
id: std-KEPT
type: standard
tier: normative
status: active
implements:
  - pol-KEPT.CLEAN
applies-to:
  - all
review-by: "2031-01-05"
owner: human:alex.doe
---

# Rule ids are kept

`Standard: std-KEPT` `ACTIVE`

## Summary

An active standard whose rule `FAST` is removed at the head.

## Rules

### CLEAN: A clean record reports nothing

- A record here **MUST** stay free of findings.

_**Covers:** [pol-KEPT].CLEAN_

### FAST: A check runs fast

- A check here **SHOULD** finish within a second.

## Examples

This file is the example.

## Conformance checklist

- [ ] The record produces no finding of its own.

## Rationale and provenance

It implements [pol-KEPT].

[pol-KEPT]: ../policies/kept-clause-ids.md
