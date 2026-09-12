---
id: pol-OBSV
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.8.6, A.8.15, A.8.16, A.8.17 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ alerting, logging, monitoring, observability ]
---

# Systems are observable and actively monitored

`Policy: pol-OBSV` `DRAFT`

## Purpose

Our systems emit telemetry (logs, metrics and traces) as they run, and somebody watches it.

A failure recorded in telemetry nobody watches goes unnoticed. Monitoring with too little telemetry cannot explain a
failure. We need both.

## Scope

All production systems we operate, and the lower environments where validating a change means understanding how it
behaves.

_Boundary: this policy sets how long telemetry is kept, which is long enough to investigate. Where telemetry contains
personal data, [pol-DATA] sets the lifetime and the shorter of the two applies. [pol-DATA] and [pol-SCRT] govern what
is written into telemetry at all._

## Clauses

| Id        | Clause                                                                                                                       | Alignment                                                   |
|-----------|------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------|
| `CENTRAL` | **MUST** emit operational and security-relevant telemetry to a central store that the emitting system cannot alter or delete | [ISO 27001:2022].A.8.15, [Azure WAF].operational-excellence |
| `CLOCKS`  | **MUST** synchronise system clocks to a single approved time source, so events can be correlated into one timeline           | [ISO 27001:2022].A.8.17, [Azure WAF].operational-excellence |
| `RETAIN`  | **MUST** retain telemetry for a defined period: long enough to investigate, no longer than justified                         | [ISO 27001:2022].A.8.15, [Azure WAF].operational-excellence |
| `HEALTH`  | **MUST** monitor the availability and health of production systems, and alert an accountable owner when they degrade         | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `USAGE`   | **MUST** monitor resource use against the capacity a system has, and act before it runs out                                  | [ISO 27001:2022].A.8.6                                      |
| `SECMON`  | **MUST** monitor for security-relevant events, not only for availability                                                     | [ISO 27001:2022].A.8.16                                     |
| `ALERTS`  | **MUST** treat alerts as something to be acted on, and keep them few enough and meaningful enough that they still are        | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `BLIND`   | **MUST NOT** run a production system with no monitoring and no alerting                                                      | [ISO 27001:2022].A.8.16, [Azure WAF].operational-excellence |
| `SECRETS` | **MUST NOT** write secrets, credentials or unmasked sensitive personal data into telemetry. See [pol-SCRT] and [pol-DATA]    | [ISO 27001:2022].A.8.15                                     |
| `ESTATE`  | SHOULD keep a list of the production systems we run, so a system with no monitoring can be found                             |                                                             |
| `SLO`     | SHOULD express what good looks like as service-level objectives, and monitor against them                                    | [Azure WAF].reliability                                     |
| `CORREL`  | SHOULD emit telemetry that can be correlated across systems by a shared identifier, not only by time                         | [ISO 27001:2022].A.8.15                                     |
| `LOOP`    | SHOULD put production telemetry in front of the people who build a system, and not only in front of whoever operates it      |                                                             |

## Exceptions

The depth of monitoring is proportionate to how critical the system is. The commitment to monitor at all is not. Where
a system seems too unimportant to monitor, question whether it should exist.

[pol-DATA]: ../security/data-data-protection.md
[pol-SCRT]: ../security/scrt-secrets-are-never-embedded.md
[Azure WAF]: ../../frameworks.md#azure-well-architected-framework
[ISO 27001:2022]: ../../frameworks.md#iso-27001
