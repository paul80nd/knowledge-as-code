---
id: pol-RECV
tier: normative
category: operations
status: draft
aligns-with:
  - ISO27001:2022 A.5.29
  - ISO27001:2022 A.5.30
  - ISO27001:2022 A.8.13
  - ISO27001:2022 A.8.14
review-by: "2027-08-04"
owner: paul.law
tags: [ backup, continuity, recovery, resilience ]
---

# Services and data are recoverable

`Policy: pol-RECV` `DRAFT`

## Purpose

We know how quickly each critical system must come back and how much data we can afford to lose, we can demonstrate that
we can meet those objectives, and our systems degrade rather than collapse when something they depend on fails.

Resilience that has never been exercised is an assumption. A backup that has never been restored is not a backup, and a
dependency with no failure path is an outage waiting for its trigger.

## Scope

All systems we operate and the data they hold, with depth set by how critical the system is. Covers both designed-in
fault tolerance and the ability to recover after failure.

## Commitments

* We **will** define recovery-time and recovery-point objectives for critical systems, and design to meet them.
* We **will** back up critical data on a schedule that matches those objectives.
* We **will** test restoration periodically — recovery is proven, not assumed.
* We **will** keep at least one copy of critical data outside the failure domain of its source.
* We **will** bound every outbound call in time, so a slow dependency cannot become an unbounded wait.
* We **will** design so that the failure of one dependency degrades function rather than taking the system down with it.
* We **will** make operations that may be retried safe to re-run.
* We **will not** rely on a backup that has never been test-restored.
* We **will not** retry indefinitely, without limit or backoff, against a failing dependency.

## Alignment

| Reference                 | Area                                            |
|---------------------------|-------------------------------------------------|
| ISO/IEC 27001:2022 A.5.29 | Information security during disruption          |
| ISO/IEC 27001:2022 A.5.30 | ICT readiness for business continuity           |
| ISO/IEC 27001:2022 A.8.13 | Information backup                              |
| ISO/IEC 27001:2022 A.8.14 | Redundancy of information processing facilities |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Systems holding no state that cannot be regenerated from source need no data backup — but their recovery path is still
defined and still exercised. Accepting a longer recovery objective than a system's criticality suggests is a recorded
deviation under [pol-DEVI], owned by whoever will answer for the downtime.

[pol-DEVI]: devi-deviations-are-recorded.md
