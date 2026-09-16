---
id: cap-card-payment
type: capability
tier: descriptive
status: live
implemented-by:
  - svc-payment-api
  - svc-payment-ledger
ado-epics: [ 2101, 2140 ]
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

`Capability: cap-card-payment` `LIVE`

A customer pays at the checkout and is told there and then whether the card was accepted.

## What it does

The customer hears yes or no while the checkout page is still open. The money moves later, when the order ships, and
the customer sees one charge rather than two.

Authorisation and capture are separate steps for that reason. An order that never ships is never charged.

## Why it exists

Nothing else in the business takes money. Every other capability here corrects or explains a payment this one took.

## Surfaces

The checkout page, and the payment frame the payment service provider serves inside it.

## Where the detail lives

|                    |                                                       |
|--------------------|-------------------------------------------------------|
| **Implemented by** | [svc-payment-api], [svc-payment-ledger]               |
| **Specified in**   | ADO epics #2101, #2140                                |
| **Tested by**      | `authorise.feature`, `capture.feature`,               |
|                    | `payment-history.feature`                             |
| **Constrained by** | [nfr-0001]                                            |

## Known limitations

A card issuer may take several seconds to answer a step-up challenge, and [nfr-0001] does not cover that wait.

[nfr-0001]: ../nfrs/0001-authorisation-latency.md
[svc-payment-api]: ../services/payment-api.md
[svc-payment-ledger]: ../services/payment-ledger.md
