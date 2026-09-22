---
id: adr-0006
type: adr
tier: decided
status: superseded
owner: human:alex.doe
superseded-by: adr-0007
---

# Sixth graph fixture

`ADR: adr-0006` `SUPERSEDED`

> **In the context of** the graph rules, **facing** a supersede chain every other fixture breaks, **we decided** to
> name the ADR that replaced this one, **rather than** leaving the pair to the fault cases, **to achieve** one
> reciprocated chain, **accepting** a second clean node.

## Context

The withdrawn end of the only sound supersede chain here. ADR-0004 and ADR-0005 are superseded by documents the corpus
cannot resolve, so the pass that reports nothing has nothing to run on.

## Decision

Name ADR-0007, which reciprocates.

## Alternatives Considered

* **Reuse ADR-0001 and ADR-0002**: rejected, because their broken reciprocity is what the check fires on.

## Consequences

`superseded-by`, `reciprocal` and `required-when: status == superseded` each run to a pass.
