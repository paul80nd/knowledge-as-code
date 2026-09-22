---
id: adr-0007
type: adr
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: human:alex.doe
supersedes: adr-0006
---

# Seventh graph fixture

`ADR: adr-0007` `ACCEPTED`

> **In the context of** the graph rules, **facing** a supersede chain with no sound end, **we decided** to reciprocate
> ADR-0006, **rather than** pointing at it one way, **to achieve** a chain both checks agree on, **accepting** that
> this node reports nothing either.

## Context

The live end of the only sound supersede chain here. It closes the pair ADR-0006 opens.

## Decision

Name ADR-0006 in `supersedes`, which is what `reciprocal` reads from this end.

## Alternatives Considered

* **Leave the chain one-way**: rejected, because that is ADR-0001 and there is one already.

## Consequences

This document stays finding-free.
