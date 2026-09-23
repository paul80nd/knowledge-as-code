---
id: rbk-resolution-with-no-outcome
type: runbook
tier: procedural
status: active
severity: sev3
owner: human:alex.doe
last-rehearsed: "never"
rehearsal-frequency: quarterly
---

# Nightly backup job fails

`Runbook: rbk-resolution-with-no-outcome` `ACTIVE`

## Symptoms

The nightly backup job exits non-zero and no archive appears in the backup store.

## Impact

Yesterday's data has no backup until the job succeeds.

`outcome-stated` reports the resolution below. It gives the steps and a route out, so `failure-route-stated`
passes on the escalation link. What is missing is the line telling a reader who ran the steps when to stop.

## Immediate actions

Check the backup store for the most recent archive.

## Diagnosis

**Did the job reach the backup store?**

* **Yes** → clear the partial archive, then go to [Resolution](#resolution).
* **No** → [escalate](#escalation).

## Resolution

1. Delete the partial archive.
2. Rerun the backup job.

If the rerun fails the same way, [escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel once an archive lands.
