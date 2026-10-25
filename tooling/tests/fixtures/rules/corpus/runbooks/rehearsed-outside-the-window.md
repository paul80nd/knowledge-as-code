---
id: rbk-rehearsed-outside-the-window
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: { at: "2020-01-01", by: human:alex.doe, method: tabletop }
rehearsal-frequency: quarterly
---

# Cache node runs out of memory

`Runbook: rbk-rehearsed-outside-the-window` `ACTIVE`

## Symptoms

Eviction rate climbs and read latency doubles.

## Impact

Reads that miss the cache reach the database and take longer.

`method` is stated, so `walk-method-stated` is silent here and reports against the runbook beside this one. That
pair is what pins both sides of the rule. `staleness-loud` still reports the date, because a method says which walk
happened and not when.

## Immediate actions

Raise the eviction threshold.

## Diagnosis

Compare the working set against the node's memory ceiling.

## Resolution

Add a node, or shrink what is cached.

Confirmed when the eviction rate falls and latency returns. If neither works, [escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel while latency is raised.
