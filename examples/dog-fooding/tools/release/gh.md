---
id: tol-gh
type: tool
tier: descriptive
status: approved
versions:
licence: MIT
decided-in:
replaces:
successor:
owner: human:paul.law
tags: [ cli, github, releases ]
---

# GitHub CLI

`Tool: tol-gh` `APPROVED`

The `gh` command, used by the publish workflow to tag a release and by anyone working here to read and raise issues.

## What we use it for

`publish-tool.yml` calls it twice in one step: once to ask whether the release already exists, and once to create it
with the changelog section for that version as the body. The tag arrives as a consequence of publishing rather than as
a second thing to remember.

It is also how issues are reached. `.claude/agents-config/issue-tracker.md` names `gh` as the way an agent reads and
raises them on this repository.

## Status

**approved** since 2026-08-20.

## Where it is used

* [svc-kac] is tagged and released with it.

`versions` is bare because nothing pins it. The GitHub-hosted runner ships `gh` already installed, and a developer
takes whatever their package manager offers.

## Alternatives considered

* **The REST API over `curl`**: it needs a token handled by hand in every step, where `gh` reads `GH_TOKEN` from the
  job. The release step already passes `github.token` that way.

## Licence and obligations

MIT. Nothing follows for anything this repository publishes.

## Related

* [std-CI] holds the rule that a workflow carries no credential of its own.

[std-CI]: ../../standards/workflows.md
[svc-kac]: ../../services/kac.md
