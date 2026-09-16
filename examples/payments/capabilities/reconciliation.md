---
id: cap-reconciliation
type: capability
tier: descriptive
status: live
implemented-by:
  - svc-payment-ledger
ado-epics: [ 2077 ]
feature-files:
  - payment-ledger/features/break-report.feature
  - payment-ledger/features/settlement-import.feature
nfrs:
  - nfr-0002
owner: human:paul.law
tags: [ finance, reconciliation ]
---

# Account for the day's takings

`Capability: cap-reconciliation` `LIVE`

Finance can state what the business took on a given day, and show that the bank received the same.

## What it does

The customer here is the finance team. A difference between the bank and [svc-payment-ledger] is itemised, and each
item is owned by a named person until it closes.

## Why it exists

The business states its takings to its auditor. An unexplained difference is a finding at audit.

## Where the detail lives

|                    |                                                     |
|--------------------|-----------------------------------------------------|
| **Implemented by** | [svc-payment-ledger]                                |
| **Specified in**   | ADO epic #2077                                      |
| **Tested by**      | `break-report.feature`, `settlement-import.feature` |
| **Constrained by** | [nfr-0002]                                          |

## Known limitations

A gap longer than one settlement file cannot be rebuilt from it. [nfr-0002] says what that costs.

[nfr-0002]: ../nfrs/0002-ledger-recovery-point.md
[svc-payment-ledger]: ../services/payment-ledger.md
