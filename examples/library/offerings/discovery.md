---
id: ofr-discovery
type: offering
tier: descriptive
status: live
implemented-by:
  - svc-catalogue-api
  - svc-catalogue-web
  - svc-covers-cdn
  - svc-search
feature-files:
  - platform/tests/Catalogue.Acceptance/browse.feature
  - platform/tests/Catalogue.Acceptance/item-page.feature
  - search/features/faceted-browse.feature
  - search/features/type-ahead.feature
owner: human:robin.hale
tags: [ discovery ]
---

# Find a title in the collection

`Offering: ofr-discovery` `LIVE`

A borrower searches the consortium's collection, sees which branches hold a copy, and opens the item they want.

## What it does

Search reaches the whole consortium. A borrower standing in one
[branch](../glossary/example-libraries.md#branch) sees stock in every other, and may place a hold on any of it.

## Who it is for

Anyone. The catalogue answers a search without a card, so a person who has not joined yet still reaches
this.

## Why it exists

A borrower who cannot find a title does not borrow it. Discovery is the only surface most borrowers ever use, and every
other offering starts here.

## Surfaces

The public catalogue at `catalogue.example.com`, and the same pages in the kiosk profile on the branch terminals.

## Where the detail lives

* **Implemented by**: [svc-catalogue-api], [svc-catalogue-web], [svc-covers-cdn], [svc-search]
* **Specified in**: [ADO#1204], [ADO#1338]

## Known limitations

A title the supplier has sent no jacket image for renders a placeholder.

[ADO#1204]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/1204
[ADO#1338]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/1338
[svc-catalogue-api]: ../services/catalogue-api.md
[svc-catalogue-web]: ../services/catalogue-web.md
[svc-covers-cdn]: ../services/covers-cdn.md
[svc-search]: ../services/search.md
