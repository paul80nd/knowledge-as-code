---
id: std-GATES
type: standard
tier: normative
status: draft
implements: [ pol-AUTV.BLOCK, pol-AUTV.BYPASS, pol-AUTV.DISABLE, pol-AUTV.INTEG, pol-AUTV.MACHINE, pol-AUTV.REGRESS,
  pol-DEVI.EXPIRY, pol-DEVI.OWNER ]
verified-by: [ ctl-0004 ]
applies-to:
  - all
review-by: "2027-08-26"
owner: human:paul.law
tags: [ continuous-integration, quality-gates, testing ]
---

# A failing check blocks the merge

`Standard: std-GATES` `DRAFT`

## Summary

Every push builds the code and runs the test suite automatically. A check that fails stops the change. Turning a check
off takes a recorded deviation with a named owner.

## Rules

### BUILD: Every change is built and tested automatically

- A push to any branch **MUST** trigger a build and the test suite, without anyone asking for it.
- The build **MUST** run from a clean checkout, on an agent that is not a person's own machine.
- The agent **SHOULD** be provisioned from a definition in the repository, because a change to it is then reviewed like
  the code.
- A fix for a defect **MUST** arrive with a test that fails without it.

_**Covers:** [pol-AUTV].INTEG, [pol-AUTV].MACHINE, [pol-AUTV].REGRESS_

### BLOCK: A failing check stops the change

- Branch policy **MUST** block a merge while any required check is failing.
- A team **MUST NOT** merge over a failing check without a recorded deviation.
- A team **MUST NOT** skip, silence or suppress a check without a recorded deviation.
- A deviation covering a suppressed check **MUST** name the individual who accepts the risk.
- That deviation **MUST** have a review date.
- A team **MAY** guard a test off a platform it cannot run on without a deviation, where a required check runs the
  test on a platform where it can, and the guard states why.

_**Covers:** [pol-AUTV].BLOCK, [pol-AUTV].BYPASS, [pol-AUTV].DISABLE, [pol-DEVI].EXPIRY, [pol-DEVI].OWNER_

## Examples

```
Good
  # covers-api.test.ts
  test.skip("retries on 503")   // deviation dev-covers-retry-flaky, r.okafor, review 2026-10-01

Avoid
  test.skip("retries on 503")
```

Both stop the test running. The first records a name, a reason and a date, so somebody comes back to it. Nobody comes
back to the second until the retry path breaks in production.

## Conformance checklist

- [ ] A push triggers the build with no manual step.
- [ ] Branch policy lists the build and the test suite as required checks.
- [ ] The build passes from a clean clone on a fresh agent.
- [ ] Every skipped, excluded or suppressed check names a deviation.
- [ ] Every test guarded off a platform states why in the guard, and a required check runs it on a platform where it
      can.
- [ ] Every such deviation is open, owned by an individual, and inside its review date.
- [ ] The last defect fix in this repository has a test that fails without the fix.

## Rationale and provenance

A check anybody can waive without a record blocks nothing. Recording the waiver keeps the exception visible and gives
it an end date.

## Sources and further reading

- **Informative.** [SLSA build levels] describe what a build platform guarantees before a gate running on it means
  anything: a clean checkout, an agent defined in the repository, and provenance for what the build produced.

## Changelog

- 2026-10-20: a test guarded off a platform it cannot run on needs no deviation, where a required check runs it on a
  platform where it can and the guard states why.
- 2026-10-18: the build agent needs only to be nobody's own machine. Provisioning it from a definition in the
  repository becomes a `SHOULD`.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[pol-AUTV]: ../../policies/delivery/autv-automated-verification.md#clauses
[pol-DEVI]: ../../policies/governance/devi-deviations.md#clauses
[SLSA build levels]: https://slsa.dev/spec/v1.0/levels
