---
id: nfr-0002
type: nfr
tier: normative
status: agreed
characteristic: recovery
applies-to:
  - svc-payment-ledger
target: RPO 5 minutes, RTO 1 hour
measured-by: A quarterly restore of the ledger database into an isolated environment
agreed-on: "2026-08-28"
review-by: "2027-08-28"
owner: human:paul.law
tags: [ ledger, recovery, resilience ]
---

# A recovered ledger loses at most five minutes of payments

`NFR: nfr-0002` `AGREED`

The ledger is recoverable to a point five minutes before a failure. It is back in service within an hour.

## Target

RPO of 5 minutes and RTO of 1 hour for the ledger database behind [svc-payment-ledger]. Both figures are read against
each failure.

## Why this number

Five minutes is what the reconciliation can repair. The PSP's settlement file lists every authorisation it took, so a
gap shorter than one file can be rebuilt from it. A longer gap needs the PSP's support desk, and no procedure covers
that.

The hour is a figure the quarterly restore has already met. `Current actual` records the last one at 38 minutes, so
the target commits to what the procedure does today and not to an improvement nobody has planned.

## How it is measured

A quarterly restore into an isolated environment measures both figures. The recovery time runs from the decision to
restore to the ledger serving queries. The recovery point compares the last entry in the restored data with the last
entry the source held.

## Current actual

The last restore took 38 minutes and recovered to within 2 minutes. The database configuration shows point-in-time
backup on, with a 35-day retention.

## If it is breached

Payments taken inside the lost window are absent from the account finance reconciles. The money moved and the ledger
has no entry for it. The repair is the PSP's settlement file, and until that runs the ledger understates what was
taken.

## Related

* [pmt-0001] records the incident this target was written after.

[pmt-0001]: ../postmortems/0001-refused-authorised-payments.md
[svc-payment-ledger]: ../services/payment-ledger.md
