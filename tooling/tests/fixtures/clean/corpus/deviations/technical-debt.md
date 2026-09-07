---
id: dev-technical-debt
tier: normative
status: active
departs-from:
  - none
accepted-on: "2026-05-01"
review-by: "2026-08-01"
owner: alex.doe
tags: [ baseline ]
---

# The importer retries nothing

`Deviation: dev-technical-debt` `ACTIVE`

## What we are doing instead

The importer makes each call once. A call that fails leaves the row unimported until the next scheduled run.

## Why we need it

Retries were dropped to meet the date the branch libraries were told to expect the catalogue on.

## What compensates

The scheduled run repeats every hour, and the unimported count is on the same dashboard as the run itself.

## How it closes

The importer takes the retry policy the other consumers use.
