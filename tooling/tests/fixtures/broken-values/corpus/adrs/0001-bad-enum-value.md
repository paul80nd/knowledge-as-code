---
id: adr-0001
tier: decided
status: Draft
owner: alex.doe
---

# Bad enum value

`ADR: adr-0001` `DRAFT`

> **In the context of** the enum rules, **facing** two checks that travel together, **we decided** to use a status
> that is both out of range and capitalised, **rather than** one fault at a time, **to achieve** coverage of `enum`
> and `enum-lowercase`, **accepting** that this document is intentionally broken.

## Context

`status: Draft` is not one of the type's allowed values and is not lowercase, so both enum checks fire from the one
field. The `Contains` check is case-sensitive, so an uppercase value in range would trip `enum` too — the two checks
travel together for these enums.

## Decision

Trigger `enum` and `enum-lowercase`.

## Alternatives Considered

* **Use a valid status** — rejected: then there would be nothing to test here.

## Consequences

The golden pins both enum findings for this document.
