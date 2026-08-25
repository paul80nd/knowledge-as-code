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
facets: [ public ]
tags: [ holds, payments ]
---

# Reservations

`Service: svc-reservations` `BUILDING`

The reserve-and-pay flow: the pages a reader completes to place a hold on an item and settle any fee it attracts.

## What it does

Hosts the reserve-and-pay flow as a server-rendered application. It is deploy-isolated from the rest of the monorepo and
reached on its own hostname. The flow is multistep and progressive, so it works on the branch terminals as well as on a
reader's own device.

**Stateless.** Wizard state before confirmation is ephemeral and carried through the flow. The only durable write is the
provisional hold it creates in [svc-lending] at confirm. That hold doubles as the reference a returning card payment is
matched against. Resuming a half-finished reservation in a later session is deliberately not supported.

## Where it lives

* **Repository**: [`platform`](https://git.example.com/example-libraries/platform), at `src/Services/Reservations`
* **Platform**: ASP.NET Core Razor Pages (.NET 10)
* **Deployed as**: App Service `app-reservations-<env>`

The `platform` release pipeline deploys it as the `reservations` app, from the `Reservations.Web.zip` package. It is the
third deployable out of that repository.

## Environments

| Environment | URL                              | Notes               |
|-------------|----------------------------------|---------------------|
| Development | https://reserve-dev.example.com  |                     |
| Test        | https://reserve-test.example.com |                     |
| Production  | https://reserve.example.com      | Served, no link yet |

Production is deployed ahead of release and is reachable, but [svc-catalogue-web] does not yet link to it. The hostname
is `reserve`, not `reservations`, in every environment. The short form comes from the printed branch signage, and the
other environments carry it so they match production.

## Dependencies

* [svc-lending] is configured as `Apis__Lending__Url`. This service reaches it over the private network.

## Data

**None of its own** (verified): the infrastructure declares no database, no storage account and no connection string for
this app service. The single durable write is the provisional hold created in [svc-lending], which owns it.

## Operational notes

* **Payments.** The payment provider takes the card payment on its own hosted page, and the reader returns against the
  hold reference. This service holds no card detail. We do not deploy that provider, so it is an integration and absent
  from the graph.
* **Criticality**: `important`. A failure stops readers reserving online, and branch staff fall back to placing holds at
  the desk. The platform degrades and keeps running.

[svc-catalogue-web]: catalogue-web.md
[svc-lending]: lending.md
