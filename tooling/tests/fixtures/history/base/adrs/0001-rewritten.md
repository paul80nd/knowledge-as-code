---
id: adr-0001
type: adr
tier: decided
status: accepted
decided-on: "2026-06-11"
owner: human:alex.doe
---

# A decision rewritten

`ADR: adr-0001` `ACCEPTED`

> **In the context of** the history fixture, **facing** a record compared with its state at a base revision, **we
> decided** to keep this decision fixed once settled, **rather than** editing it in place, **to achieve** a record a
> reader can rely on, **accepting** that a new decision takes a new record.

## Context

The fixture needs records that change between two revisions, so that each history check has something to judge.

## Decision

We keep every decision in a record, reviewed before it is accepted. The [handbook](https://example.com/handbook) says how.

## Alternatives Considered

* **Edit the record in place**: rejected, because a reader citing it would read a different decision.

## Consequences

A new decision takes a new record, and this one stays as it was settled.
