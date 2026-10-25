---
id: rbk-walked-by-a-process
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: { at: "2020-01-01", by: process:fault-injector, method: live }
rehearsal-frequency: quarterly
---

# Disk fills on the log volume

`Runbook: rbk-walked-by-a-process` `ACTIVE`

## Symptoms

Writes to the log volume fail and the service stops accepting requests.

## Impact

Every request is refused until the volume has space again.

`last-rehearsed` names a process as `by`, so `walked-by-a-person-or-agent` reports it. A fault a process injects with
nobody walking the steps is a control. The date is far enough past that `staleness-loud` reports it as well.

## Immediate actions

Rotate the logs.

## Diagnosis

Compare the volume's free space against the log rotation schedule.

## Resolution

Delete the rotated logs, then check the service accepts requests.

Confirmed when writes succeed and requests are accepted. If they are not, [escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel once requests are accepted.
