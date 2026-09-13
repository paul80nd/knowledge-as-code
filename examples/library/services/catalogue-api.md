---
id: svc-catalogue-api
type: service
tier: descriptive
status: live
repo: platform
platform: dotnet-api
criticality: critical
depends-on:
  - svc-lending
owner: human:robin.hale
facets: [ internal ]
tags: [ holdings ]
---

# Catalogue API

`Service: svc-catalogue-api` `LIVE`

The gateway behind the public catalogue, and the API the reader-facing site calls for everything but search and
jacket imagery. It keeps the internal services off the public internet.

## What it does

Publishes a simplified API to [svc-catalogue-web]. It aggregates across the services behind it, so the site makes one
call where it would otherwise make four.

**Two capabilities are compiled into this application.** Account management and loan history are both under
`src/Services` in the monorepo. Nothing deploys either one on its own, so both are part of this service. The `platform`
repository therefore produces three services in this catalogue, though its solution file shows five components.

## Where it lives

* **Repository**: [`platform`](https://git.example.com/example-libraries/platform), at `src/ApiGateways/Catalogue`
* **Platform**: ASP.NET Core Web API (.NET 10)
* **Deployed as**: App Service `app-catalogue-api-<env>`

The `platform` release pipeline deploys it as the `api` app, from the `Catalogue.ApiGateway.zip` package.

## Environments

| Environment | URL                                        | Notes            |
|-------------|--------------------------------------------|------------------|
| Development | https://app-catalogue-api-dev.example.net  | No custom domain |
| Test        | https://app-catalogue-api-test.example.net | No custom domain |
| Production  | https://app-catalogue-api-prd.example.net  | No custom domain |

**Quick check**: the OpenAPI document, in
[dev](https://app-catalogue-api-dev.example.net/openapi/v1.json) ·
[test](https://app-catalogue-api-test.example.net/openapi/v1.json) ·
[prod](https://app-catalogue-api-prd.example.net/openapi/v1.json).

This service has no custom domain, so callers use the platform-assigned hostname. It runs locally on port 5110.

## Dependencies

Taken from the application settings the infrastructure declares for the app service.

* [svc-lending] serves loans, holds and borrower records. It is configured as `Apis__Lending__Url`, with a matching
  `Apis__Lending__Key`.

The same settings include the identity provider's authority and the bibliographic data supplier's API. Neither is a
service in this catalogue.

## Data

This service has a database connection, configured as `ConnectionStrings__Catalogue`. The account-management and
loan-history capabilities are compiled into this application, so their stores are read through it too.

**What that database is, and whether it is the same one [svc-lending] maps, is an open question.** No source this
catalogue was built from states it.

## Operational notes

* **Rate limiting and CORS.** Both are configured here, and neither is configured on the services behind this gateway.
* **Consumers.** [svc-catalogue-web] is the only service configured with this URL. Nothing checks this line, so it goes
  stale.

[svc-catalogue-web]: catalogue-web.md
[svc-lending]: lending.md
