---
id: rbk-database-connections-exhausted
tier: procedural
status: active
owner: alex.doe
last-rehearsed: "never"
---

# Database connections exhausted

`Runbook: rbk-database-connections-exhausted` `ACTIVE`

## Immediate actions

Every section this type requires is present, so `required-section` is satisfied. What is wrong is the
order: the reader meets the fix before the symptom.

`last-rehearsed` carries the literal `"never"` that its schema admits. Nothing here reports it, and that silence is the
assertion: without `allow-literal` the value fails `date-format`, and this golden would gain a finding.

## Symptoms

Connection pool errors in the application log.

## Diagnosis

Check the pool size against the connection count.

## Resolution

Raise the pool ceiling, or find what is holding connections open.

## Escalation

The platform team.
