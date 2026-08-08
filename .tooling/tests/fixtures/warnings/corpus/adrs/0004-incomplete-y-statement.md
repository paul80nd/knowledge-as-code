---
id: adr-0004
tier: decided
status: accepted
decided-on: "2026-06-14"
owner: alex.doe
---

# A Y-statement that states four of the six moves

`ADR: adr-0004` `ACCEPTED`

> **In the context of** a block-quote that looks like a Y-statement, **we decided** to drop the concern and the
> contrast, **to achieve** coverage of the moves branch, **accepting** that what is left is a summary.

## Context

A Y-statement is six moves. Drop **facing** and the reader never learns what forced the decision; drop **rather
than** and the rejected options go unnamed. Both are the moves an author skips first, which is why they are the two
missing here.

## Decision

Trigger `y-statement` once, naming both absent moves, and nothing else. The block-quote stays well under
`max-words`, so the ceiling branch belongs to ADR-0003 alone.

## Alternatives Considered

* **Drop a single move** — rejected; two missing moves pin the wording that joins them into one message.

## Consequences

The golden pins the names of the missing moves, so reordering or rewording the six is visible here.
