---
id: ctl-0015
type: control
tier: normative
status: active
verifies: [ std-CI ]
mechanism: ci
frequency: per-pr
evidence: The `lint` job's log, under the step "Check the workflows and dependabot.yml for insecure patterns".
applies-to:
  - all
review-by: "2027-10-23"
owner: human:paul.law
tags: [ github-actions, linting, permissions, pinning, security ]
---

# Every workflow passes zizmor

`Control: ctl-0015` `ACTIVE`

zizmor settles whether a workflow is safe to run, once actionlint has settled that it runs.

## What it checks

* [std-CI.every-action-is-pinned-to-a-commit] says a step "**MUST** pin its action to a commit SHA". zizmor's
  `unpinned-uses` audit fails a `uses:` naming a tag or a branch. Its `impostor-commit` audit fails a commit that is not
  in the action's own repository.
* [std-CI.a-checkout-leaves-no-credential-behind] says a checkout "**MUST** set `persist-credentials: false`". zizmor's
  `artipacked` audit fails a checkout that leaves the setting out.
* [std-CI.a-job-declares-the-permission-it-needs] says "A workflow **MUST** declare `permissions: contents: read` at
  its top level". zizmor's `excessive-permissions` audit fails a workflow with no `permissions:` block, and one granting
  a write permission at its top level.

## How it works

The `lint` job installs zizmor at the version `.github/requirements.txt` pins, then runs it over `.github/` and
`template/.github/`. In each folder it reads the workflows and `dependabot.yml`. It runs with the default persona and
no configuration file. Any finding exits non-zero and fails the job.

The step passes `github.token` as `GH_TOKEN`. With a token, zizmor runs its online audits, which ask GitHub about each
pinned commit and each action's advisories.

A finding a workflow keeps on purpose takes an inline `# zizmor: ignore[...]` comment, and the comment above the step
gives the reason. These audits are suppressed that way:

* `adhoc-packages`, on each step installing the Claude Code CLI at `latest`.
  [std-CI.every-action-is-pinned-to-a-commit] allows a tool at `latest` in a job with `contents: read`, and each of
  those jobs has only that.
* `use-trusted-publishing`, on the push to GitHub Packages in `publish-corpus.yml`. GitHub Packages offers no trusted
  publishing, and that push takes `github.token`.

## Coverage and gaps

zizmor covers part of each rule it checks:

* It does not know which permission a job uses, so a job declaring one it never uses passes.
* In a workflow of one job, it reports a top-level write permission only under its `pedantic` persona.
* It reads a workflow file, so it never sees the branch rule or the `nuget.org` environment.

It reports nothing about these rules of [std-CI]:

* a secret interpolated into a script body, which only its `auditor` persona reports
* a secret printed to the log
* a write permission declared on more than one job

zizmor also reports a template expansion an attacker can write, and a trigger such as `pull_request_target`. No rule of
[std-CI] states either one, and the workflows here have neither.

In each `dependabot.yml`, its `dependabot-cooldown` audit fails an ecosystem that waits fewer than seven days before
proposing a new release. Each ecosystem here sets `cooldown: {default-days: 7}`. No rule of [std-CONFIG] states a
cooldown.

[std-CI]: ../standards/workflows.md
[std-CI.a-checkout-leaves-no-credential-behind]: ../standards/workflows.md#a-checkout-leaves-no-credential-behind
[std-CI.a-job-declares-the-permission-it-needs]: ../standards/workflows.md#a-job-declares-the-permission-it-needs
[std-CI.every-action-is-pinned-to-a-commit]: ../standards/workflows.md#every-action-is-pinned-to-a-commit
[std-CONFIG]: ../standards/configuration.md
