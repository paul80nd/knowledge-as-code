---
id: adr-0002
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# ADR-0003: Third graph fixture

> **In the context of** the graph rules, **we decided** to reuse an id that already exists, **to achieve** coverage of
> `id-unique`, **accepting** that `id-matches-filename` fires alongside it.

## Context

This document reuses `adr-0002`, which ADR-0002 already owns, so `id-unique` fires here. Because the id number then
cannot match this file's own number (0003), `id-matches-filename` fires too — that pairing is unavoidable and
intentional.

## Decision

Trigger `id-unique` (and `id-matches-filename` with it).

## Alternatives Considered

* **Use a fresh id** — rejected: then there would be no duplicate to detect.

## Consequences

The golden pins both findings for this document.
