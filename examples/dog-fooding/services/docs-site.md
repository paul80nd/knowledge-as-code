---
id: svc-docs-site
type: service
tier: descriptive
status: live
repo: knowledge-as-code
platform: static
criticality: important
owner: human:paul.law
facets: [ public ]
tags: [ github-pages, mkdocs ]
---

# Documentation site

`Service: svc-docs-site` `LIVE`

The public documentation for `kac` and for the framework around it. MkDocs builds it from `docs/`, and GitHub Pages
serves it.

## What it does

The site is where somebody who has installed nothing reads what this is and how to use it. It carries a page for each
`kac` verb, the framework pages, the corpus descriptor reference and the tool's changelog.

A page's content need not come from the file holding the page. `docs/changelog.md` is a snippet of
`tooling/kac/CHANGELOG.md`, so a release moves the site without any file under `docs/` changing. Taken from the header
comment in `.github/workflows/publish-docs.yml`.

`docs/` holds every page and nothing else does. The site is not a corpus, and `publishing-target: mkdocs` in a corpus
descriptor is a separate thing this build does not exercise. Taken from the header comment in `mkdocs.yml`.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), at `docs`, configured by
  `mkdocs.yml` at the root
* **Platform**: MkDocs with the Material theme, both pinned in `docs/requirements.txt`
* **Deployed as**: a static site uploaded as a Pages artifact by `publish-docs.yml`

## Environments

| Environment | URL                                             | Notes                                                                  |
|-------------|-------------------------------------------------|------------------------------------------------------------------------|
| Development | No published URL                                | `mkdocs serve` from a checkout.                                        |
| Test        | No published URL                                | The `docs` job in `kac.yml` runs `mkdocs build` on every pull request. |
| Production  | <https://paul80nd.github.io/knowledge-as-code/> | Published by `publish-docs.yml` on a push to `main`.                   |

## Dependencies

None in this catalogue. The site is static HTML and calls nothing.

The packages that build it are in the tool register: [tol-mkdocs] and [tol-mkdocs-material].

## Data

None of its own. Every page is built from a file in the repository.

## Operational notes

* **`strict: true` in `mkdocs.yml` turns a dead link into a failed build.** A page the navigation does not list is
  reported at INFO instead, which strict mode does not catch, so `NavigationTests` in `kac.tests` holds that half.
* **The Pages source is set to GitHub Actions in the repository settings**, so nothing pushes a branch. No file in this
  repository holds that setting.
* **The publish concurrency group cancels a superseded run.** There is one live site, so a newer push takes over from
  an older one rather than queueing behind it.

[tol-mkdocs]: ../tools/docs/mkdocs.md
[tol-mkdocs-material]: ../tools/docs/mkdocs-material.md
