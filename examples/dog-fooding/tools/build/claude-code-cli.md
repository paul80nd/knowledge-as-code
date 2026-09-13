---
id: tol-claude-code-cli
type: tool
tier: descriptive
status: approved
licence: LicenseRef-Anthropic-Terms-of-Service
owner: human:paul.law
tags: [ claude-code, plugin, validation ]
---

# Claude Code CLI

`Tool: tol-claude-code-cli` `APPROVED`

The `claude` command, installed from npm in CI. It checks that each plugin bundle is valid against the CLI a reader
runs.

## What we use it for

`claude plugin validate --strict` runs against every assembled bundle and against the marketplace beside it. It is the
last check before a broken bundle gets to the people who have it installed.

A clean run prints one line whatever it read. A plugin with no component passes it too, so two shell guards in the
same step cover that gap. Taken from the comments in `.github/workflows/kac.yml`.

`versions` is bare because CI installs `@latest` on purpose. The bundle has to be valid against the CLI a reader runs,
and `.github/dependabot.yml` would not move an npm pin. The install therefore runs in a job with no write permission,
so a moved tag cannot cause a push.

## Status

**approved** since 2026-08-18.

## Where it is used

* [svc-marketplace] is validated with it, in `kac.yml` and again in `publish-plugin.yml`.

## Alternatives considered

None. Nothing else reads a plugin manifest the way the CLI that loads it does.

## Licence and obligations

`LicenseRef-Anthropic-Terms-of-Service`. The package declares `SEE LICENSE IN README.md`, and that README has no
licence section. The `LICENSE.md` shipped in the same package reserves all rights to Anthropic PBC. It grants use
under the [legal agreements](https://code.claude.com/docs/en/legal-and-compliance) Anthropic publishes. The Commercial
Terms of Service cover Team, Enterprise and API customers, and the Consumer Terms of Service cover Free, Pro and Max
ones.

The licence grants no right to copy, change or redistribute the package. CI installs the published binary with
`npm install -g` and runs it untouched. The CLI is installed outside the working tree, and the plugin bundle it
validates includes no part of it.

Anthropic puts further conditions on preinstalling or running the CLI inside a product offered to other people.
Nothing here does that, so those conditions do not apply to this repository.

## Related

* [std-CI] states the rule that a tool installed at a moving version runs in a job with no write permission.
* [std-CONFIG] requires a pin, and puts the explanation for a dependency that cannot be pinned in the header comment.

[std-CI]: ../../standards/workflows.md
[std-CONFIG]: ../../standards/configuration.md
[svc-marketplace]: ../../services/marketplace.md
