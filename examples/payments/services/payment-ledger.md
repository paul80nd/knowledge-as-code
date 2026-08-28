---
id: svc-payment-ledger
tier: descriptive
status: live
repo: payment-ledger
platform: dotnet-api
criticality: critical
depends-on:
data-stores:
owner: paul.law
tags: [ audit, ledger, reconciliation ]
---

# Payment Ledger

`Service: svc-payment-ledger` `LIVE`

Records what happened to every payment: the authorisation, the capture, the refund, and what the PSP said each time. It
is the account finance reconciles against.

## What it does

Writes one immutable entry per event on a payment, and answers queries about a payment's history. Nothing here amends an
entry: a correction is a further entry, so the reconciliation a month later reads the same sequence finance read on the
day.

It also runs the nightly reconciliation against the PSP's settlement file, and raises a discrepancy where the two
disagree. Its own README says the file arrives by SFTP at 02:00.

## Where it lives

* **Repository**: [`payment-ledger`](https://git.example.com/example-payments/payment-ledger)
* **Platform**: ASP.NET Core (.NET 10)
* **Deployed as**: App Service `app-payment-ledger-<env>`

## Environments

| Environment | URL                                     | Notes                              |
|-------------|-----------------------------------------|------------------------------------|
| Development | https://payment-ledger-dev.example.com  | Reconciles against the PSP sandbox |
| Test        | https://payment-ledger-test.example.com | Reconciles against the PSP sandbox |
| Production  | https://payment-ledger.example.com      | No public route. Platform network  |

## Dependencies

None in this catalogue. It reads the PSP's settlement file and writes to its own database.

## Data

Owns the ledger database, `sql-payment-ledger-<env>`, configured as `ConnectionStrings__Ledger`. An entry holds the
order, the amount, the PSP's reference and the token, and no card number reaches it. The `data` type is not adopted
here, so nothing in this corpus describes the schema. The repository's own migrations do.

## Operational notes

* **Criticality**: `critical`. [svc-payment-api] writes here before it answers, so a ledger that is down stops payment
  as surely as the PSP being down does.
* **NFRs**: [nfr-0002] holds how much of the ledger a recovery may lose.

[nfr-0002]: ../nfrs/0002-ledger-recovery-point.md
[svc-payment-api]: payment-api.md
