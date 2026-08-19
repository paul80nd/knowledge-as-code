---
id: adr-0004
tier: decided
status: superseded
owner: alex.doe
superseded-by: adr-0099
---

# Fourth graph fixture

`ADR: adr-0004` `SUPERSEDED`

> **In the context of** the graph rules, **facing** a reciprocal field whose target is absent, **we decided** to name
> an ADR nobody wrote, **rather than** pointing at one of the others, **to achieve** coverage of the layering,
> **accepting** that this document can never be reciprocated.

## Context

`superseded-by` declares both a `ref:` and a `reciprocal:`, so it is the field where the two checks could report the
same fault twice.

## Decision

Point at `adr-0099`, which does not exist.

## Alternatives Considered

* **Point at adr-0002** — rejected: a target that exists is the reciprocity case, and ADR-0001 already carries it.

## Consequences

One finding, `ref-resolves`. The reciprocity check has nothing to ask of a document that is not there, so it says
nothing, and the golden pins that silence.
