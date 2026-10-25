---
id: tol-mkdocs-material
type: tool
tier: descriptive
status: deprecated
packages:
  - { purl: pkg:pypi/mkdocs-material, versions: 9.7.x }
homepage: https://squidfunk.github.io/mkdocs-material/
licence: MIT AND Apache-2.0 AND CC-BY-4.0 AND CC0-1.0
licence-declared: MIT
decided-on: "2026-10-25"
review-by: "2027-08-24"
successor: tol-zensical
owner: human:paul.law
tags: [ documentation, theme ]
---

# Material for MkDocs

`Tool: tol-mkdocs-material` `DEPRECATED`

The theme the documentation site rendered with. It supplied the navigation, search and colour-scheme behaviour
`mkdocs.yml` turns on.

## What we use it for

The `features:` and `palette:` blocks in `mkdocs.yml` were written for this theme, and Zensical reads them unchanged.
They put each section's index page behind the
section name, and they add an edit pencil to every page. They also let the reader's own light or dark setting choose
the colour scheme, with a toggle either way.

It was pinned beside MkDocs in `docs/requirements.txt`, so a theme release changed the site only when the pin moved.

## Status

[tol-zensical] builds the site instead. Material for MkDocs reaches end of life on 2026-11-05, after which it gets no
maintenance except in exceptional circumstances. Zensical comes from the same team and reads the same `mkdocs.yml`.

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

## Alternatives considered

None recorded.

## Licence and obligations

The community edition is MIT, and that is what `docs/requirements.txt` installs. The wheel also ships four icon sets
under `material/templates/.icons/`, each with its own licence file: Material Design Icons from the Pictogrammers under
Apache-2.0, Font Awesome Free under CC-BY-4.0, GitHub's Octicons under MIT, and Simple Icons under CC0-1.0. Those sets
ship as SVG files and no fonts, so the font clauses in the Font Awesome and Pictogrammers licences do not apply.

An icon put on a page comes under its own set's terms, and CC-BY-4.0 asks for attribution. The sponsor-only edition is
a separate package and nothing here uses it.

## Related

* [std-A11Y] requires the site it renders, and this entry, to meet [WCAG 2.2 AA].
* [tol-mkdocs] is the generator it themes.
* [tol-zensical] replaces it.

[WCAG 2.2 AA]: https://www.w3.org/TR/WCAG22/
[std-A11Y]: ../../standards/accessibility.md
[tol-mkdocs]: mkdocs.md
[tol-zensical]: zensical.md
