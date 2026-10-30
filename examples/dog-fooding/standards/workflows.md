---
id: std-CI
type: standard
tier: normative
status: active
implements: [ eng:pol-ACCS.DUTIES, eng:pol-ACCS.LEAST, eng:pol-AUTV.BLOCK, eng:pol-AUTV.INTEG, eng:pol-EVER.BRANCH,
  eng:pol-PIPE.ASCODE, eng:pol-PIPE.DEPLOY, eng:pol-PIPE.GATES, eng:pol-PIPE.LOCAL, eng:pol-PIPE.MANUAL,
  eng:pol-PIPE.TRACE, eng:pol-SCRT.EMBED, eng:pol-SCRT.LOGS, eng:pol-SCRT.ROTATE, eng:pol-SCRT.STORE,
  eng:pol-TRUS.MALWARE, eng:pol-TRUS.MUTATE, eng:pol-TRUS.REPO, eng:pol-TRUS.SOURCE, eng:pol-TRUS.UNTRUST ]
verified-by: [ ctl-actionlint, ctl-changelog-tests, ctl-content-version-tests, ctl-merge-gate, ctl-workflow-gate-tests,
  ctl-zizmor ]
applies-to:
  - all
review-by: "2027-09-02"
owner: human:paul.law
tags: [ continuous-integration, github-actions, publishing ]
---

# Every check and every publish runs from a workflow in this repository

`Standard: std-CI` `ACTIVE`

## Summary

One workflow gates a pull request. The others publish what has merged: the tool to nuget.org, each corpus to GitHub
Packages, the plugins to the `marketplace` branch, and the site to GitHub Pages. A job declares what it may do before
it does it, pins the actions it runs, and stores no credential of its own. Those three rules are [OpenSSF Scorecard]'s
`Token-Permissions`, `Pinned-Dependencies` and `Dangerous-Workflow` checks. A checkout leaves no credential for a later
step, which is [zizmor]'s `artipacked` audit. [actionlint] decides the rest of how a workflow is written.

## Rules

### GATE: The gate runs on every pull request into main

- A check that can block a merge **MUST** run in `.github/workflows/kac.yml` on a pull request into `main`.
- `kac.yml` **MUST** name every one of its jobs in the `needs:` of its `validate` job.
- The `validate` job **MUST** fail when any job in its `needs:` fails.
- The `validate` job **MUST** declare `if: always()`, because GitHub skips a job whose dependency failed and counts
  a skipped required check as passing.
- The branch rule on `main` **MUST** name `validate` as the check a merge waits for.
- A job **MUST NOT** declare `continue-on-error`.
- A workflow **MUST** pass `actionlint`, which also puts every `run:` block through shellcheck.
- A workflow file **MUST** arrive on `main` by the same reviewed merge as any other change.

_**Covers:** `eng:pol-AUTV.BLOCK`, `eng:pol-AUTV.INTEG`, `eng:pol-EVER.BRANCH`, `eng:pol-PIPE.ASCODE`,
`eng:pol-PIPE.GATES`_

### PERMS: A job declares the permission it needs

- A workflow **MUST** declare `permissions: contents: read` at its top level, which is what [OpenSSF Scorecard]'s
  `Token-Permissions` check asks for.
- A job needing wider access **MUST** declare that permission on itself.
- A job **MUST NOT** declare a permission no step in it uses.
- A workflow **MUST NOT** commit to a branch a person edits.

_**Covers:** `eng:pol-ACCS.LEAST`_

### PINNED: Every action is pinned to a commit

- A step in `.github/workflows/` or `template/.github/workflows/` **MUST** pin its action to a commit SHA, which is
  what [OpenSSF Scorecard]'s `Pinned-Dependencies` check asks for.
- A pinned step **SHOULD** state the released version in a trailing comment, as `# v7.0.1`, because a reviewer reads
  the version there.
- `.github/dependabot.yml` **MUST** track `github-actions`.
- A job with a write permission **MUST NOT** install a tool at a moving version.
- A workflow needing a tool at `latest` **MUST** install it in a job with `contents: read`.

_**Covers:** `eng:pol-TRUS.SOURCE`, `eng:pol-TRUS.UNTRUST`_

### CREDS: A workflow holds no credential of its own

- A workflow **MUST** take every credential from GitHub's secret store or from an identity it exchanged.
- A publish to nuget.org **MUST** authenticate through a trusted publishing policy naming this repository, this
  workflow file and the `nuget.org` environment.
- A job **MUST** exchange its own identity for a short-lived key in the step before the one that uses it.
- A workflow calling GitHub's own API **MUST** take `github.token`.
- A step **MUST** pass a secret to a script through `env:`.
- A step **MUST NOT** interpolate a secret into a script body.
- A step **MUST NOT** print a secret.

