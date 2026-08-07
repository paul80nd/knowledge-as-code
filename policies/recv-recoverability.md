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

## Clauses

| Id        | Clause                                                                                                             | Alignment                                        |
|-----------|--------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `RTORPO`  | **MUST** define recovery-time and recovery-point objectives for critical systems, and design to meet them          | [ISO 27001:2022].A.5.30                          |
| `BACKUP`  | **MUST** back up critical data on a schedule that matches those objectives                                         | [ISO 27001:2022].A.8.13                          |
| `RESTORE` | **MUST** test restoration periodically — recovery is proven, not assumed                                           | [ISO 27001:2022].A.8.13                          |
| `OFFSITE` | **MUST** keep at least one copy of critical data outside the failure domain of its source                          | [ISO 27001:2022].A.8.13, [ISO 27001:2022].A.8.14 |
| `TIMEOUT` | **MUST** bound every outbound call in time, so a slow dependency cannot become an unbounded wait                   |                                                  |
| `DEGRADE` | **MUST** design so that the failure of one dependency degrades function rather than taking the system down with it | [ISO 27001:2022].A.5.29, [ISO 27001:2022].A.8.14 |
| `IDEMPOT` | **MUST** make operations that may be retried safe to re-run                                                        |                                                  |
| `UNTEST`  | **MUST NOT** rely on a backup that has never been test-restored                                                    | [ISO 27001:2022].A.8.13                          |
| `RETRY`   | **MUST NOT** retry indefinitely, without limit or backoff, against a failing dependency                            |                                                  |

## Exceptions

Systems holding no state that cannot be regenerated from source need no data backup — but their recovery path is still
defined and still exercised. Accepting a longer recovery objective than a system's criticality suggests is a recorded
deviation under [pol-DEVI], owned by whoever will answer for the downtime.

[pol-DEVI]: devi-deviations-are-recorded.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
