---
id: adr-0002
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
tags:
  - identity
  - access-control
  - least-privilege
---

# ADR-0002: A list out of alphabetical order

> **In the context of** the `list-order` rule, **we decided** to leave `tags` in the order the author typed them,
> **to achieve** coverage of the warning, **accepting** that only the first pair out of order is reported.

## Context

`tags` is a set — nothing gives its sequence meaning — so alphabetical is the order that scan-reads.

## Decision

Trigger `list-order` once, pointing at the first entry that belongs earlier, and nothing else.

## Alternatives Considered

* **Report every pair out of order** — rejected; the rest are noise once the author re-sorts the field.

## Consequences

The golden pins one warning for this document.
