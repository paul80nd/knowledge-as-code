---
id: adr-0002
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
tags: not-a-sequence
---

# ADR-0002: Scalar where a list is expected

> **In the context of** the list rules, **we decided** to give a list field a scalar value, **to achieve** coverage of
> `list`, **accepting** that this document is intentionally broken.

## Context

`tags` is declared as a list, but here it is a bare scalar rather than a YAML sequence.

## Decision

Trigger `list` and nothing else.

## Alternatives Considered

* **Use a sequence** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `list` finding for this document.
