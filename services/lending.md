---
id: svc-lending
tier: descriptive
status: live
repo: lending
platform: dotnet-api
criticality: critical
depends-on:
data-stores:
owner: dev.raman
tags: [ internal ]
---

# Lending

`Service: svc-lending` `LIVE`

Loans, returns and holds — a strangler wrapper over the legacy library management system, and the only route to the
data that still lives there.

## What it does

Exposes loans, holds and borrower records as a RESTful API. It began as a wrapper: it fronted the legacy library
management system, gaining functionality over time and overriding the routes it replaces as it does — the strangler
pattern, deliberately.

Two things follow from that, and both matter more than the API surface itself.

**It maps onto the legacy database rather than owning one.** This is the estate's one deliberate exception to
service-owned data: it maps directly onto the same database the legacy system still writes to, does not own that
schema, and does not migrate it. The object-relational mapping is a mapper over pre-existing tables, not a model this
service designed.

**It is therefore the only way to reach legacy-only data.** Anything that exists only in that database — the branch
opening calendars are the worked example — is reachable only by asking this service. A service that needs it calls
this one rather than reading the database itself.

## Where it lives

* **Repository** — [`lending`](https://git.example.com/example-libraries/lending)
* **Platform** — ASP.NET Core Web API (.NET 10)
* **Deployed as** — App Service `app-lending-<env>`

## Environments

| Environment | URL                                   | Notes            |
|-------------|---------------------------------------|------------------|
| Development | https://app-lending-dev.example.net   | No custom domain |
| Test        | https://app-lending-test.example.net  | No custom domain |
| Production  | https://app-lending-prd.example.net   | No custom domain |

**Quick check** — the OpenAPI document:
[dev](https://app-lending-dev.example.net/openapi/v1.json) ·
[test](https://app-lending-test.example.net/openapi/v1.json) ·
[prod](https://app-lending-prd.example.net/openapi/v1.json)

Reached over the private network rather than these hostnames wherever possible. The hostnames exist because the
platform assigns them, not because anything is meant to call them.

## Dependencies

No service in this catalogue. Its downward dependencies are all legacy:

* The **legacy database**, configured as `ConnectionStrings__Legacy`.
* The **legacy circulation API**, reached through a reverse-proxy cluster. This is the half of the strangler that has
  not been replaced yet.

Neither is a service in this catalogue, so neither is an edge — which is why `depends-on` is bare on a service that
depends on a great deal. A bare field means "nothing in this catalogue", not "nothing at all".

## Data

**The legacy database**, which it maps but does not own. Any data document describing that database has to record more
than one writer: the legacy library management system still writes to it directly.

## Operational notes

* **Authentication** — callers present an API key, configured as `Api__Key`. Each caller holds its own.
* **Consumers** — [svc-catalogue-api] and [svc-reservations] are each configured to reach it. Judged `critical` on
  that basis: it is the estate's single point of access to loan data, and nothing degrades gracefully without it.
  This list is maintained by hand and nothing checks it against the graph.

[svc-catalogue-api]: catalogue-api.md
[svc-reservations]: reservations.md
