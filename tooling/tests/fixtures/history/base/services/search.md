---
id: svc-search
type: service
tier: descriptive
status: live
component-type: api
repos: [ search ]
platform: dotnet-web
criticality: supporting
monitoring-output: alert
owner: human:alex.doe
---

# Search

`Service: svc-search` `LIVE`

## What it does

Answers catalogue searches. It is here because its id changes between the two revisions while its file moves, and
the postmortem that names it follows the new id.

## Where it lives

* **Repository**: [`search`](https://git.example.com/fixture/search)

## Environments

| Environment | URL                        |
|-------------|----------------------------|
| Production  | https://search.example.com |
