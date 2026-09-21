---
id: nfr-0002
type: nfr
tier: normative
status: draft
characteristic: availability
applies-to: [svc-unalerted-critical]
target: 99.9%
window: monthly
alert-after: 15 minutes
measured-by: The availability panel on the platform dashboard, read monthly.
review-by: "2026-12-31"
owner: human:alex.doe
---

# A delay in front of an alert nobody raises

`NFR: nfr-0002` `DRAFT`

## Target

99.9% monthly, which is concrete and measured and is not what this document is about.

## Why this number

Nothing fixes it. The figure is here so the record has one, and what the fixture is about is `alert-after`.

## How it is measured

From the availability panel, which names an instrument and reports a number, so `target-is-measurable` is
satisfied and this record reports the one check it exists for.

## Current actual

99.94% over the last month.

## What a breach costs

Nobody would find out. The service this binds states `monitoring-output: none`, so the fifteen minutes above
sit in front of an alert that does not exist.

## What we do about a breach

Nothing automatic, which is the fault `alert-after-needs-an-alert` reports. Either the service raises an
alert, or the record drops the delay.
