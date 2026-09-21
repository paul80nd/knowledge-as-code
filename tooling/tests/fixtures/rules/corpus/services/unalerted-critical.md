---
id: svc-unalerted-critical
type: service
tier: descriptive
status: live
component-type: worker
repos: [ unalerted-critical ]
platform: azure-function
criticality: critical
monitoring-output: none
nfrs: [nfr-0002]
owner: human:alex.doe
---

# Unalerted critical

`Service: svc-unalerted-critical` `LIVE`

## What it does

Grades itself `critical` and emits nothing, so `critical-service-is-alerted` reports it. Every other
service here states a value a person acts on.

## Where it lives

* **Repository**: [`unalerted-critical`](https://git.example.com/fixture/unalerted-critical)

## Environments

| Environment | URL                                    |
|-------------|----------------------------------------|
| Production  | https://unalerted-critical.example.com |