_**Covers:** `eng:pol-SCRT.EMBED`, `eng:pol-SCRT.LOGS`, `eng:pol-SCRT.ROTATE`, `eng:pol-SCRT.STORE`_

### PERSIST: A checkout leaves no credential behind

- A checkout in `.github/workflows/` or `template/.github/workflows/` **MUST** set `persist-credentials: false`, which
  is what [zizmor]'s `artipacked` audit asks for.
- A step needing git write access **MUST** supply the token itself.

_**Covers:** `eng:pol-ACCS.LEAST`_

### WRITE: One job holds the write permission

- A publishing workflow **MUST** declare its write permission on one job.
- A workflow publishing a package or a branch **MUST** declare `needs:` on a job with `contents: read`.
- A workflow publishing a package or a branch **MUST** run its checks again against the merge commit.
- The publishing job **MUST** build what it publishes from that commit.
- The publishing job **MUST** declare `timeout-minutes`.
- A workflow publishing a version **MUST** declare a `concurrency` group that queues rather than cancels.
- A branch a workflow publishes **MUST** contain only the files that workflow built.
- A workflow writing to a branch **SHOULD** empty its worktree before it stages a file.
- That workflow **SHOULD** prove `git ls-files` returns nothing in the emptied worktree.
- A person **MUST NOT** edit a published branch, package or release by hand.

_**Covers:** `eng:pol-PIPE.DEPLOY`, `eng:pol-PIPE.MANUAL`_

### SCAN: A publishing workflow scans what it publishes

- A publishing workflow **MUST** scan what it publishes for malicious content before it publishes it.
- A publishing workflow **MUST** fail before it publishes where that scan reports a finding.

_**Covers:** `eng:pol-TRUS.MALWARE`_

### RELEASE: A version moves by hand and publishes once

- A change to `kac` a user can observe **MUST** take a line under `## Unreleased` in `tooling/kac/CHANGELOG.md`, in the
  pull request making that change.
- A pull request **MUST NOT** move `<Version>` in `tooling/kac/kac.csproj` unless the branch owner has decided to
  release.
- A pull request moving `<Version>` **MUST** rename `## Unreleased` to that version and its date.
- That rename **SHOULD** sit in the commit moving `<Version>`.
- `content-version` in a corpus's `.corpus.yaml` **MUST** move in the pull request changing what that corpus knows.
- A publishing job **MUST** ask the registry whether it already holds the version in front of it.
- Where the registry holds that version already, a publishing job **MUST** finish green.
- A publish to nuget.org **MUST** wait for a person to approve the `nuget.org` environment.
- A publish to nuget.org **MUST** tag the commit it published from.
- The release notes **MUST** be that version's section of `tooling/kac/CHANGELOG.md`.
- A person **MUST NOT** push a package from their own machine.

_**Covers:** `eng:pol-ACCS.DUTIES`, `eng:pol-PIPE.LOCAL`, `eng:pol-PIPE.TRACE`, `eng:pol-TRUS.MUTATE`,
`eng:pol-TRUS.REPO`_

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
        with:
          persist-credentials: false

❌ Avoid
permissions: write-all

jobs:
  publish:
    steps:
      - uses: actions/checkout@v7
```

`write-all` gives every scope to every job in the file, including the ones that only read. A job with no `needs:`
publishes whatever the checkout contains, verified by nothing. A job with no `timeout-minutes` keeps its concurrency
group for six hours when a push hangs. A tag moves, so `@v7` is a different action tomorrow. A checkout without
`persist-credentials: false` leaves the job's token for every later step to push with.

## Conformance checklist

- [ ] `actionlint` passes.
- [ ] Every job in `kac.yml` is named in `validate`'s `needs:`.
- [ ] `validate` fails when any job in its `needs:` fails.
- [ ] `validate` declares `if: always()`.
- [ ] Every `uses:` in `.github/workflows/` and `template/.github/workflows/` names a commit SHA.
- [ ] Every checkout sets `persist-credentials: false`, and every step needing git write access supplies the token.
- [ ] Every workflow declares `permissions: contents: read` at its top level.
- [ ] Every job with a write permission has a step that uses it.
- [ ] No secret appears in a workflow's text, and no step prints one.
- [ ] Every publishing job declares `timeout-minutes`, and every package publish sits behind a verifying job.
- [ ] Every publishing workflow scans what it publishes for malicious content, and fails before it publishes on a
  finding.
- [ ] Every observable change to `kac` here has its line under `## Unreleased`.
- [ ] A pull request moving `<Version>` has the branch owner's decision behind it, and has renamed `## Unreleased`.
- [ ] Every corpus whose export changed has moved its `content-version`.

