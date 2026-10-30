---
id: nfr-target-left-as-a-dash
type: nfr
tier: normative
status: draft
characteristic: availability
applies-to: [svc-unalerted-critical]
target: —
window: monthly
measured-by: The uptime probe on the status dashboard, read at the end of each month.
review-by: "2030-12-31"
owner: human:alex.doe
---

# A target left as a dash

`NFR: nfr-target-left-as-a-dash` `DRAFT`

## Target

None. `target` declares no `pattern:`, so only `bare-key` and `required-field` report the dash in its place.

## Why this number

There is no number. A dash is the spelling somebody reaches for when they have not decided one.

## How it is measured

The uptime probe reports one reading a month, and the dashboard keeps the series.

## Current actual

99.95% over the last month.

## What a breach costs

Nothing can be breached until somebody states a target.

## What we do about a breach

State the target first, then raise a ticket against the service when the probe falls below it.
