---
id: svc-payment-api
tier: descriptive
status: live
repo: payment-api
platform: dotnet-api
criticality: critical
depends-on:
  - svc-payment-ledger
data-stores:
owner: paul.law
tags: [ authorisation, cards, psp ]
---

# Payment API

`Service: svc-payment-api` `LIVE`

Takes an authorisation request from the checkout and asks the payment service provider (PSP) to move the money. It holds
a token standing for a card, and never the card itself.

## What it does

Accepts a request naming an order, an amount and a card token, calls the PSP to authorise it, and writes the outcome to
[svc-payment-ledger]. Capture and refund follow the same path.

**The token comes from the browser rather than from here.** The checkout page collects the card details in a frame the
PSP serves, and the browser posts them to the PSP directly. What reaches this service is the token the PSP returned.
Taken from the application settings, which carry the PSP's publishable key and no card storage of any kind.

## Where it lives

* **Repository**: [`payment-api`](https://git.example.com/example-payments/payment-api)
* **Platform**: ASP.NET Core (.NET 10)
* **Deployed as**: App Service `app-payment-api-<env>`

## Environments

| Environment | URL                                  | Notes                                      |
|-------------|--------------------------------------|--------------------------------------------|
| Development | https://payment-api-dev.example.com  | Points at the PSP sandbox                  |
| Test        | https://payment-api-test.example.com | Points at the PSP sandbox                  |
| Production  | https://payment-api.example.com      | The only environment reaching the live PSP |

## Dependencies

* **[svc-payment-ledger]**: every authorisation, capture and refund is written there before this service answers its
  caller. Configured as `Services__Ledger`.

The PSP is not in this catalogue. It is a third party, reached at `api.psp.example.com` and configured as `Psp__BaseUrl`.

## Data

None of its own, and that is the point. It holds a card token for the life of one request and stores nothing. What
happened to a payment is [svc-payment-ledger]'s.

## Operational notes

* **Criticality**: `critical`. A customer meets the failure at the moment they try to pay, and there is no second route
  to the PSP.
* **NFRs**: [nfr-0001] holds the authorisation latency a customer waits through.

[nfr-0001]: ../nfrs/0001-authorisation-latency.md
[svc-payment-ledger]: payment-ledger.md
