---
id: pmt-0003
type: postmortem
tier: decided
status: draft
occurred-at: 2026-06-12T09:00:00Z
detected-at: 2026-06-12T09:05:00Z
restored-at: 2026-06-12T09:40:00Z
duration: PT30M
severity: sev2
affected: [svc-catalogue]
owner: human:alex.doe
tags: [expressions]
---

# A duration the moments refuse

`Postmortem: pmt-0003` `DRAFT`

## Summary

The moments give forty minutes and the frontmatter states thirty, which is what `duration-matches-the-moments`
refuses. The rule is written in C# so that its message carries the span the moments give.

## Timeline

Nothing happened, for forty minutes, recorded as thirty.

## Impact

None. This document exists to be judged, not to describe an incident.

## Resolution

Nothing resolved it. This document exists to be judged.

## Root cause

`duration` is ten minutes short of the span between the two moments.

## Contributing factors

Someone subtracted by hand.

## What went well

The message names the value to write, so the fix is a paste.

## What went wrong

The fixture was written to fail, which is what went wrong.

## Where we got lucky

Nothing depends on this document, so nobody was misled.

## Actions

Leave the duration wrong, so the golden keeps pinning the finding.
