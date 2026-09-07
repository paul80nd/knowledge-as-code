---
id: ctl-0003
type: control
tier: normative
status: active
verifies: [ std-CI ]
mechanism: ci
frequency: per-pr
evidence: The `lint` job's log on the pull request, under the step "Check the workflows".
applies-to:
  - all
owner: paul.law
tags: [ github-actions, linting, shell ]
---

# Every workflow passes actionlint

`Control: ctl-0003` `ACTIVE`

A workflow can be valid YAML and still not run.

## What it checks

* [std-CI.the-gate-runs-on-every-pull-request-into-main] says a workflow "**MUST** pass `actionlint`, which also puts
  every `run:` block through shellcheck".

## How it works

The `lint` job installs actionlint at `v1.7.12` with `go install`, then runs it with no arguments. It walks every file
under `.github/workflows/` and reads the syntax, the expressions and the inputs each action declares. The runner
carries shellcheck, so actionlint hands it each `run:` block as well. actionlint runs on its own defaults, and this
repository holds no configuration for it.

## Coverage and gaps

actionlint reports nothing about a permission a job holds and never uses, a credential written in the wrong place, or
`.azuredevops/kac.yml` drifting from its GitHub twin. A reviewer is what catches those three, and they are most of
what [std-CI] states: its permission, credential, pinning and publishing rules have no check here.

actionlint ships no manifest Dependabot reads, so the `v1.7.12` pin moves when somebody edits the job.

[std-CI]: ../standards/workflows.md
[std-CI.the-gate-runs-on-every-pull-request-into-main]: ../standards/workflows.md#the-gate-runs-on-every-pull-request-into-main
