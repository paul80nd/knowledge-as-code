---
id: adr-0002
type: adr
tier: decided
status: accepted
decided-on: "2026-09-02"
owner: human:mira.okonjo
deciders: [ human:alex.doe, human:mira.okonjo, human:robin.hale ]
tags: [ holds, reservations, state ]
---

# Reservations keeps no state of its own

`ADR: adr-0002` `ACCEPTED`

> **In the context of** the reserve-and-pay flow, **facing** where a half-finished reservation should live, **we
> decided** that Reservations keeps no state and writes only the provisional hold at confirm, **rather than** a draft
> store or an early hold, **to achieve** one owner for every hold, **accepting** that a borrower who leaves part-way
> starts again.

## Context

The reserve-and-pay flow takes a borrower through several pages before a hold exists, and it runs on the shared branch
terminals as well as on a borrower's own device. Something has to decide where a reservation lives between the first
page and the confirmed hold. The answer reaches two repositories: `platform`, where [svc-reservations] hosts the flow,
and `lending`, where [svc-lending] owns every hold in the legacy database.

Three facts shape the response.

* Every hold already lives in the legacy library management system, and branch staff see and act on it there.
* [itg-card-payments] returns the borrower against a hold reference, so the hold has to exist before the payment.
* Borrower data lives in the legacy database. A second store of it needs its own protection, backup and retention.

## Decision

Reservations keeps no state of its own. Wizard state before confirmation is ephemeral, and passes from page to page
through the flow. The only durable write is the provisional hold the service creates through the lending API when the
borrower confirms, and that hold's reference is what a card payment returns against.

Resuming a half-finished reservation in a later session is deliberately not supported.

## Alternatives Considered

* **A draft store: a table or a storage account holding half-finished reservations against the borrower.** It would let
  a borrower leave part-way and resume later, on another device. Rejected: it keeps borrower data outside the legacy
  database, in a store that needs its own backup, its own retention, and a clean-up of drafts nobody returns to.
* **Create the provisional hold on the first page, and let it serve as the draft.** It needs no new store. Rejected:
  branch staff see every provisional hold, so each abandoned reservation would become a hold somebody has to clear.

## Consequences

* **A borrower who leaves part-way starts again.** Nothing is kept for a later session, on any device.
* **The service runs no database, no storage account and no connection string.** There is nothing of its own to back
  up, migrate or hold to a retention period.
* **The lending API is the one owner of every hold.** A change to what a hold records is a change in `lending`, never a
  second copy in `platform`.
* **A payment taken after the borrower closes the browser leaves its hold provisional.** The flow keeps nothing that
  could finish the hold once the borrower has gone, and nothing reconciles the payment with the hold.

[itg-card-payments]: ../integrations/card-payments.md
[svc-lending]: ../services/lending.md
[svc-reservations]: ../services/reservations.md
