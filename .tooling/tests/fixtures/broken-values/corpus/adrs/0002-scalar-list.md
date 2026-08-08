---
id: adr-0002
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
tags: not-a-sequence
---

# Scalar where a list is expected

`ADR: adr-0002` `ACCEPTED`

> **In the context of** the list rules, **facing** a field the schema declares as a sequence, **we decided** to give
> it a scalar value, **rather than** an ill-formed sequence, **to achieve** coverage of `list`, **accepting** that
> this document is intentionally broken.

## Context

`tags` is declared as a list, but here it is a bare scalar rather than a YAML sequence.

## Decision

Trigger `list` and nothing else.

## Alternatives Considered

* **Use a sequence** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `list` finding for this document.
