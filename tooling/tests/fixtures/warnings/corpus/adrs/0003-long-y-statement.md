---
id: adr-0003
tier: decided
status: accepted
decided-on: "2026-06-14"
owner: alex.doe
---

# A Y-statement that runs past its ceiling

`ADR: adr-0003` `ACCEPTED`

> **In the context of** a Y-statement that has been allowed to grow well past the ceiling the schema sets for it,
> **facing** the temptation to explain the whole decision in the summary rather than in the sections written to hold
> it, **we decided** to keep writing until the block-quote is comfortably longer than the limit, **rather than**
> stopping at the point where a reader has what they need, **to achieve** a document that trips the word count,
> **accepting** that nobody would write this on purpose and that the warning is the whole reason it exists.

## Context

The `y-statement-present` rule reports three faults under one id, and the coverage gate reads ids rather than
branches. This document owns the ceiling: the block-quote states all six moves and runs past `max-words`.

## Decision

Run past the ceiling, so the branch that reads `max-words` is exercised and the number in the schema is pinned by
something other than the code that reads it.

## Alternatives Considered

* **Assert the ceiling in a unit test** — rejected, because it would pin the parsing without pinning that the
  parsed value reaches the check.

## Consequences

The golden pins the word count, so changing `max-words` in the schema fails here rather than silently.
