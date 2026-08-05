---
id: adr-0009
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# `adr-0009` Missing consequences and a mismatched number

> **In the context of** the structure rules, **we decided** to mismatch the id and H1 against the filename and drop a
> required section, **to achieve** coverage of `id-matches-filename`, `h1-matches-id` and `required-section`,
> **accepting** that this document is intentionally broken.

## Context

The id and H1 both say 0009, but the filename says 0004 — the filename is the anchor, so both mismatch. The
Consequences section is absent.

## Decision

Trigger `id-matches-filename`, `h1-matches-id` and `required-section`.

## Alternatives Considered

* **Match the filename** — rejected: then there would be nothing to test here.
