---
id: ctl-0003
type: control
tier: normative
status: active
verifies: [std-ERRORS]
mechanism: ci
frequency: per-pr
evidence: The pipeline log for the step that runs the check.
review-by: "2020-06-01"
owner: human:alex.doe
---

# A control nobody has read since 2020

`Control: ctl-0003` `ACTIVE`

## What it checks

The same rule `ctl-0001` and `ctl-0002` name. Nothing about the mechanism is wrong here.

## How it works

By stating a `review-by` that fell in 2020. `control-in-date` reports it: the check may well still run,
and nobody has confirmed that this description is what it runs.

## Coverage and gaps

The gap is the reading, not the check.
