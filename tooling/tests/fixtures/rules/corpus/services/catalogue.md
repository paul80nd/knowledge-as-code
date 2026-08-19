---
id: svc-catalogue
tier: descriptive
status: live
repo: catalogue
platform: dotnet-web
criticality: critical
depends-on: [svc-notifications]
data-stores: [dat-borrower-records, dat-reader-contact-list]
owner: alex.doe
---

# Catalogue

`Service: svc-catalogue` `LIVE`

## What it does

Serves the public catalogue. It is here because the records around it name it — a data domain it owns, an integration
it uses, a standard it is held to, and two postmortems it appears in — and `ref-resolves` asks that each of those ids
lands somewhere.

## Where it lives

The `catalogue` repository.

## Environments

| Environment | URL                             |
|-------------|---------------------------------|
| Production  | https://catalogue.example.com   |
