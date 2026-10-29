---
id: svc-payments
type: service
tier: descriptive
status: live
component-type: api
repos:
  - Platform/Infra Tools
  - Proj X/My Repo
platform: dotnet-web
criticality: important
monitoring-output: ticket
owner: human:alex.doe
---

# Payments

`Service: svc-payments` `LIVE`

## What it does

Resolves two repositories in two projects of one organisation, on both Azure DevOps hosts.

## Where it lives

* **Repositories**: [`Infra Tools`](https://acme.visualstudio.com/Platform/_git/Infra%20Tools?path=/src) and
  [`My Repo`](https://dev.azure.com/acme/proj%20x/_git/my%20repo)

## Environments

| Environment | URL                          |
|-------------|------------------------------|
| Production  | https://payments.example.com |
