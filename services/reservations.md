---
id: svc-reservations
tier: descriptive
status: building
repo: platform
platform: dotnet-web
criticality: important
depends-on:
  - svc-lending
data-stores:
owner: mira.okonjo
tags: [ public ]
---

# Reservations

`Service: svc-reservations` `BUILDING`

The reserve-and-pay flow — the pages a reader completes to place a hold on an item and settle any fee it attracts.

## What it does

Hosts the hold-and-pay flow as a server-rendered application, deploy-isolated from the rest of the monorepo and reached
on its own hostname. The flow is multi-step and progressive rather than a single-page application, so it works on the
branch terminals as well as on a reader's own device.

It is **stateless**. Wizard state before confirmation is ephemeral and carried through the flow; the only durable write
is the provisional hold it creates in [svc-lending] at confirm, which doubles as the reference a returning card payment
is matched against. Resuming a half-finished reservation in a later session is deliberately not supported.

## Where it lives

* **Repository** — [`platform`](https://git.example.com/example-libraries/platform) — `src/Services/Reservations`
* **Platform** — ASP.NET Core Razor Pages (.NET 10)
* **Deployed as** — App Service `app-reservations-<env>`

Deployed by the `platform` release pipeline as the `reservations` app, from the `Reservations.Web.zip` package. It is
the third deployable out of that repository.

## Environments

| Environment | URL                              | Notes                     |
|-------------|----------------------------------|---------------------------|
| Development | https://reserve-dev.example.com  |                           |
| Test        | https://reserve-test.example.com |                           |
| Production  | https://reserve.example.com      | Served, not yet linked to |

Production is deployed ahead of release and is reachable, but [svc-catalogue-web] does not yet link to it. The hostname
is `reserve`, not `reservations`, in every environment. The short form comes from the printed branch signage and is
carried through the other environments so they match.

## Dependencies

* [svc-lending] — configured as `Apis__Lending__Url`. Reached over the private network rather than through the public
  edge.

## Data

**None of its own**, and checked rather than assumed: the infrastructure declares no database, no storage account and no
connection string for this app service. The single durable write is the provisional hold created in [svc-lending], which
owns it.

## Operational notes

* **Payments** — the card payment is taken on the payment provider's hosted page and the reader returns against the hold
  reference. No card detail is held by this service. The provider is an integration rather than a service, so it is not
  an edge in the graph.
* **Criticality** — `important`. A failure stops readers reserving online and branch staff fall back to placing holds at
  the desk, which is degradation rather than a platform failure.

[svc-catalogue-web]: catalogue-web.md
[svc-lending]: lending.md
