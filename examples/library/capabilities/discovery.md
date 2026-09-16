---
id: cap-discovery
type: capability
tier: descriptive
status: live
implemented-by:
  - svc-catalogue-api
  - svc-catalogue-web
  - svc-covers-cdn
  - svc-search
ado-epics: [ 1204, 1338 ]
feature-files:
  - platform/tests/Catalogue.Acceptance/browse.feature
  - platform/tests/Catalogue.Acceptance/item-page.feature
  - search/features/faceted-browse.feature
  - search/features/type-ahead.feature
owner: human:robin.hale
tags: [ discovery ]
---

# Find a title in the collection

`Capability: cap-discovery` `LIVE`

A borrower searches the consortium's collection, sees which branches hold a copy, and opens the item they want.

## What it does

Search reaches the whole consortium. A borrower standing in one
[branch](../glossary/example-libraries.md#branch) sees stock in every other, and may place a hold on any of it.

## Why it exists

A borrower who cannot find a title does not borrow it. Discovery is the only surface most borrowers ever use, and every
other capability starts here.

## Surfaces

The public catalogue at `catalogue.example.com`, and the same pages in the kiosk profile on the branch terminals.

## Where the detail lives

|                    |                                                           |
|--------------------|-----------------------------------------------------------|
| **Implemented by** | [svc-catalogue-web], [svc-catalogue-api], [svc-search],   |
|                    | [svc-covers-cdn]                                          |
| **Specified in**   | ADO epics #1204, #1338                                    |
| **Tested by**      | `browse.feature`, `item-page.feature`,                    |
|                    | `faceted-browse.feature`, `type-ahead.feature`            |

## Known limitations

A title the supplier has sent no jacket image for renders a placeholder.

[svc-catalogue-api]: ../services/catalogue-api.md
[svc-catalogue-web]: ../services/catalogue-web.md
[svc-covers-cdn]: ../services/covers-cdn.md
[svc-search]: ../services/search.md
