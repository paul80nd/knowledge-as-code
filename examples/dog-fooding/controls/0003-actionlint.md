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
review-by: "2027-09-22"
owner: human:paul.law
tags: [ github-actions, linting, shell ]
---

# Every workflow passes actionlint

`Control: ctl-0003` `ACTIVE`

A workflow can be valid YAML and still not run.

## What it checks

* [std-CI.the-gate-runs-on-every-pull-request-into-main] says a workflow "**MUST** pass `actionlint`, which also puts
  every `run:` block through shellcheck".

## How it works

The `lint` job installs actionlint at `v1.7.12` with `go install`, then runs it with no arguments. actionlint reads
every file under `.github/workflows/`: the syntax, the expressions and the inputs each action declares. The runner has
shellcheck installed, so actionlint passes each `run:` block to it as well. actionlint runs on its own defaults, and
this repository has no configuration file for it.

## Coverage and gaps

actionlint reports nothing about whether a workflow is safe to run. [ctl-0015] runs zizmor in the same job, and checks
part of the permission, pinning and checkout rules of [std-CI]. Neither linter checks these faults, and a reviewer
catches them:

* a permission a job declares and never uses
* a secret written into a script body, or printed to the log
* a write permission declared on more than one job, or a publish with no verifying job before it

actionlint publishes no manifest Dependabot reads, so the `v1.7.12` pin moves when somebody edits the job.

[ctl-0015]: 0015-zizmor.md
[std-CI]: ../standards/workflows.md
[std-CI.the-gate-runs-on-every-pull-request-into-main]: ../standards/workflows.md#the-gate-runs-on-every-pull-request-into-main
