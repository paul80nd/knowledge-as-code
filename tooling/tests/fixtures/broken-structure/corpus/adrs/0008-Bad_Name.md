---
id: adr-0008
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# Bad filename

`ADR: adr-0008` `ACCEPTED`

> **In the context of** the filename rule, **facing** a slug short enough to keep `slug-length` quiet, **we decided**
> to use capitals and an underscore, **rather than** an over-long name, **to achieve** coverage of
> `filename-pattern`, **accepting** that this document is intentionally broken.

## Context

The filename `0008-Bad_Name.md` uses an uppercase letter and an underscore, neither allowed by the type's filename
pattern. The slug is short, so `slug-length` does not fire.

## Decision

Trigger `filename-pattern` and nothing else.

## Alternatives Considered

* **Rename to lowercase-hyphen** — rejected: then there would be nothing to test here.

## Consequences

The golden pins a single `filename-pattern` finding for this document.
