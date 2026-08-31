---
id: ctl-0001
tier: normative
status: active
owner: alex.doe
verifies: [std-ERRORS]
mechanism: ci
evidence: The build log for the pipeline this control runs in.
---

# Frequency not stated

`Control: ctl-0001` `ACTIVE`

## What it checks

Nothing, in itself. It exists so that `required-when: 'mechanism != not-enforced'` has a document to hold:
the mechanism is `ci`, so a frequency must be stated, and none is. The `evidence` field is filled in, because
the same mechanism value is what `mechanism-has-evidence` asks about and that rule has a document of its own.

## How it works

By being wrong in exactly one way.

## Coverage and gaps

The gap is `frequency`, which is the point.
