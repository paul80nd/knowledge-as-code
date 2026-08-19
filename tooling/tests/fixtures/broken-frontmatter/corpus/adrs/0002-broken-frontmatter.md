---
id: adr-0002
status: accepted
tier: descriptive
priority: high
decided-on: 2026/06/12
deciders: null
---

# A deliberately broken frontmatter example

`ADR: adr-0002` `ACCEPTED`

> **In the context of** testing the frontmatter rules, **facing** the need for a document with known defects, **we
> decided** to keep the body valid and break only the frontmatter, **rather than** breaking everything at once, **to
> achieve** a golden that isolates the frontmatter checks, **accepting** that other scenarios cover the rest.

## Context

The body of this document is deliberately valid — correct H1, all required sections, a Y-statement and verdicts — so
that the only findings kac produces come from the frontmatter above.

## Decision

Break exactly one thing per frontmatter rule under test: an unknown key, a key-order violation, a missing required
field, a mismatched tier, an unquoted and mis-shaped date, and a non-bare absent value.

## Alternatives Considered

* **Break the body as well** — rejected: it would mix structural findings into a golden that is meant to isolate the
  frontmatter rules.
* **One defect per document** — rejected: it would multiply the number of fixture files without improving clarity.

## Consequences

The golden for this scenario lists exactly the frontmatter findings, so a change to any of those rules shows up here as
a diff.
