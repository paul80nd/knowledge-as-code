---
id: adr-7
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# ADR-0007: Bad id width

> **In the context of** the id rules, **we decided** to use too few digits, **to achieve** coverage of `id-format`,
> **accepting** that this document is intentionally broken.

## Context

The id is `adr-7` — the right prefix but only one digit where four are required.

## Decision

Trigger `id-format` and nothing else.

## Alternatives Considered

* **Pad the id to four digits** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `id-format` finding for this document.
