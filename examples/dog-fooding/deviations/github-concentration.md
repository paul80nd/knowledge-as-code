---
id: dev-github-concentration
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-TRUS.CLOUD
  - eng:pol-TRUS.EXIT
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-docs-site
  - svc-kac
  - svc-marketplace
owner: human:paul.law
tags: [ exit, github, supplier ]
---

# Everything here runs on GitHub, and nothing says how it would leave

`Deviation: dev-github-concentration` `ACTIVE`

GitHub stores the source, the issues, the workflows, the pages site and the marketplace branch. No record divides the
responsibility, and none says what leaving would take.

## What we are doing instead

The repository, the runners, the package registry approval, the published site and the plugin's marketplace branch are
all GitHub's. nuget.org hosts the published tool, and that is the one thing outside.

Nothing states which failures are GitHub's to handle and which are this repository's. Nothing has been written about
moving elsewhere.

## Why we need it

Every corpus here is Markdown in git and the tool is a .NET project, so the content and the code move to any git host
by pushing them. What does not move is the workflow syntax, the pages publish and the plugin marketplace. Writing down
that third of the problem is all the work there is, and nobody has needed it.

Nothing here makes money or stores anybody's data. On the day GitHub stops, the cost is a rebuild.

## What compensates

* Everything is in git, so any clone is a complete copy of the content and the code.
* The MIT licence lets anybody continue the work from that clone.
* Published versions of `kac` stay on nuget.org, which is not GitHub.
* [std-CI] keeps the CI logic in workflow files in the repository, so what would need rewriting is visible.

## How it closes

A record says what this repository depends on GitHub for, which of those has an alternative, and what a move would
cost. That is a page of work, and this record closes on it.

## Scope

The repository, its issues, its workflows, the documentation site and the marketplace branch.

## Related

* [std-CI] keeps the CI logic in files that would move with the repository.

[std-CI]: ../standards/workflows.md
