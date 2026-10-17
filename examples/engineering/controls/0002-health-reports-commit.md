---
id: ctl-0002
type: control
tier: normative
status: active
verifies: [ std-DEPLOY ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ deployment, health-checks, traceability ]
---

# Nothing checks that a running service reports its commit

`Control: ctl-0002` `ACTIVE`

No check fails a deployment whose service cannot say which commit it was built from.

## What it checks

* [std-DEPLOY.the-artefact-is-built-once] says an artefact "**MUST** record the commit it was built from, so a running
  version identifies the change it came from".

## How it works

Nothing runs. The check this rule needs is a step at the end of each deployment. It requests the service's health
endpoint and compares the commit it reports with the commit the pipeline built:

```sh
test "$(curl -fsS "$SERVICE_URL/health" | jq -r .commit)" = "$BUILD_COMMIT"
```

The step fails the deployment where the two differ, or where the endpoint reports no commit. It runs per deployment.
A pre-commit check or a Claude Code hook sees only the source, and cannot tell what a running service reports.

## Coverage and gaps

A reviewer reading the health handler is the only thing that notices a missing commit today.

The standard names no endpoint and no field, so the check assumes `/health` and `commit`. The other rules under
[std-DEPLOY.the-artefact-is-built-once] go unchecked:

* the pipeline builds once and promotes that build
* the deployment record names the artefact, the change and the approval
* nothing deploys an artefact built outside the pipeline

[std-DEPLOY.the-artefact-is-built-once]: ../standards/common/deployment.md#the-artefact-is-built-once
