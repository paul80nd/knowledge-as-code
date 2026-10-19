---
id: std-OBS
type: standard
tier: normative
status: active
implements:
  - pol-OBSV.TRACE
applies-to:
  - all
review-by: "2031-08-05"
owner: human:alex.doe
---

# One request reads as one timeline

`Standard: std-OBS` `ACTIVE`

## Summary

A standard no control checks, so the review view says so.

## Rules

### A call forwards the trace

- A service **MUST** pass the inbound `traceparent` to everything it calls.

## Examples

An outbound HTTP call carrying the `traceparent` header it received.

## Conformance checklist

- [ ] Every outbound call forwards the trace.

## Rationale and provenance

Filed here so a view has an `active` rule with no control.
