---
id: adr-0001
tier: decided
status: accepted
decided-on: "2026-06-12"
owner: alex.doe
supersedes:
  - adr-0002
related:
  - adr-0002
---

# First graph fixture

`ADR: adr-0001` `ACCEPTED`

> **In the context of** the graph rules, **we decided** to point at a missing target and a one-sided supersession, **to
> achieve** coverage of the link and reciprocity checks, **accepting** that this document is intentionally broken.

## Context

This fixture exercises the cross-document rules: a dead link, an undefined shortcut, a one-sided supersession, and a
`related` field that disagrees with its section.

## Decision

It links to [a page that does not exist](nonexistent-target.md), which fails `link-resolves`, and cites [ADR-0099]
with no link definition, which fails `undefined-label`. It also leaves [an unlinked placeholder] in prose, which
warns `bracket-literal`.

## Alternatives Considered

* **Leave the graph rules untested** — rejected: cross-document rules are the easiest to regress silently.

## Consequences

The golden pins the `link-resolves`, `undefined-label`, `reciprocal` and `related-matches-section` findings.

## Related

This section deliberately references no ADR, so it disagrees with the `related` field in the frontmatter above.
