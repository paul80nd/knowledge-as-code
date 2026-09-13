---
id: ctl-0001
type: control
tier: normative
status: active
verifies: [ eng:std-GATES, std-CI ]
mechanism: ci
frequency: per-pr
evidence: The `validate` check on the pull request, and the branch rule on `main` in the repository settings.
applies-to:
  - all
owner: human:paul.law
tags: [ branch-protection, continuous-integration ]
---

# A merge waits for the validate job

`Control: ctl-0001` `ACTIVE`

A failing job in `kac.yml` stops the merge.

## What it checks

* [std-CI.the-gate-runs-on-every-pull-request-into-main] says the branch rule on `main` "**MUST** name `validate` as
  the check a merge waits for".
* `eng:std-GATES.a-failing-check-stops-the-change` says branch policy "**MUST** block a merge while any required check
  is failing".

## How it works

`kac.yml` declares a `validate` job whose `needs:` lists `corpora`, `tool`, `lint`, `docs`, `round-trip` and
`import-round-trip`. The job itself runs one `echo`. GitHub reports `validate` once every job in that list has passed,
so the branch rule requires one check name.

Three of those jobs are matrices. A matrix job reports one check per cell: `corpora` reports as `corpora (library)`
and three more. A branch rule naming the matrix would wait for a check that never arrives.

## Coverage and gaps

The branch rule lives in GitHub's repository settings, and no file in this repository has a copy of it. Nothing in CI
reads it, so a rule renamed, relaxed or turned off shows nowhere on the pull request. [ctl-0002] checks the other
half: whether `validate` still needs every job.

[ctl-0002]: 0002-workflow-gate-tests.md
[std-CI.the-gate-runs-on-every-pull-request-into-main]: ../standards/workflows.md#the-gate-runs-on-every-pull-request-into-main
