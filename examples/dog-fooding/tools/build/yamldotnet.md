---
id: tol-yamldotnet
type: tool
tier: descriptive
status: approved
versions: 18.x
licence: MIT
decided-in:
replaces:
successor:
owner: paul.law
tags: [ parser, yaml ]
---

# YamlDotNet

`Tool: tol-yamldotnet` `APPROVED`

The YAML reader `kac` uses for every file it parses: the schema under `.schema/`, each corpus descriptor, the overlay
manifest, and the frontmatter block at the top of a record.

## What we use it for

`kac.core` references it. Four kinds of file reach it, and a record's frontmatter is the one a reader meets: the
`frontmatter-parses` check is this parser reporting that the block is a valid YAML mapping.

## Status

**approved** since 2026-08-03.

## Where it is used

* [svc-kac] carries it.

## Alternatives considered

None. It arrived with the first parser and nothing has pushed against it.

## Licence and obligations

MIT. Nothing follows for a package this repository publishes.

## Related

* [tol-markdig] reads the other half of a record.

[svc-kac]: ../../services/kac.md
[tol-markdig]: markdig.md
