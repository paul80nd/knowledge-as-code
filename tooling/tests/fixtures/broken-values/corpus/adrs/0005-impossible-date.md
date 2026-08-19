---
id: adr-0005
tier: decided
status: accepted
decided-on: "2026-13-40"
owner: alex.doe
---

# A date the calendar does not have

`ADR: adr-0005` `ACCEPTED`

> **In the context of** the date rules, **facing** a value that is shaped like a date and is not one, **we decided** to
> write a thirteenth month, **rather than** a mis-shaped value, **to achieve** coverage of `date-format`'s calendar
> answer, **accepting** that this document is intentionally broken.

## Context

`2026-13-40` is quoted and is `YYYY-MM-DD` in shape, so every question about how it is written is satisfied. There is
no thirteenth month and no fortieth day, which only parsing it can find. The mis-shaped case is
`broken-frontmatter`'s, where an unquoted `2026/06/12` trips the shape branch and the quoting check together.

## Decision

Trigger `date-format` on the calendar, and nothing else.

## Alternatives Considered

* **Use 2025-02-29** — rejected: a leap year that is not one is the same finding by a longer route, and this value
  fails on two counts at once, so no single correction hides the other.

## Consequences

The golden pins one finding for this document.
