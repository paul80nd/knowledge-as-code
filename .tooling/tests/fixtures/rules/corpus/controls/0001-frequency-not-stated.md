---
id: ctl-0001
tier: normative
status: active
owner: alex.doe
verifies: [std-0001]
mechanism: ci
---

# Frequency not stated

`Control: ctl-0001` `ACTIVE`

## What it checks

Nothing, in itself. It exists so that `required-when: 'mechanism != not-enforced'` has a document to hold:
the mechanism is `ci`, so a frequency must be stated, and none is.

## How it works

By being wrong in exactly one way.

## Coverage and gaps

The gap is `frequency`, which is the point.
