---
id: rbk-rehearsal-method-not-stated
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: "2020-01-01"
rehearsal-frequency: quarterly
---

# Queue consumer stops draining

`Runbook: rbk-rehearsal-method-not-stated` `ACTIVE`

## Symptoms

Queue depth climbs and the consumer logs nothing.

## Impact

Work queued after the stall is not processed.

`last-rehearsed` states a day, so `rehearsal-method` is required and this record omits it. The date is far enough
past that `staleness-loud` reports it as well, which is the window arm of that rule. The runbooks beside this one
stating `"never"` pin the other arm.

## Immediate actions

Restart the consumer.

## Diagnosis

Compare the queue depth against the consumer's processed count.

## Resolution

Restart the consumer, then check the depth falls.

Confirmed when the depth falls and stays down. If it does not, [escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel once the depth is falling.
