---
id: ofr-refund
type: offering
tier: descriptive
status: live
implemented-by:
  - svc-payment-api
  - svc-payment-ledger
feature-files:
  - payment-api/features/refund.feature
  - payment-ledger/features/refund-entry.feature
nfrs:
  - nfr-0003
owner: human:paul.law
tags: [ refunds ]
---

# Get money back for an order

`Offering: ofr-refund` `LIVE`

A customer who returns an order gets the money back on the card that paid for it.

## What it does

The money goes back to the card that paid, and reaches at most what [svc-payment-api] captured.

## Who it is for

Customers who have returned an order. Every one of them paid through [ofr-card-payment], so this group is
part of that one.

## Why it exists

The card schemes require a route to a refund. A customer who doubts they can get the money back does not pay.

## Where the detail lives

* **Implemented by**: [svc-payment-api], [svc-payment-ledger]
* **Specified in**: [gh#2188]
* **Constrained by**: [nfr-0003]

## Known limitations

A card that has expired since the payment cannot be refunded to it.

[gh#2188]: https://git.example.com/example-payments/payment-api/issues/2188
[nfr-0003]: ../nfrs/0003-refund-settlement.md
[ofr-card-payment]: card-payment.md
[svc-payment-api]: ../services/payment-api.md
[svc-payment-ledger]: ../services/payment-ledger.md
