---
id: svc-catalogue
type: service
tier: descriptive
status: live
component-type: api
repos: [ catalogue ]
platform: dotnet-web
criticality: critical
monitoring-output: alert
depends-on: [svc-notifications]
data-stores: [dat-borrower-records, dat-reader-contact-list]
owner: human:alex.doe
---

# Catalogue

`Service: svc-catalogue` `LIVE`

## What it does

Serves the public catalogue. It is here because the records around it name it — a data domain it owns, an integration
it uses, a standard it is held to, and two postmortems it appears in — and `ref-resolves` asks that each of those ids
lands somewhere. It is graded `critical` and depends on a service graded `important`, which is the edge
`dependency-criticality` reports.

## Where it lives

* **Repository**: [`catalogue`](https://git.example.com/fixture/catalogue)

## Environments

| Environment | URL                           |
|-------------|-------------------------------|
| Production  | https://catalogue.example.com |
