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
tags: [ internal, scheduled ]
---

# Shelf Audit

`Service: svc-shelf-audit` `DEPRECATED`

A nightly outside-in check that every branch's catalogue pages respond, reported by email.

> **Being replaced** by the platform's own synthetic monitoring. It still runs, and this record stays until it is
> switched off, because it is the only thing checking the catalogue from outside today.

## What it does

Once a night it walks the list of branch catalogue URLs and checks each one, confirming that both the bare and `www`
forms respond over HTTPS. Results are collated and emailed.

It is the estate's only outside-in check that the catalogue is reachable — and nothing consumes its output except a
mailbox. Whether an unread nightly email is the right home for that signal is the question that got it deprecated.

## Where it lives

* **Repository** — [`shelf-audit`](https://git.example.com/example-libraries/shelf-audit)
* **Platform** — a PowerShell function and a workflow app, deployed together
* **Deployed as** — both, by Terraform from within the same repository

**It is deployed unlike anything else in the estate.** There is no release pipeline: the repository holds its own
Terraform configuration and is applied from a workstation against a dedicated workspace, rather than from source
control. It runs in a pay-as-you-go subscription rather than the per-environment subscriptions the rest of the estate
uses.

`platform` is `mixed` rather than `terraform`, and the distinction is the point: the field describes what a service is
**built on**, not what deploys it. A Terraform-deployed service is not a Terraform service, and infrastructure-as-code
deploys services rather than being one.

## Environments

| Environment | URL | Notes                                          |
|-------------|-----|------------------------------------------------|
| Development |     | Does not exist                                 |
| Test        |     | Does not exist                                 |
| Production  |     | `func-shelf-audit-prd` and `logic-shelf-audit-prd` |

**Production only**, and not by configuration: `-prd` is written into the resource names rather than derived from an
environment variable, so there is no other environment to deploy to. A change cannot be rehearsed before it runs
against production. Every other service in the estate runs in three environments.

## Dependencies

None. It reads the branch list from a file in its own repository and checks the catalogue from the outside, as any
reader would.

## Operational notes

* **Schedule** — nightly.
* **Output** — an email. Nothing else reads the result and no alert is raised from it.
* **Criticality** — `supporting`. Nothing a reader touches depends on it; if it stops, the only loss is the signal
  itself. That said, it is the check that would notice the catalogue being down, so its own silence is
  indistinguishable from good news.
