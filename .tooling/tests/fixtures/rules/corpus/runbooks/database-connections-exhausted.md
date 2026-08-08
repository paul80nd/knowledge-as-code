---
id: rbk-database-connections-exhausted
tier: procedural
status: active
owner: alex.doe
last-rehearsed: "2026-05-01"
---

# Database connections exhausted

`Runbook: rbk-database-connections-exhausted` `ACTIVE`

## Immediate actions

Every section this type requires is present, so `required-section` is satisfied. What is wrong is the
order: the reader meets the fix before the symptom.

## Symptoms

Connection pool errors in the application log.

## Diagnosis

Check the pool size against the connection count.

## Resolution

Raise the pool ceiling, or find what is holding connections open.

## Escalation

The platform team.
