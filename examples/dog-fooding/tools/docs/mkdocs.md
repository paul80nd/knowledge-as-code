---
id: tol-mkdocs
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:pypi/mkdocs, versions: 1.6.x }
homepage: https://www.mkdocs.org/
licence: BSD-2-Clause
licence-declared: BSD-2-Clause
decided-on: "2026-08-24"
review-by: "2027-08-24"
owner: human:paul.law
tags: [ documentation, static-site ]
---

# MkDocs

`Tool: tol-mkdocs` `APPROVED`

The static site generator that builds the documentation site from `docs/`, configured by `mkdocs.yml` at the
repository root.

## What we use it for

The site documents `kac`, and MkDocs is here because it is what tool documentation uses. The site is not a corpus, so
`publishing-target: mkdocs` in a corpus descriptor is a separate thing this build does not exercise.

`strict: true` in `mkdocs.yml` turns a dead link into a failed build. MkDocs reports a page the navigation does not
list at INFO only, so `NavigationTests` in `kac.tests` covers that case instead.

## Where it is used

* [svc-docs-site] is built with it.

## Alternatives considered

None recorded.

## Licence and obligations

BSD-2-Clause. It asks that the copyright notice travel with a redistribution. The site is generated output and not a
redistribution of the generator, so nothing follows for what is published.

## Related

* [tol-mkdocs-material] is the theme it renders with.

[svc-docs-site]: ../../services/docs-site.md
[tol-mkdocs-material]: mkdocs-material.md
