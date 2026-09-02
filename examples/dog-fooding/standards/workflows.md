---
id: std-CI
tier: normative
status: active
implements: [ eng:pol-ACCS.DUTIES, eng:pol-ACCS.LEAST, eng:pol-AUTV.BLOCK, eng:pol-AUTV.INTEG, eng:pol-EVER.BRANCH,
  eng:pol-PIPE.ASCODE, eng:pol-PIPE.DEPLOY, eng:pol-PIPE.GATES, eng:pol-PIPE.LOCAL, eng:pol-PIPE.MANUAL,
  eng:pol-PIPE.REVERT, eng:pol-PIPE.TRACE, eng:pol-SCRT.EMBED, eng:pol-SCRT.LOGS, eng:pol-SCRT.ROTATE,
  eng:pol-SCRT.STORE, eng:pol-TRUS.MUTATE, eng:pol-TRUS.REPO, eng:pol-TRUS.SOURCE, eng:pol-TRUS.UNTRUST ]
applies-to:
  - all
review-by: "2027-09-02"
owner: paul.law
tags: [ continuous-integration, github-actions, publishing ]
---

# Every check and every publish runs from a workflow in this repository

`Standard: std-CI` `ACTIVE`

## Summary

One workflow gates a pull request. The others publish what landed: the tool to nuget.org, each corpus to GitHub
Packages, the plugins to the `marketplace` branch, and the site to GitHub Pages. A job says what it may do before it
does it, pins the actions it runs, and holds no credential of its own. Those three are [OpenSSF Scorecard]'s
`Token-Permissions`, `Pinned-Dependencies` and `Dangerous-Workflow` checks, and [actionlint] holds the rest of how a
workflow is written.

## Rules

### The gate runs on every pull request into main

- A check that can block a merge **MUST** run in `.github/workflows/kac.yml` on a pull request into `main`.
- `kac.yml` **MUST** name every one of its jobs in the `needs:` of its `validate` job.
- The branch rule on `main` **MUST** name `validate` as the check a merge waits for.
- A job **MUST NOT** declare `continue-on-error`.
- A workflow **MUST** pass `actionlint`, which also puts every `run:` block through shellcheck.
- A workflow file **MUST** arrive on `main` by the same reviewed merge as any other change.
- `.azuredevops/kac.yml` **MUST** run the same steps, in the same order, as `.github/workflows/kac.yml`.
- `.azuredevops/kac.yml` **MUST** name in its own header comment each step it leaves out, and why.

_**Covers:** `eng:pol-AUTV.BLOCK`, `eng:pol-AUTV.INTEG`, `eng:pol-EVER.BRANCH`, `eng:pol-PIPE.ASCODE`,
`eng:pol-PIPE.GATES`_

### A job declares the permission it needs

- A workflow **MUST** declare `permissions: contents: read` at its top level, which is what [OpenSSF Scorecard]'s
  `Token-Permissions` check asks for.
- A job needing wider access **MUST** declare that permission on itself.
- A job **MUST NOT** hold a permission no step in it uses.
- A workflow **MUST NOT** commit to a branch a person edits.

_**Covers:** `eng:pol-ACCS.LEAST`_

### Every action is pinned to a commit

- A step in `.github/workflows/` **MUST** pin its action to a commit SHA, which is what [OpenSSF Scorecard]'s
  `Pinned-Dependencies` check asks for.
- A pinned step **MUST** carry the released version in a trailing comment, as `# v7.0.1`.
- `.github/dependabot.yml` **MUST** track `github-actions`.
- A job holding a write permission **MUST NOT** install a tool at a moving version.
- A workflow needing a tool at `latest` **MUST** install it in a job holding `contents: read`.

_**Covers:** `eng:pol-TRUS.SOURCE`, `eng:pol-TRUS.UNTRUST`_

### A workflow holds no credential of its own

- A workflow **MUST** take every credential from GitHub's secret store or from an identity it exchanged.
- A publish to nuget.org **MUST** authenticate through a trusted publishing policy naming this repository, this
  workflow file and the `nuget.org` environment.
- A job **MUST** exchange its own identity for a short-lived key in the step before the one that spends it.
- A workflow calling GitHub's own API **MUST** take `github.token`.
- A step **MUST** pass a secret to a script through `env:`.
- A step **MUST NOT** interpolate a secret into a script body.
- A step **MUST NOT** print a secret.

_**Covers:** `eng:pol-SCRT.EMBED`, `eng:pol-SCRT.LOGS`, `eng:pol-SCRT.ROTATE`, `eng:pol-SCRT.STORE`_

### One job holds the write permission

- A publishing workflow **MUST** hold its write permission on one job.
- A workflow publishing a package or a branch **MUST** declare `needs:` on a job holding `contents: read`.
- A workflow publishing a package or a branch **MUST** run its checks again against the merge commit.
- The publishing job **MUST** build what it publishes from that commit.
- The publishing job **MUST** declare `timeout-minutes`.
- A workflow publishing a version **MUST** declare a `concurrency` group that queues rather than cancels.
- A workflow writing to a branch **MUST** empty its worktree, and **MUST** prove `git ls-files` returns nothing
  there before it stages a file.
