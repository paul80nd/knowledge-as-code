---
id: svc-{{slug}}
tier: descriptive
status: live
repo:
platform:
criticality:
depends-on:
data-stores:
owner:
tags: [ a, b ]
---

# {{Service name}}

`Service: svc-{{slug}}` `LIVE`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`id`** — `svc-` plus the repository or component name.
* **`status`** — `live` · `building` · `deprecated` · `retired`.
* **`platform`** — `dotnet-web` · `dotnet-api` · `azure-function` · `static` · `typescript` · `terraform` · `mixed`.
* **`criticality`** — `critical` if a customer sees the failure, `important` if service degrades, `supporting` if the
  impact is internal only. It drives runbook and NFR priority, so be honest rather than generous.
* **`depends-on`** — Other service ids, pointing downward only — the reverse view is generated.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences: what this component is for, in terms someone unfamiliar with it would understand.

## What it does

The responsibilities this service owns. Keep it to what is true today — this is a descriptive document and a catalogue
that disagrees with the estate is worse than none.

## Where it lives

|                 |                                                 |
|-----------------|-------------------------------------------------|
| **Repository**  | [`{{repo-name}}`]({{url}})                      |
| **Platform**    | {{runtime / framework}}                         |
| **Deployed as** | {{app service / function app / CDN assets / …}} |

## Environments

| Environment | URL | Notes |
|-------------|-----|-------|
| Development |     |       |
| Test        |     |       |
| Production  |     |       |

## Dependencies

* **[svc-example](example.md)** — what this service calls it for.
* **[int-example](/integrations/example.md)** — external systems it depends on.

_(Downward only. If nothing is listed, say "none" rather than leaving it blank — an empty section reads as unfinished.)_

## Data

What this service owns, and where it stores it. Link to the [data](/data) document for the domain rather than describing
schemas here.

## Operational notes

* **Runbooks** — [rbk-example](/runbooks/example.md)
* **NFRs** — [nfr-example](/nfrs/example.md)
* **Known issues** — link any [FAQs](/faqs) that recur for this service.

_(Links only. How to operate it belongs in a process or runbook; why it is shaped this way belongs in an ADR or
explanation.)_
