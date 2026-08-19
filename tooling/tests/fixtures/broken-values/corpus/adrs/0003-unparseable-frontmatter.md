---
id: "adr-0003
status: accepted
---

# Unparseable frontmatter

`ADR: adr-0003` `ACCEPTED`

> The frontmatter above has an unterminated quote, so it is not valid YAML. `frontmatter-parses` fires and the
> validator returns early, so no other check runs against this document.

## Context

This document exists to cover the `frontmatter-parses` check, which short-circuits all others.

## Decision

Trigger `frontmatter-parses` alone.

## Alternatives Considered

* **Close the quote** — rejected: then the frontmatter would parse and there would be nothing to test here.

## Consequences

The golden pins a single `frontmatter-parses` finding for this document.
