---
id: cap-reservations
type: capability
tier: descriptive
status: building
implemented-by:
  - svc-catalogue-web
  - svc-lending
  - svc-notices
  - svc-reservations
feature-files:
  - platform/tests/Reservations.Acceptance/pay-fee.feature
  - platform/tests/Reservations.Acceptance/place-hold.feature
owner: human:mira.okonjo
tags: [ holds, payments ]
---

# Reserve an item and pay the fee

`Capability: cap-reservations` `BUILDING`

A borrower places a hold on an item held at another branch, pays the fee it attracts, and collects it when it arrives.

## What it does

A hold moves an item between branches, so the fee covers the transport the consortium pays for. The borrower chooses
which branch to collect from, and [svc-notices] tells them when the item gets there.

## Why it exists

A consortium of branches is worth more than a single library only where a borrower can reach stock held elsewhere. The
hold is what makes the shared collection real.

## Surfaces

Its own hostname, apart from the rest of the catalogue, and the same pages on the branch terminals.

## Where the detail lives

|                    |                                                                                     |
|--------------------|-------------------------------------------------------------------------------------|
| **Implemented by** | [svc-reservations]                                                                  |
|                    | [svc-lending]                                                                       |
|                    | [svc-notices]                                                                       |
|                    | [svc-catalogue-web]                                                                 |
| **Specified in**   | [ADO#1455](https://dev.azure.com/example-libraries/consortium/_workitems/edit/1455) |
|                    | [ADO#1462](https://dev.azure.com/example-libraries/consortium/_workitems/edit/1462) |

## Known limitations

This capability is live in no environment above test. [svc-reservations] says what remains.

[svc-catalogue-web]: ../services/catalogue-web.md
[svc-lending]: ../services/lending.md
[svc-notices]: ../services/notices.md
[svc-reservations]: ../services/reservations.md
