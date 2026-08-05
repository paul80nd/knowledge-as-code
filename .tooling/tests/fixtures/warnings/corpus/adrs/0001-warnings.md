---
id: adr-0001
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# `adr-0001` Warning-level findings

> **In the context of** the warning rules, **we decided** to leave one alternative open and one link definition unused,
> **to achieve** coverage of `alternatives-verdict` and `unused-definition`, **accepting** that the document is
> otherwise valid so only warnings appear.

## Context

This document is valid except for two warning-level issues: an Alternatives Considered bullet that states no outcome,
and a link reference definition that nothing references.

## Decision

Trigger `alternatives-verdict` and `unused-definition`, both warnings, so the run stays at exit 0.

## Alternatives Considered

* **A message queue** — we might explore this in a future revision.

## Consequences

The golden pins the two warnings and no errors for this document.

[unused-ref]: /adrs/0001-warnings
