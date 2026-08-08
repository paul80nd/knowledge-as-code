---
id: svc-catalogue-api
tier: descriptive
status: live
repo: platform
platform: dotnet-api
criticality: critical
depends-on:
  - svc-lending
data-stores:
owner: robin.hale
tags: [ internal ]
---

# Catalogue API

`Service: svc-catalogue-api` `LIVE`

The gateway behind the public catalogue — the only API the reader-facing site talks to, and the boundary that keeps the
internal services off the public internet.

## What it does

Publishes a simplified API to [svc-catalogue-web] and aggregates across the services behind it, so the site makes one
call where it would otherwise make four.

Two capabilities are **compiled into this application** rather than deployed beside it. Account management and loan
history both live under `src/Services` in the monorepo, but nothing deploys them independently, so they are part of this
service rather than services in their own right. The `platform` repository therefore yields three services in this
catalogue rather than the five components its solution file suggests.

## Where it lives

* **Repository** — [`platform`](https://git.example.com/example-libraries/platform) — `src/ApiGateways/Catalogue`
* **Platform** — ASP.NET Core Web API (.NET 10)
* **Deployed as** — App Service `app-catalogue-api-<env>`

Deployed by the `platform` release pipeline as the `api` app, from the `Catalogue.ApiGateway.zip` package.

## Environments

| Environment | URL                                        | Notes            |
|-------------|--------------------------------------------|------------------|
| Development | https://app-catalogue-api-dev.example.net  | No custom domain |
| Test        | https://app-catalogue-api-test.example.net | No custom domain |
| Production  | https://app-catalogue-api-prd.example.net  | No custom domain |

**Quick check** — the OpenAPI document:
[dev](https://app-catalogue-api-dev.example.net/openapi/v1.json) ·
[test](https://app-catalogue-api-test.example.net/openapi/v1.json) ·
[prod](https://app-catalogue-api-prd.example.net/openapi/v1.json)

Unlike the reader-facing services this one is reached on its platform-assigned hostname. Runs locally on port 5110.

## Dependencies

Taken from the application settings the infrastructure declares for the app service.

* [svc-lending] — loans, holds and borrower records, configured as `Apis__Lending__Url` with a matching
  `Apis__Lending__Key`.

It is also configured with the identity provider's authority and with the bibliographic data supplier's API, neither of
which is a service in this catalogue.

## Data

Holds a database connection, configured as `ConnectionStrings__Catalogue`. Because the account-management and
loan-history capabilities are compiled into this application rather than deployed beside it, their stores are reached
through that connection rather than through services of their own.

**What that database is, and whether it is the same one [svc-lending] maps, is an open question.** No source this
catalogue was built from establishes it.

## Operational notes

* **Rate limiting and CORS** live at this boundary rather than in the services behind it.
* **Consumers** — [svc-catalogue-web] is the only service configured with this URL. Maintained by hand; nothing checks
  it.

[svc-catalogue-web]: catalogue-web.md
[svc-lending]: lending.md
