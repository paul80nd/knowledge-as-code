---
id: std-GATES
tier: normative
status: draft
implements: [ pol-AUTV.BLOCK, pol-AUTV.BYPASS, pol-AUTV.DISABLE, pol-AUTV.INTEG, pol-AUTV.MACHINE, pol-AUTV.REGRESS,
  pol-DEVI.EXPIRY, pol-DEVI.OWNER ]
applies-to:
  - all
review-by: "2027-08-26"
owner: paul.law
tags: [ continuous-integration, quality-gates, testing ]
---

# A failing check blocks the merge

`Standard: std-GATES` `DRAFT`

## Summary

Every push builds and tests automatically, and a check that fails stops the change. Turning a check off is a recorded
deviation with a named owner.

## Rules

### Every change is built and tested automatically

- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it.
- The build **MUST** run from a clean checkout, on an agent provisioned from a definition in the repository.
- A fix for a defect **MUST** arrive with a test that fails without it.

_**Covers:** [pol-AUTV].INTEG, [pol-AUTV].MACHINE, [pol-AUTV].REGRESS_

### A failing check stops the change

- Branch policy **MUST** block a merge while any required check is failing.
- A team **MUST NOT** merge over a failing check without a recorded deviation.
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation.
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk.
- That deviation **MUST** carry a review date.

_**Covers:** [pol-AUTV].BLOCK, [pol-AUTV].BYPASS, [pol-AUTV].DISABLE, [pol-DEVI].EXPIRY, [pol-DEVI].OWNER_

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

## Sources and further reading

- **Informative.** [SLSA build levels] cover what a build platform guarantees before a gate running on it means
  anything: a clean checkout, an agent defined in the repository, and provenance for what the build produced.

[pol-AUTV]: ../../policies/delivery/autv-automated-verification.md#clauses
[pol-DEVI]: ../../policies/governance/devi-deviations-are-recorded.md#clauses
[SLSA build levels]: https://slsa.dev/spec/v1.0/levels
