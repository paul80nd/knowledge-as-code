---
id: ctl-health-reports-commit
type: control
tier: normative
status: active
verifies: [ std-DEPLOY.COMMIT ]
mechanism: not-enforced
review-by: "2031-08-05"
owner: human:alex.doe
---

# Nothing checks that a running service reports its commit

`Control: ctl-health-reports-commit` `ACTIVE`

## What it checks

* [std-DEPLOY.COMMIT] says an artefact **MUST** record the commit it was built from.

## How it works

Nothing runs.

## Coverage and gaps

A reviewer reading the health handler is the only check.

[std-DEPLOY]: ../standards/common/deployment.md
