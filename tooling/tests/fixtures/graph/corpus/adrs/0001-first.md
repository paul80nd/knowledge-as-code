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
  - adr-0099
---

# First graph fixture

`ADR: adr-0001` `ACCEPTED`

> **In the context of** the graph rules, **facing** checks that need more than one document, **we decided** to point
> at a missing target and a one-sided supersession, **rather than** keeping every edge sound, **to achieve** coverage
> of the link and reciprocity checks, **accepting** that this document is intentionally broken.

## Context

This fixture exercises the cross-document rules: a dead link, a link into a heading that is not there, an undefined
shortcut, a one-sided supersession, and a `related` field that both disagrees with its section and names an ADR nobody
wrote. `related` declares a `ref:` and no `reciprocal:`, so the unresolved id is what holds the two checks apart.

## Decision

It links to [a page that does not exist](nonexistent-target.md), which fails `link-resolves`, and cites [ADR-0099]
with no link definition, which fails `undefined-label`. It links to
[a heading nobody wrote](0002-second.md#renamed-away), where the file resolves and the fragment names nothing, which
fails `fragment-resolves`. It also leaves [an unlinked placeholder] in prose, which warns `bracket-literal`.

## Alternatives Considered

* **Leave the graph rules untested** — rejected: cross-document rules are the easiest to regress silently.

## Consequences

The golden pins the `link-resolves`, `fragment-resolves`, `undefined-label`, `ref-resolves`, `reciprocal` and
`related-matches-section` findings. The checklist below warns nothing: a checkbox is a marker, and the golden pins
that silence.

- [ ] a box left to tick
- [x] one already ticked

## Related

This section deliberately references no ADR, so it disagrees with the `related` field in the frontmatter above.
