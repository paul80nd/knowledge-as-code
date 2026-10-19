---
id: std-DEPLOY
type: standard
tier: normative
status: draft
implements:
  - pol-PIPE.DEPLOY
applies-to:
  - all
review-by: "2031-08-05"
owner: human:alex.doe
---

# An artefact says which change it came from

`Standard: std-DEPLOY` `DRAFT`

## Summary

A standard with one control checking it, so the review view lists that control beside each of its rules.

## Rules

### The artefact records its commit

- An artefact **MUST** record the commit it was built from.
- A service **SHOULD** report that commit on its health endpoint.

## Examples

A health endpoint returning `{"commit": "4f2a9c1"}`.

## Conformance checklist

- [ ] The artefact names its commit.

## Rationale and provenance

Filed here so a view has a rule with a control and a status of `draft`.
