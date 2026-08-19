---
id: svc-search
tier: descriptive
status: live
repo: search
platform: typescript
criticality: important
depends-on:
data-stores:
owner: mira.okonjo
facets: [ event-driven, internal ]
tags: [ discovery, indexing ]
---

# Search

`Service: svc-search` `LIVE`

The catalogue search index — rebuilt from bus events, queried by the catalogue.

## What it does

Maintains the search index over the collection and answers queries against it: free-text search, faceted browse, and the
type-ahead on the catalogue's search box.

The index is **not** read from a database. It is built from events: every change to an item, a holding or a branch's
stock arrives on the message bus and is applied incrementally, with a full rebuild available on demand. That is what
makes it the estate's clearest example of heavy coupling with no dependency edge.

## Where it lives

* **Repository** — [`search`](https://git.example.com/example-libraries/search)
* **Platform** — Node.js with TypeScript, on a container image
* **Deployed as** — Container App `ca-search-<env>`

## Environments

| Environment | URL                             | Notes            |
|-------------|---------------------------------|------------------|
| Development | https://search-dev.example.net  | No custom domain |
| Test        | https://search-test.example.net | No custom domain |
| Production  | https://search-prd.example.net  | No custom domain |

**Quick check** — the index health document:
[dev](https://search-dev.example.net/health/index) ·
[test](https://search-test.example.net/health/index) ·
[prod](https://search-prd.example.net/health/index)

## Dependencies

**None, and the graph is right.** This service cannot function without the events [svc-catalogue-api] and
[svc-lending] publish, so it is coupled to both as tightly as anything in the estate. Neither is an edge: `depends-on`
records calls, and a bus message is not a call — see [Services](/services).

So this service looks unconnected while being anything but. What it consumes is in Operational notes below, which is
where the coupling actually lives.

## Data

The index itself, held in the search engine's own storage and rebuildable from the bus. Nothing here is a system of
record: if the index is lost it is rebuilt, and no other service reads it.

## Operational notes

* **Messaging** — subscribes to three topics: `catalogue.item_changed`, `catalogue.holding_changed` and
  `lending.stock_moved`. Each has its own subscription, named `search`.
* **Rebuild** — a full rebuild takes around forty minutes and is run from the pipeline rather than on a timer, so a
  rebuild is a deliberate act with someone watching it.
* **Criticality** — `important`. When it stops, the catalogue falls back to browse-by-shelf and every item page still
  renders; a reader loses search rather than the site. That is the argument for `important` over `critical`, and it is
  why [svc-catalogue-web] is graded above one of its own dependencies.

[svc-catalogue-api]: catalogue-api.md
[svc-catalogue-web]: catalogue-web.md
[svc-lending]: lending.md
