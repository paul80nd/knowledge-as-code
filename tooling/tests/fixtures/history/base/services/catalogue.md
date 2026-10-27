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
owner: human:alex.doe
---

# Catalogue

`Service: svc-catalogue` `LIVE`

## What it does

Serves the public catalogue. It is here because the postmortem names it in `affected`, and `ref-resolves` asks that
the id lands somewhere.

## Where it lives

* **Repository**: [`catalogue`](https://git.example.com/fixture/catalogue)

## Environments

| Environment | URL                           |
|-------------|-------------------------------|
| Production  | https://catalogue.example.com |
