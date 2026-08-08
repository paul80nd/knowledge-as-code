---
id: ctl-0002
tier: normative
status: active
owner: alex.doe
verifies: [std-0001]
mechanism: review-checklist
frequency: per-pr
---

# Evidence not named

`Control: ctl-0002` `ACTIVE`

## What it checks

Nothing that can be shown. The mechanism says a human works through a checklist every pull request, and no
field says where the record of that lives, so nobody can tell the control from a claim about one.

## How it works

`mechanism` is anything other than `not-enforced`, and `evidence` is absent. `frequency` is filled in, so
the `required-when` on that field stays out of the way and this document owns one finding.

## Coverage and gaps

The gap is the evidence, which is the point. `not-enforced` is the honest value for a control nobody can
produce a record for, and the reason this reports rather than fails.
