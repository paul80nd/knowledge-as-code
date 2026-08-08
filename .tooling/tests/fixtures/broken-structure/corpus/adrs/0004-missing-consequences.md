---
id: adr-0009
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# Missing consequences and a mismatched number

`ADR: adr-0009` `ACCEPTED`

> **In the context of** the structure rules, **facing** one cluster of defects per document, **we decided** to
> mismatch the id against the filename and drop a required section, **rather than** splitting the two across separate
> files, **to achieve** coverage of `id-matches-filename` and `required-section`, **accepting** that this document is
> intentionally broken.

## Context

The id and the identity line both say 0009, but the filename says 0004 — the filename is the anchor, so the id is what
mismatches. The identity line agrees with the id it is checked against, so it stays quiet and the finding lands in one
place. The Consequences section is absent.

## Decision

Trigger `id-matches-filename` and `required-section`.

## Alternatives Considered

* **Match the filename** — rejected: then there would be nothing to test here.
