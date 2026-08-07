---
id: adr-0004
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
tags:
  - fine-tag
  - Not Lowercase
  - trailing_underscore
---

# List entries that break the field pattern

`ADR: adr-0004` `ACCEPTED`

> **In the context of** the field-pattern rule, **we decided** to give `tags` entries that break the declared regex,
> **to achieve** coverage of `field-pattern` on a list, **accepting** that this document is intentionally broken.

## Context

`tags` is declared in `_universal.yaml` with `pattern: '^[a-z0-9-]+$'`. The pattern applies to each entry, not to the
list as a whole, so a sequence carrying one good entry and two bad ones fires once per bad entry and stays silent on
`fine-tag`. The entries are in alphabetical order so that `list-order` stays out of the way — this fixture is about
the pattern alone.

## Decision

Trigger `field-pattern` twice and nothing else.

## Alternatives Considered

* **Use conforming tags** — rejected: then there would be nothing to test here.

## Consequences

The golden pins one finding per offending entry, each on the entry's own line.
