---
id: pol-MEXP
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.8.20, A.8.21, A.8.22, A.8.24 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ exposure, network-security, segmentation ]
---

# Exposure is minimised and traffic is controlled

`Policy: pol-MEXP` `DRAFT`

## Purpose

A system is reachable only from where it needs to be reachable, and it can reach only what it needs. We deny traffic
crossing a trust boundary unless we have deliberately allowed it.

Connectivity that exists because nobody removed it is the path an attacker takes after the first foothold. If every
route has to be asked for, an attacker who gets into one system finds nothing else it can reach.

## Scope

All networks, services and interfaces belonging to systems we build or operate, in every environment. It covers traffic
arriving at our systems, traffic between them, and traffic leaving them.

A trust boundary is the line between two things that have no automatic right to talk to each other: the public internet
and our estate, or two of our own systems with no business calling one another.

## Clauses

| Id        | Clause                                                                                                                              | Alignment                                        |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `SEGMENT` | **MUST** segment by trust boundary and by environment                                                                               | [ISO 27001:2022].A.8.22                          |
| `DENY`    | **MUST** deny by default, and expose a service only through an intended, controlled route                                           | [ISO 27001:2022].A.8.20, [ISO 27001:2022].A.8.21 |
| `TRANSIT` | **MUST** protect traffic crossing a trust boundary                                                                                  | [ISO 27001:2022].A.8.20                          |
| `PEERID`  | **MUST** verify the identity presented at the far end of a connection that crosses a trust boundary, and refuse it where that fails | [ISO 27001:2022].A.8.20, [ISO 27001:2022].A.8.24 |
| `PRIVATE` | **MUST** prefer private paths to public ones for access between our own services                                                    | [ISO 27001:2022].A.8.21                          |
| `EGRESS`  | **MUST** control and observe what leaves our systems, not only what arrives                                                         | [ISO 27001:2022].A.8.20                          |
| `ASCODE`  | **MUST** define network topology as reviewable code, under [pol-EVER]                                                               |                                                  |
| `PUBLIC`  | **MUST NOT** expose a management interface or a datastore directly to the public internet                                           | [ISO 27001:2022].A.8.21                          |
| `LATERAL` | **MUST NOT** leave traffic between our own systems unrestricted where they have no need to talk to each other                       | [ISO 27001:2022].A.8.22                          |
| `WEAKEN`  | **MUST NOT** disable, bypass or weaken identity verification at a trust boundary                                                    | [ISO 27001:2022].A.8.24                          |
| `ZEROTR`  | COULD authenticate and authorise every service-to-service call, so that network location grants no trust                            |                                                  |

## Exceptions

Some services are meant to be reached by anyone. They are in scope for the rest of this policy rather than exempt from
it. They are exposed through an intended route, and everything behind them stays private. Any other exposure requires a
recorded deviation under [pol-DEVI].

[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-EVER]: ../delivery/ever-everything-in-version-control.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
