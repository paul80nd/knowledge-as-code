---
id: ofr-card-payment
type: offering
tier: descriptive
status: live
implemented-by:
  - svc-payment-api
  - svc-payment-ledger
feature-files:
  - payment-api/features/authorise.feature
  - payment-api/features/capture.feature
  - payment-ledger/features/payment-history.feature
nfrs:
  - nfr-0001
owner: human:paul.law
tags: [ checkout ]
---

# Pay for an order with a card

`Offering: ofr-card-payment` `LIVE`

A customer pays at the checkout and is told there and then whether the card was accepted.

## What it does

The customer hears yes or no while the checkout page is still open. The money moves later, when the order ships, and
the customer sees one charge rather than two.

Authorisation and capture are separate steps for that reason. An order that never ships is never charged.

## Who it is for

Anyone who reaches the checkout. Paying needs no account, so the group is every visitor with an order to
settle.

## Why it exists

Nothing else in the business takes money. Every other offering here corrects or explains a payment this one took.

## Surfaces

The checkout page, and the payment frame the payment service provider serves inside it.

## Where the detail lives

* **Implemented by**: [svc-payment-api], [svc-payment-ledger]
* **Specified in**: [gh#2101], [gh#2140]
* **Constrained by**: [nfr-0001]

## Known limitations

A card issuer may take several seconds to answer a step-up challenge, and [nfr-0001] does not cover that wait.

[gh#2101]: https://git.example.com/example-payments/payment-api/issues/2101
[gh#2140]: https://git.example.com/example-payments/payment-api/issues/2140
[nfr-0001]: ../nfrs/0001-authorisation-latency.md
[svc-payment-api]: ../services/payment-api.md
[svc-payment-ledger]: ../services/payment-ledger.md
