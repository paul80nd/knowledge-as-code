---
id: adr-0002
tier: decided
status: proposed
decided-on:
owner: alex.doe
---

# Second decision

`ADR: adr-0002` `PROPOSED`

> **In the context of** the index fixture, **we decided** to keep one proposed ADR, **to achieve** an index row with an
> empty decided-on cell, **accepting** that it must stay valid.

## Context

A proposed ADR has a bare `decided-on`, so its index cell is empty — exercising the empty-column path.

## Decision

Serve as the second row of the generated index.

## Alternatives Considered

* **Accept it** — rejected: then both rows would carry a date and the empty-cell path would go untested.

## Consequences

The generated `INDEX.md` lists this ADR with an empty decided-on cell.
