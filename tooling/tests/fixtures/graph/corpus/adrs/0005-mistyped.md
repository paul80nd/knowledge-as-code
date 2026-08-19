---
id: adr-0005
tier: decided
status: superseded
owner: alex.doe
superseded-by: giz-mirrored
---

# Fifth graph fixture

`ADR: adr-0005` `SUPERSEDED`

> **In the context of** the graph rules, **facing** a reference that resolves to the wrong type, **we decided** to
> supersede this ADR with a gizmo, **rather than** with an ADR nobody wrote, **to achieve** coverage of the half of
> `ref-resolves` that existence cannot answer, **accepting** that no estate would write this on purpose.

## Context

`giz-mirrored` is a document, so the target exists and a reader following it lands on a real page. Only the `ref:`
declaration says what it should have been.

## Decision

Point `superseded-by` at a gizmo.

## Alternatives Considered

* **Point at adr-0099** — rejected: an absent target is the dangling case, and ADR-0004 already carries it.

## Consequences

One finding, `ref-resolves`, naming both types. The reciprocity check has nothing to ask of a gizmo, which carries no
`supersedes` field to answer with, so it says nothing and the golden pins that silence.
