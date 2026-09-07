---
id: adr-0006
type: adr
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# An owner written without an actor prefix

`ADR: adr-0006` `ACCEPTED`

> **In the context of** the field-pattern rule, **facing** a scalar field rather than a list, **we decided** to write
> `owner` as a bare name, **rather than** as `human:alex.doe`, **to achieve** coverage of the actor prefix on a
> universal field, **accepting** that this document is intentionally broken.

## Context

`owner` is declared in `_universal.yaml` with `pattern: '^(human|role):[a-z0-9.-]+$'`. A bare name is what an author
writes by habit, so it is the value worth pinning. The finding lands on the field's own line, because a scalar has no
entry to point at.

## Decision

Trigger `field-pattern` once and nothing else.

## Alternatives Considered

* **Write `agent:alex.doe`**. Rejected, because the pattern refuses an unknown prefix for the same reason it refuses
  a bare name, and one fixture proving one thing is enough.

## Consequences

The golden pins one finding, on the `owner` line.
