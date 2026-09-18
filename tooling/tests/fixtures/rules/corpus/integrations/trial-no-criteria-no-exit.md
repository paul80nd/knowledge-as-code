---
id: int-trial-no-criteria-no-exit
type: integration
tier: descriptive
status: trial
vendor: Example Tax
used-by: [svc-catalogue]
criticality: critical
owner: human:alex.doe
---

# A critical trial that says nothing about leaving

`Integration: int-trial-no-criteria-no-exit` `TRIAL`

## What it does

Works out the tax on a fee. [svc-catalogue] calls it on every priced request.

## Contract

REST over HTTPS. The key is held in the platform secret store.

## Failure modes

The service can time out, and the fallback is the rate table held here.

This document carries two faults. `trial-criteria-required` reports the missing `## Trial criteria`, so nothing
says what would end the trial. `exit-required` reports the missing `## Exit`, so a critical vendor has no stated
way out.

[svc-catalogue]: ../services/catalogue.md