## Rationale and provenance

Read-only permission keeps CI out of the files a person edits. `generate --check` reports a stale generated file and
names the command to run locally, so no job needs to write one back.

A `content-version` that has not moved publishes nothing, so no reader sees the edited record and the published copy
drifts from `main`. [ctl-content-version-tests] reports that on the pull request, and a reviewer judges whether the move
is the right size. Merging a move of `<Version>` publishes to nuget.org, and a published version cannot be replaced. So
the branch owner decides whether to release before the pull request opens, and an entry waits under `## Unreleased`
until they do. What each of these numbers means, and what a producer's move obliges of the corpora consuming it, are
[std-VERS]'s. This standard covers when a number moves in the delivery flow, and how a workflow publishes it.

`WorkflowGateTests` reads `kac.yml`. It fails a job that `validate` does not name, a `validate` without
`if: always()`, and a `validate` with no step reading `needs.*.result`. Its header comment says why a job outside the
gate is invisible. The `lint` job runs `actionlint` over every workflow, and then zizmor over every workflow and each
`dependabot.yml`. [ctl-zizmor] says which rules zizmor checks. Nothing checks a permission nobody uses, or a secret
written into a script body. A reviewer catches both.

A checkout keeping its credential lets every later step push with the job's token, a test run or a package restore
included. The one `git push` here, to the `marketplace` branch, takes the token in its own step, so the restores and
builds before it run without write access.

The timeout rule covers the publishing jobs, where a hang keeps a concurrency group and the next merge queues behind
it. `ChangelogTests` fails a version with no section, so the tag step always has a release body to publish.

The tool's publish runs the three test layers again against the merge commit, because the gate read the pull request's
head and `main` moved after that.

No publishing workflow here scans what it publishes. [dev-package-unscanned] records that departure from the scan
rule, and what would close it.

## Sources and further reading

- **Normative.** [OpenSSF Scorecard] defines `Token-Permissions`, `Pinned-Dependencies` and `Dangerous-Workflow`, the
  three checks the permission and pinning rules above satisfy.
- **Normative.** [actionlint] sets how a workflow is written. It runs with its own defaults here, and this standard
  adds what a linter reading one file cannot see.
- **Normative.** [Security hardening for GitHub Actions] is what Scorecard's three checks operationalise, and it
  gives the reasoning behind each.
- **Normative.** [Trusted publishing on nuget.org] defines the policy this repository's publish authenticates against.
- **Normative.** [zizmor] defines `artipacked`, the audit the checkout rule above satisfies.

## Changelog

- 2026-10-30: a publishing workflow scans what it publishes for malicious content, and fails on a finding.
- 2026-10-30: the `validate` job fails when a job it needs fails, and runs with `if: always()` to do it.
- 2026-10-21: a checkout sets `persist-credentials: false`, and a step needing git write access supplies the token
  itself.
- 2026-10-21: the pinning rule covers `template/.github/workflows/`, which holds the starter every new corpus built on
  GitHub receives.
- 2026-10-18: the trailing version comment, emptying the worktree and renaming `## Unreleased` in the commit moving
  `<Version>` become `SHOULD`. A published branch contains only what its workflow built, and the rename lands in the
  pull request moving `<Version>`.
- 2026-09-23: dropped the clauses binding `.azuredevops/kac.yml` to its GitHub twin, and the file with them. No Azure
  DevOps project ever ran it, and mirroring it by hand cost an edit on every workflow change.
- 2026-09-15: made the release the branch owner's decision, so an observable change takes its changelog entry under
  `## Unreleased` without moving `<Version>`.
- 2026-09-07: gave the stamp semantics and the consumer's lock and range to [std-VERS], keeping when a version moves,
  how a workflow publishes it, and the registry check that holds a published version in place.
- 2026-09-06: took the changelog, semantic `content-version` and consumer-repointing rules that `CLAUDE.md` had been
  stating a second time.
- 2026-09-02: initial version.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[OpenSSF Scorecard]: https://github.com/ossf/scorecard/blob/main/docs/checks.md
[Security hardening for GitHub Actions]: https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions
[Trusted publishing on nuget.org]: https://learn.microsoft.com/nuget/nuget-org/trusted-publishing
[actionlint]: https://github.com/rhysd/actionlint
[ctl-content-version-tests]: ../controls/content-version-tests.md
[ctl-zizmor]: ../controls/zizmor.md
[dev-package-unscanned]: ../deviations/package-unscanned.md
[std-VERS]: versioning.md
[zizmor]: https://docs.zizmor.sh/audits/#artipacked
