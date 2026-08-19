---
id: pol-OBSV
tier: normative
category: operations
status: draft
aligns-with:
  - ISO27001:2022 A.8.15
  - ISO27001:2022 A.8.16
  - ISO27001:2022 A.8.17
review-by: "2027-08-04"
owner: paul.law
tags: [ alerting, logging, monitoring, observability ]
---

# Systems are observable and actively monitored

`Policy: pol-OBSV` `DRAFT`

## Purpose

Our systems tell us what they are doing. They emit enough telemetry — logs, metrics and traces — to explain their
behaviour afterwards. It is protected, and tied together so events in different systems line up on one timeline. We
watch it closely enough to know about a problem before a customer reports it.

Telemetry nobody watches is storage, and monitoring without adequate telemetry is guesswork. Both halves are needed:
one reconstructs what happened, the other tells us it is happening now.

## Scope

All production systems we operate, and the lower environments where behaviour must be understood to validate a change.

_Boundary: this policy sets how long telemetry is kept to stay useful — long enough to investigate. Where that telemetry
contains personal data, [pol-DATA] sets the lifetime and the shorter of the two governs. What may be written into
telemetry at all is [pol-DATA]'s and [pol-SCRT]'s._

## Clauses

| Id        | Clause                                                                                                                       | Alignment                                                   |
|-----------|------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| `CENTRAL` | **MUST** emit operational and security-relevant telemetry to a central store that the emitting system cannot alter or delete | [ISO 27001:2022].A.8.15, [Azure WAF].operational-excellence |
| `CLOCKS`  | **MUST** keep timestamps consistent across systems, so events can be correlated into one timeline                            | [ISO 27001:2022].A.8.17, [Azure WAF].operational-excellence |
| `RETAIN`  | **MUST** retain telemetry for a defined period — long enough to investigate, no longer than justified                        | [ISO 27001:2022].A.8.15, [Azure WAF].operational-excellence |
| `HEALTH`  | **MUST** monitor the availability and health of production systems, and alert an accountable owner when they degrade         | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `SECMON`  | **MUST** monitor for security-relevant events, not only for availability                                                     | [ISO 27001:2022].A.8.16                                     |
| `ALERTS`  | **MUST** treat alerts as something to be acted on, and keep them few enough and meaningful enough that they still are        | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `BLIND`   | **MUST NOT** run a production system with no monitoring and no alerting                                                      | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `SECRETS` | **MUST NOT** write secrets, credentials or unmasked sensitive personal data into telemetry — see [pol-SCRT] and [pol-DATA]   | [ISO 27001:2022].A.8.15                                     |
| `SLO`     | SHOULD express what good looks like as service-level objectives, and monitor against them                                    | [Azure WAF].reliability                                     |
| `CORREL`  | SHOULD emit telemetry that can be correlated across systems by a shared identifier, not only by time                         | [ISO 27001:2022].A.8.15                                     |

## Exceptions

The depth of monitoring is proportionate to how critical the system is; the commitment to have some is not. A system too
unimportant to monitor is a system to question the existence of.

[pol-DATA]: data-data-protection.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[Azure WAF]: /frameworks.md#azure-well-architected-framework
[ISO 27001:2022]: /frameworks.md#iso-27001
