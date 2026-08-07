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
tags: [ public ]
---

# Catalogue

`Service: svc-catalogue-web` `LIVE`

The public catalogue — the site a reader visits to browse the collection, search it and place a hold.

## What it does

Serves the reader-facing catalogue: search results, item pages, and a reader's own loans and holds. It renders no
staff-facing screens and holds no data of its own — everything it shows arrives through [svc-catalogue-api].

Authentication is delegated to the identity provider at `id.example.com`, which issues the tokens the gateway
validates. The branch terminals run the same application in a kiosk profile rather than a separate deployable, which
is why there is no second service for them.

## Where it lives

* **Repository** — [`platform`](https://git.example.com/example-libraries/platform) — `src/Web/Catalogue`
* **Platform** — ASP.NET Core MVC (.NET 10)
* **Deployed as** — App Service `app-catalogue-<env>`

Deployed by the `platform` release pipeline as the `catalogue` app, from the `Catalogue.Web.zip` package.

## Environments

| Environment | URL                                | Notes |
|-------------|------------------------------------|-------|
| Development | https://catalogue-dev.example.com  |       |
| Test        | https://catalogue-test.example.com |       |
| Production  | https://catalogue.example.com      |       |

**Quick check** — the collection home page:
[dev](https://catalogue-dev.example.com/) ·
[test](https://catalogue-test.example.com/) ·
[prod](https://catalogue.example.com/)

Runs locally on port 5100.

## Dependencies

Taken from the application settings the infrastructure declares for the app service, so this is what it is configured
to reach rather than what it is observed to call.

* [svc-catalogue-api] — the gateway, configured as `Urls__CatalogueApi`. Everything the site reads and writes.
* [svc-search] — configured as `Urls__Search`, for the search box and faceted browse.
* [svc-covers-cdn] — jacket imagery for result lists and item pages.

**A `critical` service depending on an `important` one.** [svc-search] is graded below this service, which looks like
a mis-grading and is not. When search is unavailable the catalogue falls back to browse-by-shelf and every item page
still renders, so a reader loses a feature rather than the site. Recorded here because it is the kind of edge worth
defending in review rather than discovering later.

## Data

No database connection is configured for this app service. It reaches data through the gateway.

[svc-catalogue-api]: catalogue-api.md
[svc-covers-cdn]: covers-cdn.md
[svc-search]: search.md
