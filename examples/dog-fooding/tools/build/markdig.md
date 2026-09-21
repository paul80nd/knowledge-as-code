---
id: tol-markdig
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:nuget/Markdig, versions: 1.x }
homepage: https://github.com/xoofx/markdig
licence: BSD-2-Clause
licence-declared: BSD-2-Clause
decided-on: "2026-08-03"
review-by: "2027-08-03"
owner: human:paul.law
tags: [ markdown, parser ]
---

# Markdig

`Tool: tol-markdig` `APPROVED`

The Markdown reader `kac` uses for the body of a record: its headings, its links and the generated blocks it rewrites.

## What we use it for

`kac.core` references it. The checks that read a document body run on what it produces, so `h1`, `identity`,
`sections`, `link-resolves` and `undefined-label` all read the same parse.

## Where it is used

* [svc-kac] uses it.

## Alternatives considered

None. It reads CommonMark with the extensions a record uses, and the checks were written against what it produces.

## Licence and obligations

BSD-2-Clause. It asks that the copyright notice travel with source and binary redistributions. A NuGet package states
the licence expression in its own metadata, so publishing `kac` needs nothing further.

## Related

* [tol-yamldotnet] reads the other half of a record.

[svc-kac]: ../../services/kac.md
[tol-yamldotnet]: yamldotnet.md
