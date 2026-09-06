---
id: tol-mkdocs-material
tier: descriptive
status: approved
versions: 9.7.x
licence: MIT
decided-in:
replaces:
successor:
owner: paul.law
tags: [ documentation, theme ]
---

# Material for MkDocs

`Tool: tol-mkdocs-material` `APPROVED`

The theme the documentation site renders with, and the source of the navigation, search and colour-scheme behaviour
`mkdocs.yml` turns on.

## What we use it for

The `features:` and `palette:` blocks in `mkdocs.yml` are this theme's. They put each section's index page behind the
section name, offer an edit pencil on every page, and let the reader's own light or dark setting decide, with a toggle
either way.

It is pinned beside MkDocs in `docs/requirements.txt`, so a theme release changes the site when we take it rather than
on its own.

## Status

**approved** since 2026-08-24.

## Where it is used

* [svc-docs-site] is built with it.

## Alternatives considered

None recorded.

## Licence and obligations

MIT for the community edition, which is what `docs/requirements.txt` installs. The sponsor-only edition is a separate
package and nothing here uses it.

## Related

* [tol-mkdocs] is the generator it themes.

[svc-docs-site]: ../../services/docs-site.md
[tol-mkdocs]: mkdocs.md
