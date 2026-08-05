---
id: adr-0001
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# `adr-0001` First decision

> **In the context of** the index fixture, **we decided** to keep one accepted ADR, **to achieve** a populated index
> row with a decided-on date, **accepting** that it must stay valid.

## Context

An accepted ADR contributes a fully-populated index row.

## Decision

Serve as the first row of the generated index.

## Alternatives Considered

* **Leave the index empty** — rejected: then generation would have nothing to render.

## Consequences

The generated `INDEX.md` lists this ADR with its decided-on date.
