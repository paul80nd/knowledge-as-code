---
id: std-0004
tier: normative
status: draft
axis: common
implements:
  - pol-AUTV
  - pol-DEVI
applies-to:
  - all
review-by: "2027-08-26"
owner: paul.law
tags: [ continuous-integration, quality-gates, testing ]
---

# A failing check blocks the merge

`Standard: std-0004` `DRAFT`

## Summary

Every push builds and tests automatically, and a check that fails stops the change. Turning a check off is a recorded
deviation with a named owner.

## Rules

- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it ([pol-AUTV.INTEG]).
- Branch policy **MUST** block a merge while any required check is failing ([pol-AUTV.BLOCK]).
- The build **MUST** run from a clean checkout, on an agent provisioned from a definition in the repository
  ([pol-AUTV.MACHINE]).
- A fix for a defect **MUST** arrive with a test that fails without it ([pol-AUTV.REGRESS]).
- A team **MUST NOT** merge over a failing check without a recorded deviation ([pol-AUTV.BYPASS]).
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation ([pol-AUTV.DISABLE]).
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk ([pol-DEVI.OWNER]).
- That deviation **MUST** carry a review date ([pol-DEVI.EXPIRY]).

## Examples

```
Good
  # covers-api.test.ts
  test.skip("retries on 503")   // deviation DEV-0114, r.okafor, review 2026-10-01

Avoid
  test.skip("retries on 503")
```

Both stop the test running. The first leaves a name, a reason and a date, so somebody comes back to it. Nobody comes
back to the second until the retry path breaks in production.

## Conformance checklist

- [ ] A push triggers the build with no manual step.
- [ ] Branch policy lists the build and the test suite as required checks.
- [ ] The build passes from a clean clone on a fresh agent.
- [ ] Every skipped, excluded or suppressed check names a deviation.
- [ ] Every such deviation is open, owned by an individual, and inside its review date.
- [ ] The last defect fix in this repository carries a test that fails without the fix.

## Rationale and provenance

A check that can be waived quietly stops being a gate and becomes a report. Recording the waiver keeps the exception
visible and gives it an end date.

- [pol-AUTV] commits us to verifying every change automatically and treating a failure as blocking.
- [pol-DEVI] sets what a recorded deviation has to carry.

[pol-AUTV]: ../../policies/autv-automated-verification.md
[pol-AUTV.BLOCK]: ../../policies/autv-automated-verification.md#clauses
[pol-AUTV.BYPASS]: ../../policies/autv-automated-verification.md#clauses
[pol-AUTV.DISABLE]: ../../policies/autv-automated-verification.md#clauses
[pol-AUTV.INTEG]: ../../policies/autv-automated-verification.md#clauses
[pol-AUTV.MACHINE]: ../../policies/autv-automated-verification.md#clauses
[pol-AUTV.REGRESS]: ../../policies/autv-automated-verification.md#clauses
[pol-DEVI]: ../../policies/devi-deviations-are-recorded.md
[pol-DEVI.EXPIRY]: ../../policies/devi-deviations-are-recorded.md#clauses
[pol-DEVI.OWNER]: ../../policies/devi-deviations-are-recorded.md#clauses
