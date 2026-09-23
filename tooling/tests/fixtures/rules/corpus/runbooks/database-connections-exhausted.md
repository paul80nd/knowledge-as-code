---
id: rbk-database-connections-exhausted
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: "never"
rehearsal-frequency: quarterly
---

# Database connections exhausted

`Runbook: rbk-database-connections-exhausted` `ACTIVE`

## Immediate actions

Every section this type requires is present, so `required-section` is satisfied. What is wrong is the
order: the reader meets the fix before the symptom.

`last-rehearsed` states the literal `"never"` that its schema admits, so `date-format` stays silent on it. What
reports it is `staleness-loud`, which is the `never` arm of that rule.

## Symptoms

Connection pool errors in the application log.

## Impact

Requests that need the database fail while the pool is empty.

## Diagnosis

Check the pool size against the connection count.

## Resolution

Raise the pool ceiling, or find what is holding connections open.

Confirmed when the pool has free connections and the errors stop. If neither works, [escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel every thirty minutes until the pool recovers.
