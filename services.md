# Services

The catalogue of deployable components that make up the platform.

**[→ Index](services/INDEX.md)**

## What is a service?

One document per independently deployable component — a web app, an API, an Azure Function, a CDN asset bundle, an
infrastructure stack. It records what the component is for, where its code lives, what it runs on, what it depends on,
what data it owns, and who is answerable for it.

## Why we use them

This is the **anchor** the rest of the wiki points at. NFRs apply to services. Controls apply to services. Capabilities
are implemented by services. FAQs affect services. None of those cross-references can be validated — or even written
consistently — without one canonical list of what a service is and what it is called.

It is also the answer to the question new contributors and AI sessions ask most often: *which of the seventeen
repositories does this thing live in, and what talks to it?*

## Scope

One document per **deployable unit**, not per repository and not per feature. A repository containing two independently
deployed apps gets two documents; a capability spanning six services gets a
[capability](/capabilities) document that links to all six.

A service document is **descriptive**: it mirrors what is actually deployed. It is not the place for:

* **How to deploy it** — that is a [process](/processes).
* **How to fix it when it breaks** — that is a [runbook](/runbooks).
* **Why it is shaped the way it is** — that is an [ADR](/adrs) or an [explanation](/explanations).
* **What it promises** — availability and latency targets are [NFRs](/nfrs).

Third-party systems we depend on are [integrations](/integrations), not services — the line is whether we deploy it.

## Metadata

<!-- BEGIN GENERATED: schema-services -->

| Field         | Req | Type   | Notes                                                                                            |
|---------------|-----|--------|--------------------------------------------------------------------------------------------------|
| `status`      | ●   | enum   | `live` · `building` · `deprecated` · `retired`                                                   |
| `repo`        | ●   | string | Repository name                                                                                  |
| `platform`    | ●   | enum   | `dotnet-web` · `dotnet-api` · `azure-function` · `static` · `typescript` · `terraform` · `mixed` |
| `criticality` | ●   | enum   | `critical` · `important` · `supporting`                                                          |
| `depends-on`  |     | list   | Service ids                                                                                      |
| `data-stores` |     | list   |                                                                                                  |

<!-- END GENERATED: schema-services -->

## Adding a service

1. Copy [`template.md`](services/template.md) to `<slug>.md`. Services use a **slug id**, not a number —
   `svc-<name>` — because they have natural stable names.
2. Fill in the frontmatter. `depends-on` names other service ids; CI checks they resolve.
3. Record environments and URLs, the data stores it owns, and a named owner.
4. Keep it current. This is a descriptive document — a service catalogue that disagrees with the estate is worse than
   none, because everything else trusts it.

**Conventions**

* **Slug** — matches the repository or component name where one exists (`<component>` → `svc-<component>`).
* **Criticality** — `critical` means customer-facing failure; `important` means degraded service; `supporting` means
  internal impact only. It drives runbook and NFR prioritisation, so be honest rather than generous.
* **Dependencies point downward only.** Record what this service calls; the reverse view is generated.

## What CI checks

<!-- BEGIN GENERATED: checks-services -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-services -->
