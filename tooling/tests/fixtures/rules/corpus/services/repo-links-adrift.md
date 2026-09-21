---
id: svc-repo-links-adrift
type: service
tier: descriptive
status: live
component-type: worker
repos: [ repo-links-adrift ]
platform: azure-function
criticality: important
monitoring-output: ticket
owner: human:alex.doe
---

# Repo links adrift

`Service: svc-repo-links-adrift` `LIVE`

## What it does

States one repository in `repos` and links another under `Where it lives`, so `mirrors-repo-links` reports
it twice: once for the entry nothing links, and once for the link the field never states.

## Where it lives

* **Repository**: [`elsewhere`](https://git.example.com/fixture/elsewhere)

## Environments

| Environment | URL                                    |
|-------------|----------------------------------------|
| Production  | https://repo-links-adrift.example.com  |