- A person **MUST NOT** edit a published branch, package or release by hand.

_**Covers:** `eng:pol-PIPE.DEPLOY`, `eng:pol-PIPE.MANUAL`_

### A version moves by hand and publishes once

- `<Version>` in `tooling/kac/kac.csproj` **MUST** move in the pull request carrying the change it ships.
- `content-version` in a corpus's `.corpus.yaml` **MUST** move in the pull request changing what that corpus knows.
- A publishing job **MUST** ask the registry whether it already holds the version in front of it.
- Where the registry holds that version already, a publishing job **MUST** finish green.
- A publish to nuget.org **MUST** wait for a person to approve the `nuget.org` environment.
- A publish to nuget.org **MUST** tag the commit it published from.
- The release notes **MUST** be that version's section of `tooling/kac/CHANGELOG.md`.
- A published version **MUST NOT** be pushed again, replaced or deleted.
- A correction **MUST** ship as a new version.
- A person **MUST NOT** push a package from their own machine.

_**Covers:** `eng:pol-ACCS.DUTIES`, `eng:pol-PIPE.LOCAL`, `eng:pol-PIPE.REVERT`, `eng:pol-PIPE.TRACE`,
`eng:pol-TRUS.MUTATE`, `eng:pol-TRUS.REPO`_

## Examples

```
✅ Good
permissions:
  contents: read

jobs:
  publish:
    needs: verify
    timeout-minutes: 20
    permissions:
      contents: write
      id-token: write
    environment:
      name: nuget.org
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1

❌ Avoid
permissions: write-all

jobs:
  publish:
    steps:
      - uses: actions/checkout@v7
```

`write-all` hands every scope to every job in the file, including the ones that only read. A job with no `needs:`
publishes whatever the checkout holds, proved by nothing. A job with no `timeout-minutes` holds its concurrency group
for six hours when a push hangs. A tag moves, so `@v7` is a different action tomorrow.

## Conformance checklist

- [ ] `actionlint` passes.
- [ ] Every job in `kac.yml` is named in `validate`'s `needs:`.
- [ ] Every `uses:` names a commit SHA and carries its released version in a trailing comment.
- [ ] Every workflow declares `permissions: contents: read` at its top level.
- [ ] Every job holding a write permission has a step that uses it.
- [ ] No secret appears in a workflow's text, and no step prints one.
- [ ] Every publishing job declares `timeout-minutes`, and every package publish sits behind a verifying job.
- [ ] The version this pull request ships has moved, and the changelog carries its section.

## Rationale and provenance

Read-only permission is what keeps CI out of the files a person edits. `generate --check` reports a stale generated
file and names the command to run locally, so no job needs to write one back.

`WorkflowGateTests` reads `kac.yml` and fails a job that `validate` does not name, and its header comment says why a
job outside the gate is invisible. The `lint` job runs `actionlint` over every workflow. What neither answers is a
permission nobody uses, a credential in the wrong place, or `.azuredevops/kac.yml` drifting from its GitHub twin, so a
reader is what catches those.

The timeout rule reaches the publishing jobs, where a hang parks a concurrency group and the next merge queues behind
it. `ChangelogTests` fails a version with no section, which is what makes a release body available to the tag step.

A published version is permanent on both registries, so the recovery path for a bad release is the next version. The
tool's publish runs the three test layers again against the merge commit, because the gate saw the pull request's head
and `main` moved beneath it.

The pinning rule reaches `.github/workflows/`. The starter at `template/.github/workflows/kac.yml` is a seed that
belongs to whichever corpus receives it, and it names `actions/checkout@v4` today.

## Sources and further reading

- **Normative.** [OpenSSF Scorecard] defines `Token-Permissions`, `Pinned-Dependencies` and `Dangerous-Workflow`, the
  three checks the permission and pinning rules above answer.
- **Normative.** [actionlint] sets how a workflow is written. It runs with its own defaults here, and this standard
  adds what a linter reading one file cannot see.
- **Normative.** [Security hardening for GitHub Actions] is what Scorecard's three checks operationalise, and it
  carries the reasoning behind each.
- **Normative.** [Trusted publishing on nuget.org] defines the policy this repository's publish authenticates against.
- **Informative.** [Semantic Versioning 2.0.0] is the grammar both `<Version>` and `content-version` are read under.

## Changelog

- 2026-09-02: initial version.

[OpenSSF Scorecard]: https://github.com/ossf/scorecard/blob/main/docs/checks.md
[Security hardening for GitHub Actions]: https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions
[Semantic Versioning 2.0.0]: https://semver.org
[Trusted publishing on nuget.org]: https://learn.microsoft.com/nuget/nuget-org/trusted-publishing
[actionlint]: https://github.com/rhysd/actionlint
