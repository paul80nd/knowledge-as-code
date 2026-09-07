---
id: dev-no-screen-reader-pass
tier: normative
status: active
departs-from:
  - eng:pol-A11Y.ASSIST
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-docs-site
  - svc-kac
owner: paul.law
tags: [ accessibility, documentation, screen-reader ]
---

# Nobody has read either surface with a screen reader

`Deviation: dev-no-screen-reader-pass` `ACTIVE`

The documentation site and the `kac` command line are reviewed by eye. No assistive technology has been near either.

## What we are doing instead

[std-A11Y] holds a Markdown author to heading order, link text, table structure and image alternatives, and a reviewer
checks those in the pull request. Material for MkDocs owns contrast, focus order and keyboard reach, and
[tol-mkdocs-material] records what its assessment found. `kac` repeats in words whatever it says in colour.

Every one of those is a rule somebody reads the diff against. None of them is a run with the software a blind reader
would use.

## Why we need it

`eng:pol-A11Y.ASSIST` asks for a test with the assistive technology people actually use. This repository has one
maintainer, who has neither a screen reader configured nor the practice to read a page with one. A pass run badly is
worse than no pass, because it reports a result somebody would believe.

## What compensates

* [std-A11Y] reaches the structure a screen reader depends on, which is most of what a content author can get wrong.
* The site is Markdown rendered by a widely used theme, so a fault in the chrome would be a fault thousands of sites
  carry.
* [tol-mkdocs-material] and [tol-spectre-console] each name what their assessment found short, so a known shortfall is
  written down rather than assumed away.

## How it closes

Somebody reads the site's navigation, one command page and one framework page with NVDA or VoiceOver, and runs `kac
validate` on a corpus with the same reader open. What that finds becomes issues, and this record closes on the day they
are raised.

Where nobody can run that pass by the review date, the honest close is to publish the accessibility statement
[std-A11Y] asks for and say in it that neither surface has been tested.

## Scope

Both surfaces [std-A11Y] governs: the documentation site MkDocs builds from `docs/`, and the `kac` command line.

## Related

* `eng:pol-A11Y.ASSIST` is the clause this departs from.
* [std-A11Y] is the standard that reaches everything else in that policy.

[std-A11Y]: ../standards/accessibility.md
[tol-mkdocs-material]: ../tools/docs/mkdocs-material.md
[tol-spectre-console]: ../tools/build/spectre-console.md
