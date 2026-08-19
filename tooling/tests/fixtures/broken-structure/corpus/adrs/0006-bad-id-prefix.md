---
id: xyz-0006
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# Bad id prefix

`ADR: xyz-0006` `ACCEPTED`

> **In the context of** the id rules, **facing** a prefix check that returns before the others run, **we decided** to
> use the wrong prefix, **rather than** breaking the width as well, **to achieve** coverage of `id-prefix`,
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
