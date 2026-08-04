---
id: adr-0002
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# ADR-0002: Second graph fixture

> **In the context of** the graph rules, **we decided** to keep this node fully valid, **rather than** breaking it, **to
> achieve** a clean anchor that the others point at, **accepting** nothing unusual here.

## Context

This is the clean node in the graph. ADR-0001 supersedes it but this document does not reciprocate, which is what makes
the reciprocity check fire on ADR-0001 — not here.

## Decision

Produce no findings of its own.

## Alternatives Considered

* **Reciprocate the supersession** — rejected: then ADR-0001 would have nothing to fail against.

## Consequences

This document stays finding-free.
