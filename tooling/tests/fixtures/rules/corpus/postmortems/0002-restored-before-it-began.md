---
id: pmt-0002
type: postmortem
tier: decided
status: draft
occurred-at: 2026-06-12T09:00:00Z
detected-at: 2026-06-12T09:05:00Z
restored-at: 2026-06-12T08:00:00Z
duration: PT40M
severity: sev2
affected: [svc-catalogue]
owner: human:alex.doe
tags: [expressions]
---

# Restored before it began

`Postmortem: pmt-0002` `DRAFT`

## Summary

Service comes back an hour before it goes, which is what `restored-not-before-occurred` refuses. The duration rule is
guarded on the same order, so this document reports one fault rather than two.

## Timeline

Nothing happened, and it finished before it started.

## Impact

None — this document exists to be judged, not to describe an incident.

## Root cause

`restored-at` is an hour before `occurred-at`.

## Contributing factors

Someone typed the moments from memory.

## What went well

Only the ordering rule fired, which is the half this fixture pins.

## Actions

Leave the moments wrong, so the golden keeps pinning the finding.
