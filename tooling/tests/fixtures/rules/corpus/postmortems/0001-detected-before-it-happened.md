---
id: pmt-0001
tier: decided
status: draft
occurred-on: "2026-06-12"
detected-on: "2026-06-11"
duration: 40 minutes
severity: sev2
affected: [svc-catalogue]
prompted:
owner: alex.doe
tags: [expressions]
---

# Detected before it occurred

`Postmortem: pmt-0001` `DRAFT`

## Summary

The two dates are the wrong way round, which is the whole point of this document: `detected-not-before-occurred` is
an expression rule, and this is the corpus it is evaluated against.

## Timeline

Nothing happened, in the wrong order.

## Impact

None — this document exists to be judged, not to describe an incident.

## Root cause

`detected-on` is a day before `occurred-on`.

## Contributing factors

Someone typed the dates from memory.

## What went well

The rule caught it, which is what the fixture is asserting.

## Actions

Leave the dates wrong, so the golden keeps pinning the finding.
