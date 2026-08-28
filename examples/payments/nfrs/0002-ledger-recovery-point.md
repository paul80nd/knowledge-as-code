---
id: nfr-0002
tier: normative
status: agreed
applies-to:
  - svc-payment-ledger
target: RPO 5 minutes, RTO 1 hour
measured-by: A quarterly restore of the ledger database into an isolated environment
constrained-by:
review-by: "2027-08-28"
owner: paul.law
tags: [ ledger, recovery, resilience ]
---

# A recovered ledger loses at most five minutes of payments

`NFR: nfr-0002` `AGREED`

The ledger is recoverable to a point five minutes before a failure, and back in service within an hour.

## Target

RPO of 5 minutes and RTO of 1 hour for the ledger database behind [svc-payment-ledger].

Five minutes is what the reconciliation can repair. The PSP's settlement file carries every authorisation it took, so a
gap shorter than one file can be rebuilt from it. A longer gap needs the PSP's support desk, which is a conversation
rather than a procedure.

## How it is measured

A restore into an isolated environment, quarterly, timed from the decision to restore to the ledger answering queries.
The recovery point is read off the restored data: the last entry present against the last entry the source held.

## Current actual

The last restore took 38 minutes and recovered to within 2 minutes. Point-in-time backup is on with a 35-day retention,
taken from the database configuration.

## If it is breached

Payments taken inside the lost window are absent from the account finance reconciles, so the money moved and this
corpus's record of it did not. The repair is the PSP's settlement file, and until that runs the ledger understates what
was taken.

[svc-payment-ledger]: ../services/payment-ledger.md
