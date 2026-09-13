---
id: tol-mkdocs-material
type: tool
tier: descriptive
status: approved
versions: 9.7.x
licence: MIT
owner: human:paul.law
tags: [ documentation, theme ]
---

# Material for MkDocs

`Tool: tol-mkdocs-material` `APPROVED`

The theme the documentation site renders with. It supplies the navigation, search and colour-scheme behaviour
`mkdocs.yml` turns on.

## What we use it for

The `features:` and `palette:` blocks in `mkdocs.yml` are this theme's. They put each section's index page behind the
section name, and they add an edit pencil to every page. They also let the reader's own light or dark setting choose
the colour scheme, with a toggle either way.

It is pinned beside MkDocs in `docs/requirements.txt`. A theme release changes the site only when the pin moves.

## Status

**approved** since 2026-08-24.

## Accessibility

Assessed against [WCAG 2.2 AA] at 9.7.7, by building the site with the pinned version and reading the HTML it emits.
[std-A11Y] states the rules the assessment checks against.

The theme supplies what a Markdown author cannot. `<html lang="en">` states the language of every page. A "Skip to
content" link is first in the keyboard tab order. The header logo is an image with `alt="logo"`, inside a link
with `aria-label="knowledge-as-code"`, so the accessible name is the site's own. Contrast comes from the `default` and
`slate` schemes with `indigo` primary, which are the theme's own and unchanged. `docs/assets/extra.css` sets two
margins and nothing else.

Two things fall short. Nobody has run the site through a screen reader or a contrast checker, so the contrast of the
two schemes is taken from the theme and not measured here. A Mermaid diagram renders as an SVG that the theme gives no
text alternative, so the prose beside it is the only way to read what it says.

## Where it is used

* [svc-docs-site] is built with it.

## Alternatives considered

None recorded.

## Licence and obligations

MIT for the community edition, which is what `docs/requirements.txt` installs. The sponsor-only edition is a separate
package and nothing here uses it.

## Related

* [std-A11Y] requires the site it renders, and this entry, to meet [WCAG 2.2 AA].
* [tol-mkdocs] is the generator it themes.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[std-A11Y]: ../../standards/accessibility.md
[svc-docs-site]: ../../services/docs-site.md
[tol-mkdocs]: mkdocs.md
