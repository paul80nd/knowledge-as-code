---
id: ctl-docs-build
type: control
tier: normative
status: active
verifies: [ eng:std-GATES.BUILD ]
mechanism: ci
frequency: per-pr
evidence: The `docs` job's log on the pull request, under the step "Build the site".
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ documentation, zensical ]
---

# The documentation site builds with no dead link

`Control: ctl-docs-build` `ACTIVE`

CI builds the site the way `publish-docs.yml` builds it, before it publishes.

## What it checks

* `eng:std-GATES.BUILD` says a push "**MUST** trigger a build and the test
  suite, without anyone asking for it".

## How it works

The `docs` job installs what `docs/requirements.txt` pins and runs `zensical build`. `mkdocs.yml` sets `strict: true`,
so a dead link or a dead anchor exits non-zero.

`NavigationTests` in `tooling/kac.tests` fails three faults the build lets through:

* a page under `docs/` that `mkdocs.yml` does not list, which the build passes without a word
* a nav listing the CLI reference pages in an order the parser does not declare, which the build says nothing about
* a redirect in `redirect_maps` whose target the nav does not list

`DocumentationTests` compares two tables on the site against the names in the code.

## Coverage and gaps

`strict: true` checks links inside the site. Nothing fetches an external URL, so a link to a page that has moved still
passes.

A reader judges whether a page is accurate. A published page describing behaviour the tool no longer has passes every
check here.
