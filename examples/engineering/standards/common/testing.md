---
id: std-TEST
tier: normative
status: draft
implements:
  - pol-AUTV.COVER
  - pol-AUTV.LEVELS
  - pol-ENVS.DEBUG
  - pol-ENVS.MASK
  - pol-ENVS.UNMASK
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ test-data, test-levels, testing ]
---

# A test runs at the lowest level that can catch the fault

`Standard: std-TEST` `DRAFT`

## Summary

A team writes each test at the cheapest level that can fail on the fault it is looking for, and runs the whole suite on
data that was never a real customer's.

## Rules

### Pick the level from the fault

- A team **MUST** cover a rule about one unit's behaviour with a test on that unit alone ([pol-AUTV].LEVELS).
- A team **MUST** cover an agreement between two components with a test that exercises both ([pol-AUTV].LEVELS).
- A team **MUST** keep the tests that run on every push under ten minutes, so the answer arrives while the work is
  still open ([pol-AUTV].LEVELS).
- A test **MUST** fail for one reason, and its name **MUST** say what that reason is ([pol-AUTV].LEVELS).
- A test **MUST NOT** depend on the order the suite runs in, on the wall clock, or on a service somebody else deploys
  ([pol-AUTV].LEVELS).

### Know what the suite reaches

- A build **MUST** report line coverage, and **MUST** fail where coverage falls below the figure the repository records
  ([pol-AUTV].COVER).
- A team **MUST** read a coverage drop as a gap in the tests rather than as a threshold to lower ([pol-AUTV].COVER).

### The data is never a real customer's

- A test **MUST** run against generated data, or against production data a masking step has already been through
  ([pol-ENVS].MASK).
- A team **MUST NOT** copy unmasked production data into a test fixture, a seed script or a local database
  ([pol-ENVS].UNMASK).
- A team **MUST NOT** run a test, a script or a debugger against production ([pol-ENVS].DEBUG).

## Examples

```
Good
  RetryBudget_StopsAfterTwoAttempts
  CoversApi_Returns404_WhenTheIsbnIsUnknown

Avoid
  TestRetries
```

The first two name the subject and the expected outcome, so a red build says what broke before anyone opens the file.

```
Good
  var customer = TestData.Customer(name: "Alex Fenwick", email: "alex@example.com");

Avoid
  // seeded from a Tuesday export of the production customers table
```

The avoided fixture puts real names on every laptop that clones the repository, and a masking step run later does not
reach the clones.

## Conformance checklist

- [ ] The suite that runs on every push finishes in under ten minutes.
- [ ] Running the suite twice in a row, and in a different order, gives the same result.
- [ ] No test in the repository calls a hostname outside the build agent.
- [ ] The build publishes a coverage figure, and the threshold it fails at is in the repository.
- [ ] Every fixture in the repository is generated or masked, confirmed by reading it.
- [ ] No test configuration names a production connection string or endpoint.

## Rationale and provenance

A fault caught by a unit test costs the minute it takes to read the failure. The same fault caught by an end-to-end
test costs a triage, and caught in production it costs an incident.

- [pol-AUTV] commits us to testing at the levels a change warrants, and to knowing what the tests reach.
- [pol-ENVS] commits us to keeping production data and production systems out of the work below them.

[pol-AUTV]: ../../policies/delivery/autv-automated-verification.md#clauses
[pol-ENVS]: ../../policies/security/envs-environment-separation.md#clauses
