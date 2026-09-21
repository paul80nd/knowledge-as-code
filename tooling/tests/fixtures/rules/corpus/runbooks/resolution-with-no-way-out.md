---
id: rbk-resolution-with-no-way-out
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: "never"
rehearsal-frequency: quarterly
---

# Queue consumer will not start

`Runbook: rbk-resolution-with-no-way-out` `ACTIVE`

## Symptoms

The consumer exits at startup and the queue depth climbs.

## Impact

Nothing is read off the queue while the consumer is down.

`failure-route-stated` reports the resolution below. The diagnosis links to `Escalation` and the resolution does not,
so a reader whose restart did not work has nowhere to go. The link the rule looks for is bounded to `Resolution`.

## Immediate actions

Page the on-call engineer.

## Diagnosis

**Is the broker reachable?**

* **Yes** → restart the consumer.
* **No** → [escalate](#escalation).

## Resolution

1. Restart the consumer.
2. Watch the queue depth fall.

Confirmed when the queue depth returns to its usual range.

## Escalation

The platform team.

## Communication

Tell the support channel once the consumer is back.
