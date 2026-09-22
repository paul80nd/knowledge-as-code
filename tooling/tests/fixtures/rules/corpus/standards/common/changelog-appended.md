---
id: std-CLOG
type: standard
tier: normative
status: active
implements:
  - pol-STAL.AGAIN
applies-to:
  - all
review-by: "2031-08-07"
owner: human:alex.doe
---

# A changelog somebody appended to

`Standard: std-CLOG` `ACTIVE`

## Summary

Covering `changelog-order`. Two entries are in place, and the third was added at the foot of the section
rather than at the top. That is how the order goes wrong in practice.

## Rules

### A changelog states the newest change first

- A changelog entry **MUST** sit above every entry dated before it.

_**Covers:** [pol-STAL].AGAIN_

## Examples

This file is the example. Its last entry is dated after the one above it, and `changelog-order` reports it.

## Conformance checklist

- [ ] Does the first entry under `## Changelog` carry the latest date?

## Rationale and provenance

It puts [pol-STAL].AGAIN into practice, and then fails a rule of its own.

## Changelog

- 2026-09-02: said what a reader does with an entry.
- 2026-08-20: initial version.
- 2026-09-14: added the day nobody looks at the file again. Appended here, and reported for it.

[pol-STAL]: ../../policies/stal-review-overdue.md#clauses
