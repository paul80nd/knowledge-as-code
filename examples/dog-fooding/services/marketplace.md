---
id: svc-marketplace
tier: descriptive
status: live
repo: knowledge-as-code
platform: static
criticality: important
depends-on:
data-stores:
owner: paul.law
facets: [ public ]
tags: [ claude-code, plugin ]
---

# Plugin marketplace

`Service: svc-marketplace` `LIVE`

The Claude Code marketplace offering each worked corpus as an installable plugin. It is an orphan git branch called
`marketplace`, rebuilt whole on every push to `main`.

## What it does

The branch holds one directory per plugin, a `.claude-plugin/marketplace.json` offering all of them, and a generated
`README.md` telling a browser where the source is. `publish-plugin.yml` builds it by running `kac export` and
`kac bundle` in each corpus, then copying each bundle onto an emptied worktree.

`kac bundle` runs in one corpus and knows only that corpus's own, so the marketplace listing every plugin is assembled
by the workflow rather than by the tool. Taken from the header comment in `.github/workflows/publish-plugin.yml`.

A reader installs from it with `/plugin marketplace add paul80nd/knowledge-as-code@marketplace`.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), on the `marketplace` branch,
  built from `examples/*` and `template/.plugin`
* **Platform**: generated JSON and Markdown, with no runtime
* **Deployed as**: a git branch, fetched by the Claude Code CLI

## Environments

| Environment | URL                                                              | Notes                                                                                            |
|-------------|------------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| Development | No published URL                                                 | `kac bundle` writes a marketplace under a corpus's own `.dist/`, added by absolute path.         |
| Test        | No published URL                                                 | The `corpora` job in `kac.yml` runs `claude plugin validate` on each bundle and its marketplace. |
| Production  | <https://github.com/paul80nd/knowledge-as-code/tree/marketplace> | Published by `publish-plugin.yml` on a push to `main`.                                           |

## Dependencies

None in this catalogue. The branch is fetched by the Claude Code CLI and calls nothing.

[svc-kac] builds every file on it, which is a build-time dependency rather than a call, so it is not an edge here.

## Data

Each plugin carries a frozen export of its corpus under `corpus/`. The export is read-only where it is installed, so an
agent that finds a record wrong raises an issue rather than editing one.

## Operational notes

* **`criticality` is `important` rather than `supporting`.** A reader who cannot install a plugin sees the failure, so
  the impact is not internal only. It sits below [svc-kac], which builds the branch and is what a corpus runs.
* **The branch is orphaned and never merged back.** One job in `publish-plugin.yml` holds `contents: write`, and it
  empties the worktree and asserts that nothing is still tracked before staging. That assertion is what keeps the
  source out of the published branch.
* **A plugin reaches nobody until its `content-version` moves.** Claude Code offers an update only when a manifest's
  `version` string changes, so the publish job compares each built version against the branch and stops where every one
  matches.
* **The export is frozen at bundle time.** A branch editing a corpus leaves the installed plugin behind, so a lookup
  can answer with a record that branch has already changed.

[svc-kac]: kac.md
