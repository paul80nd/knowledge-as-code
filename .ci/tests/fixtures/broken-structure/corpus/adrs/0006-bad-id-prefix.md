---
id: xyz-0006
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# ADR-0006: Bad id prefix

> **In the context of** the id rules, **we decided** to use the wrong prefix, **to achieve** coverage of `id-prefix`,
> **accepting** that this document is intentionally broken.

## Context

The id uses the `xyz-` prefix instead of `adr-`. The prefix check returns early, so no further id checks run — that is
why this document owns `id-prefix` alone.

## Decision

Trigger `id-prefix` and nothing else.

## Alternatives Considered

* **Use the right prefix** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `id-prefix` finding for this document.
