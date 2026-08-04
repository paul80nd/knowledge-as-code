---
id: adr-0003
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# A title with no ADR prefix

> **In the context of** the structure rules, **we decided** to over-long the slug and mis-shape the H1, **to achieve**
> coverage of `slug-length` and `h1-pattern`, **accepting** that this document is intentionally broken.

## Context

The filename slug runs past the 30-character limit and the H1 does not match the type's pattern.

## Decision

Trigger exactly `slug-length` and `h1-pattern`, and nothing else.

## Alternatives Considered

* **Break more in one file** — rejected: it would blur which document owns which finding.

## Consequences

The golden pins these two findings for this document.
