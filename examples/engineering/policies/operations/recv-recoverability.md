---
id: pol-RECV
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.30, A.8.13, A.8.14 ]
  - framework: UK GDPR
    clauses: [ Art.32(1)(d) ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ backup, continuity, recovery, resilience ]
---

# Services and data are recoverable

`Policy: pol-RECV` `DRAFT`

## Purpose

We know how quickly each critical system has to come back, and how much data we can afford to lose. We can show that we
meet those objectives. When a dependency fails, our systems lose some function but keep running.

Resilience that has never been exercised is an assumption. A backup nobody has restored is not a backup. A dependency
with no failure path takes the system down when it fails.

## Scope

All systems we operate and the data they contain, with depth set by how critical the system is. It covers designed-in
fault tolerance and the ability to recover after failure.

A failure domain is the set of things that fail together: a disk, a rack, a data centre, a region.

Proving a restore needs the data the backup actually contains, so it runs in a controlled restore environment at
production tier: the same access controls and the same data handling as production, and not an environment below
production. Masked or synthesised data proves the mechanism and not the restore. [pol-DATA] and [pol-ENVS] bind that
environment as production, and it is not an exception to either.

_Boundary: [pol-ENVS] governs separation between environments, and [pol-DATA] governs the handling of the data they
contain. The restore environment is at production tier, so their rules about lower environments do not apply to it.
[pol-INCR] owns the response to an outage: deciding to invoke a recovery and running it. This policy owns being able to
recover._

## Clauses

| Id        | Clause                                                                                                                         | Alignment                                                                 |
|-----------|--------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| `RTORPO`  | **MUST** define recovery-time and recovery-point objectives for critical systems, and design to meet them                      | [ISO 27001:2022].A.5.30, [Azure WAF].reliability                          |
| `BACKUP`  | **MUST** back up critical data on a schedule that matches those objectives                                                     | [ISO 27001:2022].A.8.13, [Azure WAF].reliability                          |
| `RESTORE` | **MUST** test restoration periodically                                                                                         | [ISO 27001:2022].A.8.13, [Azure WAF].reliability, [UK GDPR].Art.32(1)(d)  |
| `OFFSITE` | **MUST** keep at least one copy of critical data outside the failure domain of its source                                      | [ISO 27001:2022].A.8.13, [ISO 27001:2022].A.8.14, [Azure WAF].reliability |
| `TIMEOUT` | **MUST** bound every outbound call in time, so a slow dependency cannot become an unbounded wait                               | [Azure WAF].reliability                                                   |
| `DEGRADE` | **MUST** design so that the failure of one dependency degrades function rather than taking the system down with it             | [ISO 27001:2022].A.8.14, [Azure WAF].reliability                          |
| `IDEMPOT` | **MUST** make operations that may be retried safe to re-run                                                                    | [Azure WAF].reliability                                                   |
| `UNTEST`  | **MUST NOT** rely on a backup that has never been test-restored                                                                | [ISO 27001:2022].A.8.13, [Azure WAF].reliability                          |
| `FAILOVR` | **MUST NOT** rely on a failover that has never been tested                                                                     | [ISO 27001:2022].A.8.14                                                   |
| `RETRY`   | **MUST NOT** retry indefinitely, without limit or backoff, against a failing dependency                                        | [Azure WAF].reliability                                                   |
| `REDUND`  | SHOULD run critical services across more than one failure domain                                                               | [ISO 27001:2022].A.8.14                                                   |
| `SHED`    | SHOULD shed or slow work deliberately when load exceeds capacity, turning some requests away rather than failing unpredictably | [Azure WAF].reliability                                                   |
| `CHAOS`   | COULD prove resilience by causing failure deliberately, rather than waiting to observe one                                     | [Azure WAF].reliability                                                   |

## Exceptions

A system whose whole state can be regenerated from source needs no data backup. Its recovery path is still defined and
still exercised. Accepting a longer recovery objective than a system's criticality suggests is a recorded deviation
under [pol-DEVI], owned by whoever will answer for the downtime.

[pol-DATA]: ../security/data-data-protection.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-ENVS]: ../security/envs-environment-separation.md
[pol-INCR]: ../operations/incr-incident-response.md
[Azure WAF]: ../../frameworks.md#azure-well-architected-framework
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[UK GDPR]: ../../frameworks.md#uk-gdpr
