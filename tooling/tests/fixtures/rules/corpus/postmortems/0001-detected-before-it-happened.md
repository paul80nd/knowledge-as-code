---
id: pmt-0001
type: postmortem
tier: decided
status: draft
occurred-at: 2026-06-12T09:00:00Z
detected-at: 2026-06-11T09:00:00Z
duration: PT40M
severity: sev2
affected: [svc-catalogue]
owner: human:alex.doe
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

None. This document exists to be judged, not to describe an incident.

## Resolution

Nothing resolved it. This document exists to be judged.

## Root cause

`detected-at` is a day before `occurred-at`.

## Contributing factors

Someone typed the moments from memory.

## What went well

The rule caught it, which is what the fixture is asserting.

## What went wrong

The fixture was written to fail, which is what went wrong.

## Where we got lucky

Nothing depends on this document, so nobody was misled.

## Actions

Leave the dates wrong, so the golden keeps pinning the finding.
