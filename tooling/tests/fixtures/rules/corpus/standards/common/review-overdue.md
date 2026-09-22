---
id: std-STALE
type: standard
tier: normative
status: active
implements:
  - pol-STAL.AGAIN
applies-to:
  - all
review-by: "2020-06-01"
owner: human:alex.doe
---

# A standard nobody has read since 2020

`Standard: std-STALE` `ACTIVE`

## Summary

Valid in every respect except its review date, which fell in 2020.

## Rules

### A record states the day it is read again

- A record here **MUST** state a `review-by` that has not passed.

_**Covers:** [pol-STAL].AGAIN_

## Examples

This file is the example. `review-by` reads `2020-06-01`, and `standard-in-date` reports it.

## Conformance checklist

- [ ] Is `review-by` still ahead of today?

## Rationale and provenance

It puts [pol-STAL].AGAIN into practice, and then fails its own rule.

[pol-STAL]: ../../policies/stal-review-overdue.md#clauses
