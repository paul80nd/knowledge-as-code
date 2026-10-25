---
id: tol-mkdocs-redirects
type: tool
tier: descriptive
status: deprecated
packages:
  - { purl: pkg:pypi/mkdocs-redirects, versions: 1.2.2 }
homepage: https://github.com/mkdocs/mkdocs-redirects
licence: MIT
licence-declared: MIT
decided-on: "2026-10-25"
review-by: "2027-10-13"
owner: human:paul.law
tags: [ documentation, redirects ]
---

# mkdocs-redirects

`Tool: tol-mkdocs-redirects` `DEPRECATED`

The MkDocs plugin that writes a page at a moved page's old URL, sending the reader on to the page that replaced it.

## What we use it for

Files under `template/` and `.schema/` cite the documentation site by URL, and every corpus that took the framework
keeps a copy of them. A page that moves would break each of those links in every such corpus, and no check here reads
an external URL. The plugin kept each old URL working.

`redirect_maps` in `mkdocs.yml` lists each moved page and the page that replaced it. The page it wrote kept the
anchor. `NavigationTests` in `kac.tests` fails a redirect whose target the nav does not list.

## Status

[tol-zensical] builds the redirect pages itself, from the same `redirect_maps` key, and keeps the anchor the same way.
The approval stopped at 1.2.2, because the ProperDocs project publishes every release from 1.2.3, and that release
requires `properdocs`, a fork of MkDocs.

## Alternatives considered

None recorded.

## Licence and obligations

MIT. It asks that the copyright notice travel with a copy of the software. The site is generated output and not a copy
of the plugin, so nothing follows for what is published.

## Related

* [tol-mkdocs] is the generator it plugs into.
* [tol-zensical] replaces it.

[tol-mkdocs]: mkdocs.md
[tol-zensical]: zensical.md
