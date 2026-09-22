---
id: nfr-0003
type: nfr
tier: normative
status: agreed
characteristic: availability
applies-to: [svc-unalerted-critical]
target: 99.9%
window: monthly
measured-by: The uptime probe on the status dashboard, read at the end of each month.
agreed-on: "2019-01-01"
review-by: "2020-06-01"
owner: human:alex.doe
---

# A target agreed in 2019 and never revisited

`NFR: nfr-0003` `AGREED`

## Target

99.9% monthly.

## Why this number

It was agreed once. `review-by` fell in 2020, so nobody has confirmed the figure since.

## How it is measured

The uptime probe reports one reading a month, and the dashboard keeps the series.

## Current actual

99.95% over the last month.

## What a breach costs

The service goes on failing and nobody is told the target was missed.

## What we do about a breach

Raise a ticket against the service, and read the probe series back to the day it turned.
