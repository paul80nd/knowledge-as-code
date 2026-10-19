---
id: tol-checkout
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/actions/checkout, versions: 7.x }
homepage: https://github.com/actions/checkout
licence: MIT AND Apache-2.0 AND ISC
licence-declared: MIT
decided-on: "2026-08-03"
review-by: "2027-08-03"
owner: human:paul.law
tags: [ git, github-actions ]
---

# actions/checkout

`Tool: tol-checkout` `APPROVED`

The action that clones this repository onto the runner. Every job that reads a file here starts with it.

## What we use it for

`kac.yml` checks out the pull request, and each publishing workflow checks out the merge commit on `main`. The
`validate` job is the one job without it, because it only waits on the others.

The action configures the job's `github.token` as a git credential and removes it when the job ends. No step here sets
`persist-credentials: false`. The `publish` job of `publish-plugin.yml` declares `contents: write`, and its `git push`
to the marketplace branch authenticates with that credential. In every other job the credential goes unused, with
whatever permission the job declared.

Every step pins the action to the commit behind `v7.0.1`, and Dependabot moves the pin. `versions` approves the major
the pin sits in, so a move within 7.x changes nothing here and a move to 8 is a new decision.

## Where it is used

* [svc-corpus-feed] is packed and pushed from a checkout.
* [svc-docs-site] is built from one.
* [svc-kac] is built, tested and published from one.
* [svc-marketplace] is assembled and pushed from one.

## Alternatives considered

None recorded.

## Licence and obligations

The repository and its `package.json` declare MIT. The action runs `dist/index.js`, which bundles its npm
dependencies, and the `.licenses/` folder at the pinned commit records each of them. Most are MIT.
`before-after-hook` is Apache-2.0, and `semver` and `universal-user-agent` are ISC. The two entries marked `other` are
two versions of `@actions/http-client`, whose licence file is the MIT text under a GitHub copyright line.

The runner fetches the action for each job, and nothing here redistributes it, so nothing follows for anything this
repository publishes.

## Related

* [std-CI] requires every action to be pinned to a commit, and Dependabot to track `github-actions`.
* [tol-actionlint] checks the inputs each step passes to it.

[std-CI]: ../../standards/workflows.md
[svc-corpus-feed]: ../../services/corpus-feed.md
[svc-docs-site]: ../../services/docs-site.md
[svc-kac]: ../../services/kac.md
[svc-marketplace]: ../../services/marketplace.md
[tol-actionlint]: ../lint/actionlint.md
