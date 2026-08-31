---
id: std-RECON
tier: normative
status: active
implements:
  - eng:pol-DERV.CHECK
  - eng:pol-DERV.FAILED
  - eng:pol-DERV.RUNLOG
  - eng:pol-OBSV.ALERTS
applies-to:
  - svc-payment-ledger
review-by: "2027-08-31"
owner: paul.law
tags: [ ledger, reconciliation, settlement ]
---

# The ledger is reconciled against the PSP every day

`Standard: std-RECON` `ACTIVE`

## Summary

A daily run matches the PSP's settlement file against the ledger, entry by entry. Anything that does not match is a
break, and a break is owned by somebody until it closes.

## Rules

### The run happens every day

- A reconciliation **MUST** run once a day against the PSP's settlement file for the previous day
  (`eng:pol-DERV.CHECK`).
- The run **MUST** match a settlement row to a ledger entry on the PSP reference (`eng:pol-DERV.CHECK`).
- The run **MUST** compare the amount and the currency of a matched pair (`eng:pol-DERV.CHECK`).
- The run **MUST** record its inputs, its output and its result, and keep that record for seven years
  (`eng:pol-DERV.RUNLOG`).
- The run **MUST** raise an alert where the settlement file is missing (`eng:pol-DERV.FAILED`).
- The run **MUST NOT** record a clean run for a day whose settlement file did not arrive (`eng:pol-DERV.FAILED`).

### A break is named and owned

- The run **MUST** classify each break: in the file and not the ledger, in the ledger and not the file, or matched with
  a different amount (`eng:pol-DERV.CHECK`).
- The run **MUST** raise one alert naming the count and the total value of the breaks (`eng:pol-OBSV.ALERTS`).
- The run **MUST NOT** raise an alert for each break (`eng:pol-OBSV.ALERTS`).
- The run **MUST** escalate a break over 30 days old to the finance owner it names in its configuration
  (`eng:pol-OBSV.ALERTS`).

### Nothing downstream reads an unreconciled day

- A revenue report **MUST NOT** include a day whose reconciliation has not passed (`eng:pol-DERV.FAILED`).
- A team **MUST** close a break with a ledger correction under [std-LEDGER.nothing-amends-an-entry]
  (`eng:pol-DERV.FAILED`).
- A team **MUST NOT** close a break by editing the run's output (`eng:pol-DERV.FAILED`).

## Examples

```
Good
  2026-08-30  file 1,284 rows  ledger 1,284 entries  matched 1,282  breaks 2  £51.98
  break  ch_9Kx2  in file, not in ledger    £25.99
  break  ch_7Pm4  amounts differ 2599/2499  £25.98 vs £24.99

Avoid
  2026-08-30  reconciliation complete
```

The avoided line says a run happened. It does not say what it compared or whether anything matched, so nobody can tell
a clean day from an empty file.

## Conformance checklist

- [ ] The run has completed for every day in the last month.
- [ ] Each run's record names the file it read, the entries it compared and the breaks it found.
- [ ] A day with breaks does not appear in the revenue report.
- [ ] Every break older than 30 days has a named owner.
- [ ] A missing settlement file raises an alert within the run's own window.
- [ ] Each break closed last month closed through a ledger correction.

## Rationale and provenance

The PSP's file is what the money actually did. The ledger is what we believe it did, and a daily comparison is what
turns a slow drift between the two into a finding somebody sees the next morning.

- `eng:pol-DERV` commits us to checking a derived output before anything downstream uses it.
- `eng:pol-OBSV` commits us to keeping alerts few enough and meaningful enough that people act on them.

## Changelog

- 2026-08-31: initial version.

[std-LEDGER.nothing-amends-an-entry]: entries.md#nothing-amends-an-entry
