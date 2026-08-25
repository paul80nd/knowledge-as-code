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
facets: [ internal ]
tags: [ legacy, loans ]
---

# Lending

`Service: svc-lending` `LIVE`

Loans, returns and holds: a strangler wrapper over the legacy library management system, and the only route to the data
that still lives there.

## What it does

Exposes loans, holds and borrower records as a RESTful API. It fronts the legacy library management system, and
overrides the routes it replaces as it gains functionality. That is the strangler pattern, and it is deliberate.

**It maps onto the legacy database and owns no schema of its own.** This is the estate's one deliberate exception to
service-owned data. The legacy system still writes to that same database, and this service migrates none of it. Its
object-relational mapping is a mapper over pre-existing tables that this service did not design.

**It is therefore the only way to reach legacy-only data.** Anything that exists only in that database is reachable only
by asking this service. A service that needs such data calls this one. The branch opening calendars are the worked
example.

## Where it lives

* **Repository**: [`lending`](https://git.example.com/example-libraries/lending)
* **Platform**: ASP.NET Core Web API (.NET 10)
* **Deployed as**: App Service `app-lending-<env>`

## Environments

| Environment | URL                                  | Notes            |
|-------------|--------------------------------------|------------------|
| Development | https://app-lending-dev.example.net  | No custom domain |
| Test        | https://app-lending-test.example.net | No custom domain |
| Production  | https://app-lending-prd.example.net  | No custom domain |

**Quick check**: the OpenAPI document, in
[dev](https://app-lending-dev.example.net/openapi/v1.json) ·
[test](https://app-lending-test.example.net/openapi/v1.json) ·
[prod](https://app-lending-prd.example.net/openapi/v1.json).

Callers reach it over the private network wherever possible. The hostnames exist because the platform assigns them, and
nothing is meant to call them.

## Dependencies

No service in this catalogue (which is why `depends-on` is bare). Its downward dependencies are all legacy:

* The **legacy database**, configured as `ConnectionStrings__Legacy`.
* The **legacy circulation API**, reached through a reverse-proxy cluster. That API is the unreplaced half of the
  strangler.

## Data

**The legacy database**, which this service maps and does not own. Any data document describing it has to record more
than one writer: the legacy library management system still writes to it directly.

## Operational notes

* **Authentication.** Callers present an API key, configured as `Api__Key`. Each caller holds its own.
* **Consumers.** [svc-catalogue-api] and [svc-reservations] are each configured to reach it. Nothing checks this line,
  so it goes stale.
* **Criticality**: `critical`. It is the estate's only route to loan data, and nothing degrades gracefully without it.

[svc-catalogue-api]: catalogue-api.md
[svc-reservations]: reservations.md
