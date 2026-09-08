---
id: tol-claude-code-cli
type: tool
tier: descriptive
status: approved
versions:
licence: LicenseRef-Anthropic-Terms-of-Service
decided-in:
replaces:
successor:
owner: human:paul.law
tags: [ claude-code, plugin, validation ]
---

# Claude Code CLI

`Tool: tol-claude-code-cli` `APPROVED`

The `claude` command, installed from npm in CI to prove that each plugin bundle is valid against the CLI a reader
actually runs.

## What we use it for

`claude plugin validate --strict` runs against every assembled bundle and against the marketplace beside it. It is the
last thing between a broken bundle and the people who have it installed.

A clean run prints one line whatever it looked at, and a plugin holding no component passes it, so two shell guards in
the same step close that gap. Taken from the comments in `.github/workflows/kac.yml`.

`versions` is bare because CI installs `@latest` on purpose. The bundle has to be valid against the CLI a reader runs,
and nothing in `.github/dependabot.yml` would move an npm pin. That is why the install sits in a job holding no write
permission, where a moved tag cannot reach a push.

## Status

**approved** since 2026-08-18.

## Where it is used

* [svc-marketplace] is validated with it, in `kac.yml` and again in `publish-plugin.yml`.

## Alternatives considered

None. Nothing else reads a plugin manifest the way the CLI that loads it does.

## Licence and obligations

`LicenseRef-Anthropic-Terms-of-Service`. The package declares `SEE LICENSE IN README.md`, and the `LICENSE.md` beside
it reserves all rights to Anthropic PBC. That file grants use under the [legal agreements] Anthropic publishes. The
Commercial Terms of Service cover Team, Enterprise and API customers, and the Consumer Terms of Service cover Free, Pro
and Max ones.

The licence grants no right to copy, change or redistribute the package. CI installs the published binary with
`npm install -g` and runs it untouched. The CLI sits outside the working tree, and the plugin bundle it validates
carries no part of it.

Anthropic puts further conditions on preinstalling or running the CLI inside a product offered to other people. Nothing
here does that, so those conditions do not reach this repository.

## Related

* [std-CI] holds the rule that a tool installed at a moving version runs in a job with no write permission.
* [std-CONFIG] requires a pin, and names the header comment as where an unpinnable one is explained.

[legal agreements]: https://code.claude.com/docs/en/legal-and-compliance
[std-CI]: ../../standards/workflows.md
[std-CONFIG]: ../../standards/configuration.md
[svc-marketplace]: ../../services/marketplace.md
