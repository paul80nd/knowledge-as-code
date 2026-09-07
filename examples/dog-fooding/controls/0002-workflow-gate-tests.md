---
id: ctl-0002
type: control
tier: normative
status: active
verifies: [ std-CI ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - all
owner: human:paul.law
tags: [ continuous-integration, github-actions ]
---

# Every job in kac.yml sits behind the gate

`Control: ctl-0002` `ACTIVE`

A job the gate does not name runs, reports, and blocks nothing.

## What it checks

* [std-CI.the-gate-runs-on-every-pull-request-into-main] says `kac.yml` "**MUST** name every one of its jobs in the
  `needs:` of its `validate` job".

## How it works

`WorkflowGateTests` in `tooling/kac.tests` reads `.github/workflows/kac.yml` and collects the keys under `jobs:`. It
then compares that set against `validate`'s `needs:` with `validate` itself added back. The `tool` job runs it under
`dotnet test tooling/kac.tests`.

Comparing sets catches both faults. A job added without a line in `needs:` fails, and so does a `needs:` entry naming
a job the file no longer declares.

## Coverage and gaps

It reads `.github/workflows/kac.yml` and no other workflow. `.azuredevops/kac.yml` is a flat step list holding no
jobs, so this fault cannot arise there. Whether that file still runs the same steps in the same order is a reader's
to confirm.

Whether the branch rule still names `validate` is outside the repository, and [ctl-0001] says what covers it.

[ctl-0001]: 0001-merge-gate.md
[std-CI.the-gate-runs-on-every-pull-request-into-main]: ../standards/workflows.md#the-gate-runs-on-every-pull-request-into-main
