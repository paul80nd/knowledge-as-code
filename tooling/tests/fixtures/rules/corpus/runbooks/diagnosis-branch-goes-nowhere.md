---
id: rbk-diagnosis-branch-goes-nowhere
type: runbook
tier: procedural
status: active
severity: sev2
owner: human:alex.doe
last-rehearsed: "never"
rehearsal-frequency: quarterly
---

# Broker connection drops under load

`Runbook: rbk-diagnosis-branch-goes-nowhere` `ACTIVE`

## Symptoms

The consumer reconnects every few seconds and the queue depth climbs.

## Impact

Messages are read late while the consumer reconnects.

`escalation-required` reports two branches below. The first states an action and links nowhere. The last says
`continue` under the final question, which has nothing to fall through to. One branch reaches `Escalation`, so
`diagnosis-no-escalation` stays quiet.

## Immediate actions

Page the on-call engineer.

## Diagnosis

**Is the broker reachable?**

* **Yes** → restart the consumer.
* **No** → continue.

**Has the client certificate expired?**

* **Yes** → [escalate](#escalation).
* **No** → continue.

## Resolution

1. Restart the consumer.
2. Watch the queue depth fall.

Confirmed when the queue depth returns to its usual range. If a step does not do what it says,
[escalate](#escalation).

## Escalation

The platform team.

## Communication

Tell the support channel once the consumer is back.
