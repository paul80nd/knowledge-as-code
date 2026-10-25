---
id: tol-mkdocs
type: tool
tier: descriptive
status: deprecated
packages:
  - { purl: pkg:pypi/mkdocs, versions: 1.6.x }
homepage: https://www.mkdocs.org/
licence: BSD-2-Clause
licence-declared: BSD-2-Clause
decided-on: "2026-10-25"
review-by: "2027-08-24"
owner: human:paul.law
tags: [ documentation, static-site ]
---

# MkDocs

`Tool: tol-mkdocs` `DEPRECATED`

The static site generator that built the documentation site from `docs/`, configured by `mkdocs.yml` at the
repository root.

## What we use it for

The site documents `kac`, and MkDocs was chosen because it is what tool documentation uses. The site is not a corpus, so
`publishing-target: mkdocs` in a corpus descriptor is a separate thing this build does not exercise.

Under MkDocs, `strict: true` in `mkdocs.yml` turned a dead link into a failed build. MkDocs reported a page the
navigation does not list at INFO only, so `NavigationTests` in `kac.tests` covers that case.

## Status

[tol-zensical] builds the site instead, from the same `mkdocs.yml`. The theme this generator rendered with reaches end
of life on 2026-11-05, and MkDocs 2.0 drops the plugin system. `mkdocs.yml` still builds the same site under 1.6.1, so
returning to this generator is a change to `docs/requirements.txt` and to the build command.

## Alternatives considered

None recorded.

## Licence and obligations

BSD-2-Clause. It asks that the copyright notice travel with a redistribution. The site is generated output and not a
redistribution of the generator, so nothing follows for what is published.

## Related

* [tol-mkdocs-material] is the theme it renders with.
* [tol-mkdocs-redirects] keeps the URL of a moved page working.
* [tol-zensical] replaces it.

[tol-mkdocs-material]: mkdocs-material.md
[tol-mkdocs-redirects]: mkdocs-redirects.md
[tol-zensical]: zensical.md
