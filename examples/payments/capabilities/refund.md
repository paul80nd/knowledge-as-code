---
id: cap-refund
type: capability
tier: descriptive
status: live
implemented-by:
  - svc-payment-api
  - svc-payment-ledger
ado-epics: [ 2188 ]
feature-files:
  - payment-api/features/refund.feature
  - payment-ledger/features/refund-entry.feature
owner: human:paul.law
tags: [ refunds ]
---

# Get money back for an order

`Capability: cap-refund` `LIVE`

A customer who returns an order gets the money back on the card that paid for it.

## What it does

The money goes back to the card that paid, and reaches at most what [svc-payment-api] captured.

## Why it exists

The card schemes require a route to a refund. A customer who doubts they can get the money back does not pay.

## Where the detail lives

|                    |                                           |
|--------------------|-------------------------------------------|
| **Implemented by** | [svc-payment-api], [svc-payment-ledger]   |
| **Specified in**   | ADO epic #2188                            |
| **Tested by**      | `refund.feature`, `refund-entry.feature`  |

## Known limitations

A card that has expired since the payment cannot be refunded to it.

[svc-payment-api]: ../services/payment-api.md
[svc-payment-ledger]: ../services/payment-ledger.md
