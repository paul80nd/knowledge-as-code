---
id: svc-payment-ledger
type: service
tier: descriptive
status: live
repo: payment-ledger
platform: dotnet-api
criticality: critical
owner: human:paul.law
tags: [ audit, ledger, reconciliation ]
---

# Payment Ledger

`Service: svc-payment-ledger` `LIVE`

Records what happened to every payment: the authorisation, the capture, the refund, and what the PSP said each time. It
is the account finance reconciles against.

## What it does

Writes one immutable entry per event on a payment. Returns a payment's history to a caller that asks for it. An entry
is never amended. A correction is a further entry, so the reconciliation a month later reads the same sequence finance
read on the day.

It also runs the nightly reconciliation against the PSP's settlement file, and reports a break where the file and
the ledger disagree. The repository's README says the file arrives by SFTP at 02:00.

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

Owns the ledger database, `sql-payment-ledger-<env>`, configured as `ConnectionStrings__Ledger`. An entry contains the
order, the amount, the PSP's reference and the token. No card number is written to it. The `data` type is not adopted
here, so nothing in this corpus describes the schema. The repository's own migrations describe it.

## Operational notes

* **Criticality**: `critical`. [svc-payment-api] writes here before it replies, so a ledger that is down stops payment.
  A PSP that is down has the same effect.
* **NFRs**: [nfr-0002] states how much of the ledger a recovery may lose.

[nfr-0002]: ../nfrs/0002-ledger-recovery-point.md
[svc-payment-api]: payment-api.md
