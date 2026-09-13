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

A job the gate does not list still runs and reports. It blocks no merge.

## What it checks

* [std-CI.the-gate-runs-on-every-pull-request-into-main] says `kac.yml` "**MUST** name every one of its jobs in the
  `needs:` of its `validate` job".

## How it works

`WorkflowGateTests` in `tooling/kac.tests` reads `.github/workflows/kac.yml` and collects the keys under `jobs:`. It
compares that set against `validate`'s `needs:` plus `validate` itself. The `tool` job runs it under
`dotnet test tooling/kac.tests`.

Comparing sets catches two faults. A job added with no line in `needs:` fails. A `needs:` entry listing a job the file
no longer declares fails as well.

## Coverage and gaps

The test reads `.github/workflows/kac.yml` and no other workflow. `.azuredevops/kac.yml` is a flat step list with no
jobs, so this fault cannot arise there. A reader confirms whether that file still runs the same steps in the same
order.

The branch rule sits outside the repository. [ctl-0001] covers whether it still names `validate`.

[ctl-0001]: 0001-merge-gate.md
[std-CI.the-gate-runs-on-every-pull-request-into-main]: ../standards/workflows.md#the-gate-runs-on-every-pull-request-into-main
