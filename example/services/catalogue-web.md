---
id: svc-catalogue-web
tier: descriptive
status: live
repo: platform
platform: dotnet-web
criticality: critical
depends-on:
  - svc-catalogue-api
  - svc-covers-cdn
  - svc-search
data-stores:
owner: robin.hale
facets: [ public ]
tags: [ discovery ]
---

# Catalogue

`Service: svc-catalogue-web` `LIVE`

The public catalogue: the site a reader visits to browse the collection, search it and place a hold.

## What it does

Serves the reader-facing catalogue: search results, item pages, and a reader's own loans and holds. It renders no
staff-facing screens and holds no data of its own. Everything it shows arrives through [svc-catalogue-api].

It delegates authentication to the identity provider at `id.example.com`, which issues the tokens the gateway validates.
The branch terminals run the same application in a kiosk profile, so no second service exists for them.

## Where it lives

* **Repository**: [`platform`](https://git.example.com/example-libraries/platform), at `src/Web/Catalogue`
* **Platform**: ASP.NET Core MVC (.NET 10)
* **Deployed as**: App Service `app-catalogue-<env>`

The `platform` release pipeline deploys it as the `catalogue` app, from the `Catalogue.Web.zip` package.

## Environments

| Environment | URL                                | Notes |
|-------------|------------------------------------|-------|
| Development | https://catalogue-dev.example.com  |       |
| Test        | https://catalogue-test.example.com |       |
| Production  | https://catalogue.example.com      |       |

**Quick check**: the collection home page, in
[dev](https://catalogue-dev.example.com/) ·
[test](https://catalogue-test.example.com/) ·
[prod](https://catalogue.example.com/).

It runs locally on port 5100.

## Dependencies

Taken from the application settings the infrastructure declares for the app service.

* [svc-catalogue-api] is the gateway, configured as `Urls__CatalogueApi`. Everything the site reads and writes goes
  through it.
* [svc-search] serves the search box and faceted browse, configured as `Urls__Search`.
* [svc-covers-cdn] serves jacket imagery for result lists and item pages.

**A `critical` service depending on an `important` one.** When [svc-search] is unavailable, the catalogue falls back to
browse-by-shelf and every item page still renders. A reader loses a feature and keeps the site. The argument for that
grading is recorded on [svc-search].

## Data

No database connection is configured for this app service. It reaches data through the gateway.

[svc-catalogue-api]: catalogue-api.md
[svc-covers-cdn]: covers-cdn.md
[svc-search]: search.md
