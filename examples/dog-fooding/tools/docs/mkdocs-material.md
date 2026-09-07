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

## Accessibility

Assessed against [WCAG 2.2 AA] at 9.7.7, by building the site with the pinned version and reading the HTML it emits.
[std-A11Y] carries the rules the assessment answers.

The theme supplies what a Markdown author cannot. `<html lang="en">` names the language of every page. A "Skip to
content" link is the first thing the keyboard reaches. The header logo is an image with `alt="logo"`, inside a link
carrying `aria-label="knowledge-as-code"`, so the accessible name is the site's own. Contrast comes from the `default`
and `slate` schemes with `indigo` primary, which are the theme's own and unmodified. `docs/assets/extra.css` sets two
margins and nothing else.

Two things fall short. Nobody has run the site through a screen reader or a contrast checker, so the contrast of the
two schemes is taken from the theme rather than measured here. A Mermaid diagram renders as an SVG the theme gives no
text alternative, which leaves the prose beside it as the only route to what it says.

## Where it is used

* [svc-docs-site] is built with it.

## Alternatives considered

None recorded.

## Licence and obligations

MIT for the community edition, which is what `docs/requirements.txt` installs. The sponsor-only edition is a separate
package and nothing here uses it.

## Related

* [std-A11Y] holds the site it renders, and this entry, to [WCAG 2.2 AA].
* [tol-mkdocs] is the generator it themes.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[std-A11Y]: ../../standards/accessibility.md
[svc-docs-site]: ../../services/docs-site.md
[tol-mkdocs]: mkdocs.md
