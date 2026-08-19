---
id: adr-7
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# Bad id width

`ADR: adr-7` `ACCEPTED`

> **In the context of** the id rules, **facing** a width check the prefix check would otherwise hide, **we decided**
> to use too few digits, **rather than** the wrong prefix, **to achieve** coverage of `id-format`, **accepting** that
> this document is intentionally broken.

## Context

The id is `adr-7` — the right prefix but only one digit where four are required.

## Decision

Trigger `id-format` and nothing else.

## Alternatives Considered

* **Pad the id to four digits** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `id-format` finding for this document.
