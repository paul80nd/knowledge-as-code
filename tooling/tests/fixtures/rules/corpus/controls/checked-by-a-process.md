---
id: ctl-checked-by-a-process
type: control
tier: normative
status: active
owner: human:alex.doe
verifies: [std-ERRORS.WHAT]
mechanism: manual
frequency: quarterly
last-verified: { at: "2020-01-01", by: process:nightly-audit, method: tabletop }
evidence: The audit note filed after each check.
review-by: "2030-01-01"
---

# Checked by a process

`Control: ctl-checked-by-a-process` `ACTIVE`

## What it checks

Nothing, in itself. `last-verified` names a process as `by`, so `verified-by-a-person-or-agent` reports it. A check a
process runs is a `ci` control. It states a `method` as well, which belongs to a runbook, so
`check-states-no-method` reports it too.

## How it works

By being wrong in exactly two ways, each reported under its own id.

## Coverage and gaps

The gap is `last-verified`, which is the point.
