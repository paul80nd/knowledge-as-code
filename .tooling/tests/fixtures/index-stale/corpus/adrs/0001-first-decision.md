---
id: adr-0001
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# First decision

`ADR: adr-0001` `ACCEPTED`

> **In the context of** the index fixture, **facing** a generator that renders one row per document, **we decided**
> to keep one accepted ADR, **rather than** only proposed ones, **to achieve** a populated index row with a
> decided-on date, **accepting** that it must stay valid.

## Context

An accepted ADR contributes a fully-populated index row.

## Decision

Serve as the first row of the generated index.

## Alternatives Considered

* **Leave the index empty** — rejected: then generation would have nothing to render.

## Consequences

The generated `INDEX.md` lists this ADR with its decided-on date.
