---
id: tol-claude-code-cli
type: tool
tier: descriptive
status: approved
versions:
licence:
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

Not established. The package's own licence metadata has not been read, so this entry states none. The inherited clause
`eng:pol-TRUS.LICENCE` asks that a component's licence be screened before we adopt it, and that screening is the open
question here.

## Related

* [std-CI] holds the rule that a tool installed at a moving version runs in a job with no write permission.
* [std-CONFIG] requires a pin, and names the header comment as where an unpinnable one is explained.

[std-CI]: ../../standards/workflows.md
[std-CONFIG]: ../../standards/configuration.md
[svc-marketplace]: ../../services/marketplace.md
