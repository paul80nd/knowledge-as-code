---
id: adr-0011
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
---

# Headings with nothing under them

`ADR: adr-0011` `ACCEPTED`

> **In the context of** the structure rules, **facing** a heading that satisfies `required-section` by existing, **we
> decided** to leave one required and one optional section empty, **rather than** dropping either heading, **to
> achieve** coverage of both wordings of `empty-section`, **accepting** that this document is intentionally broken.

## Context

`required-section` asks whether the heading is there and cannot ask whether anything follows it. Consequences and
Related are both present and both blank, which is two findings under one check: the required one has to be written, the
optional one can be deleted instead.

## Decision

Trigger `empty-section` twice, once for each wording.

## Alternatives Considered

* **Empty only the required section** — rejected: the optional wording would then have no fixture, and the coverage
  gate reads ids rather than branches.

## Consequences

## Related
