---
id: svc-shelf-audit
tier: descriptive
status: deprecated
repo: shelf-audit
platform: mixed
criticality: supporting
depends-on:
data-stores:
owner: dev.raman
facets: [ internal, scheduled ]
tags: [ monitoring ]
---

# Shelf Audit

`Service: svc-shelf-audit` `DEPRECATED`

A nightly outside-in check that every branch's catalogue pages respond, reported by email.

> **Being replaced** by the platform's own synthetic monitoring. This service runs until that is in place.

## What it does

Once a night it walks the list of branch catalogue URLs and checks each one. The check confirms that both the bare and
`www` forms respond over HTTPS. It collates the results and emails them.

It is the estate's only outside-in check that the catalogue is reachable. Nothing consumes its output except a mailbox.
Whether an unread nightly email is the right home for that signal is the question behind its deprecation.

## Where it lives

* **Repository**: [`shelf-audit`](https://git.example.com/example-libraries/shelf-audit)
* **Platform**: a PowerShell function and a workflow app, deployed together
* **Deployed as**: both, by Terraform from within the same repository

**It is deployed unlike anything else in the estate.** There is no release pipeline. The repository holds its own
Terraform configuration, which somebody applies from a workstation against a dedicated workspace. It runs in a
pay-as-you-go subscription, while the rest of the estate uses a subscription per environment.

`platform` is `mixed`: a PowerShell function and a workflow app are two runtimes. Terraform deploys this service, and
deployment does not decide the field.

## Environments

| Environment | URL | Notes                                              |
|-------------|-----|----------------------------------------------------|
| Development |     | Does not exist                                     |
| Test        |     | Does not exist                                     |
| Production  |     | `func-shelf-audit-prd` and `logic-shelf-audit-prd` |

**Production only**, and by hard-coding: `-prd` is written into the resource names, so there is no other environment to
deploy to. A change cannot be rehearsed before it runs against production. Every other service in the estate runs in
three environments.

## Dependencies

None. It reads the branch list from a file in its own repository and checks the catalogue from the outside, as any
reader would.

## Operational notes

* **Schedule**: nightly.
* **Output**: an email. Nothing else reads the result, and no alert is raised from it.
* **Criticality**: `supporting`. Nothing a reader touches depends on it. Where it stops, the only loss is the signal
  itself. It is also the check that would notice the catalogue being down, so its own silence is indistinguishable from
  good news.
