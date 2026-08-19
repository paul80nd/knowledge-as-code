---
id: adr-0003
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# An over-long filename slug

`ADR: adr-0003` `ACCEPTED`

> **In the context of** the structure rules, **facing** a limit that has to be measured on its own, **we decided** to
> over-long the slug, **rather than** breaking anything else here, **to achieve** coverage of `slug-length`,
> **accepting** that this document is intentionally broken.

## Context

The filename slug runs past the 30-character limit. Everything else about the document is well-formed, so the slug is
measured on its own rather than alongside a second defect.

## Decision

Trigger exactly `slug-length`, and nothing else.

## Alternatives Considered

* **Break more in one file** — rejected: it would blur which document owns which finding.

## Consequences

The golden pins these two findings for this document.
