---
id: dev-clean-baseline
type: deviation
tier: normative
status: active
departs-from:
  - pol-VURM.CLEAN
accepted-on: "2030-01-01"
review-by: "2030-11-01"
owner: alex.doe
tags: [ baseline ]
---

# The reporting service stays on the old base image

`Deviation: dev-clean-baseline` `ACTIVE`

## What we are doing instead

The reporting service builds on the base image it shipped with, which carries two vulnerabilities rated medium.

## Why we need it

The newer image drops the font package the report renderer loads, and replacing the renderer is a quarter of work.

## What compensates

Neither vulnerability is reachable from the network, and the scanner report is attached to each release.

## How it closes

The renderer moves to the fonts the newer image carries, and the service takes that image.
