---
id: svc-payment-api
type: service
tier: descriptive
status: live
repo: payment-api
platform: dotnet-api
criticality: critical
depends-on:
  - svc-payment-ledger
owner: human:paul.law
tags: [ authorisation, cards, psp ]
---

# Payment API

`Service: svc-payment-api` `LIVE`

Takes an authorisation request from the checkout and asks the payment service provider (PSP) to move the money. It
keeps a card token. The card number never arrives here.

## What it does

Accepts a request with an order, an amount and a card token. Calls the PSP to authorise it. Writes the outcome to
[svc-payment-ledger]. Capture and refund follow the same path.

**The browser sends the card details to the PSP.** The checkout page collects them in a frame the PSP serves, and the
browser posts them to the PSP directly. This service receives the token the PSP returned. The application settings
show the PSP's publishable key and no card storage of any kind.

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

* **[svc-payment-ledger]**: every authorisation, capture and refund is written there before this service replies to its
  caller. Configured as `Services__Ledger`.

The PSP is not in this catalogue. It is a third party, called at `api.psp.example.com` and configured as `Psp__BaseUrl`.

## Data

None of its own. It keeps a card token for one request and stores nothing. [svc-payment-ledger] records what happened
to a payment.

## Operational notes

* **Criticality**: `critical`. A customer sees the failure at the moment they try to pay, and there is no second route
  to the PSP.
* **NFRs**: [nfr-0001] states how long an authorisation may take.

[nfr-0001]: ../nfrs/0001-authorisation-latency.md
[svc-payment-ledger]: payment-ledger.md
