---
id: rbk-diagnosis-never-escalates
type: runbook
tier: procedural
status: active
severity: sev3
owner: human:alex.doe
last-rehearsed: "never"
rehearsal-frequency: quarterly
---

# Connection pool runs dry

`Runbook: rbk-diagnosis-never-escalates` `ACTIVE`

## Symptoms

Requests time out waiting for a database connection.

## Impact

Reads and writes both queue while the pool is full.

`diagnosis-no-escalation` reports this diagnosis. Every branch routes, so no branch is a dead end, and none of them
reaches `Escalation`. A reader the tree does not answer has nowhere to go.

## Immediate actions

Page the on-call engineer.

## Diagnosis

**Is the pool size below the configured maximum?**

* **Yes** → raise the limit, then go to [Resolution](#resolution).
* **No** → continue.

**Is one query holding a connection open?**

* **Yes** → end that query, then go to [Resolution](#resolution).
* **No** → go to [Resolution](#resolution).

## Resolution

1. Raise the pool size to the agreed ceiling.
2. Watch the wait time fall.

Confirmed when requests stop timing out. If a step does not do what it says, [escalate](#escalation).

## Escalation

The database team.

## Communication

Tell the support channel once the wait time is back to normal.
