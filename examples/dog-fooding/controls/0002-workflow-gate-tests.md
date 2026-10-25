---
id: ctl-0002
type: control
tier: normative
status: active
verifies: [ eng:std-GATES.BLOCK, std-CI.GATE ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ continuous-integration, github-actions ]
---

# Every job in kac.yml sits behind the gate

`Control: ctl-0002` `ACTIVE`

A job the gate does not list still runs and reports. It blocks no merge.

## What it checks

* [std-CI.GATE] says `kac.yml` "**MUST** name every one of its jobs in the
  `needs:` of its `validate` job".
* `eng:std-GATES.BLOCK` says branch policy "**MUST** block a merge while any required check is
  failing". The test checks the half of that `kac.yml` decides: that `validate` fails when a job it needs fails.

## How it works

`WorkflowGateTests` in `tooling/kac.tests` reads `.github/workflows/kac.yml`. The `tool` job runs it under
`dotnet test --project tooling/kac.tests`. It asks three things of the `validate` job.

* **Every job is in `needs:`.** The test compares the keys under `jobs:` against `validate`'s `needs:` plus `validate`
  itself. A job added with no line in `needs:` fails. A `needs:` entry listing a job the file no longer declares fails
  as well.
* **The gate runs after a failure.** `validate`'s `if:` is `always()`. Without it, a failed job skips `validate`, and
  GitHub counts a skipped required check as passing.
* **The gate reads the results.** A step of `validate` reads `needs.*.result`. With `always()` and no such step, the
  gate would pass every pull request.

## Coverage and gaps

The test reads `.github/workflows/kac.yml` and no other workflow. The publishing workflows gate nothing, so a job
outside `validate`'s `needs:` is a fault only `kac.yml` can have.

The branch rule sits outside the repository. [ctl-0001] covers whether it still names `validate`.

[ctl-0001]: 0001-merge-gate.md
[std-CI.GATE]: ../standards/workflows.md#gate-the-gate-runs-on-every-pull-request-into-main
