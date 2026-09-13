---
id: tol-gh
type: tool
tier: descriptive
status: approved
licence: MIT
owner: human:paul.law
tags: [ cli, github, releases ]
---

# GitHub CLI

`Tool: tol-gh` `APPROVED`

The `gh` command. The publish workflow tags a release with it, and anyone working here reads and raises issues with
it.

## What we use it for

`publish-tool.yml` calls it twice in one step. The first call asks whether the release already exists. The second
creates the release, with the changelog section for that version as the body. Publishing creates the tag, so nobody
has a second step to remember.

It is also how issues are read and raised. `.claude/agents-config/issue-tracker.md` states that an agent uses `gh` for
issues on this repository.

## Status

**approved** since 2026-08-20.

## Where it is used

* [svc-kac] is tagged and released with it.

`versions` is bare because nothing pins it. The GitHub-hosted runner ships `gh` already installed, and a developer
installs whatever their package manager offers.

## Alternatives considered

* **The REST API over `curl`**: it needs a token handled by hand in every step. `gh` reads `GH_TOKEN` from the job,
  and the release step already passes `github.token` that way.

## Licence and obligations

MIT. Nothing follows for anything this repository publishes.

## Related

* [std-CI] states the rule that a workflow has no credential of its own.

[std-CI]: ../../standards/workflows.md
[svc-kac]: ../../services/kac.md
