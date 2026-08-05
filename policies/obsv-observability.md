---
id: pol-OBSV
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.8.15
  - ISO27001:2022 A.8.16
  - ISO27001:2022 A.8.17
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - observability
  - logging
  - monitoring
  - alerting
---

# Policy: Systems are observable and actively monitored

## Purpose

Our systems tell us what they are doing. They emit enough protected, correlatable telemetry to explain their behaviour
after the fact, and we watch that telemetry closely enough to know about a problem before a customer reports it.

Telemetry nobody watches is storage, and monitoring without adequate telemetry is guesswork. Both halves are needed:
one reconstructs what happened, the other tells us it is happening now.

## Scope

All production systems we operate, and the lower environments where behaviour must be understood to validate a change.

## Commitments

* We **will** emit operational and security-relevant telemetry to a central store that the emitting system cannot alter
  or delete.
* We **will** keep timestamps consistent across systems, so events can be correlated into one timeline.
* We **will** retain telemetry for a defined period — long enough to investigate, no longer than justified.
* We **will** monitor the availability and health of production systems, and alert an accountable owner when they
  degrade.
* We **will** monitor for security-relevant events, not only for availability.
* We **will** treat alerts as something to be acted on, and keep them few enough and meaningful enough that they still
  are.
* We **will not** run a production system with no monitoring and no alerting.
* We **will not** write secrets, credentials or unmasked sensitive personal data into telemetry — see
  [pol-SCRT] and [pol-DATA].

## Alignment

| Reference                 | Area                  |
|---------------------------|-----------------------|
| ISO/IEC 27001:2022 A.8.15 | Logging               |
| ISO/IEC 27001:2022 A.8.16 | Monitoring activities |
| ISO/IEC 27001:2022 A.8.17 | Clock synchronisation |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

The depth of monitoring is proportionate to how critical the system is; the commitment to have some is not. A system
too unimportant to monitor is a system to question the existence of.

## Implemented by

Intended implementing standards: logging and observability, and monitoring and alerting.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DATA]: data-data-protection.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
