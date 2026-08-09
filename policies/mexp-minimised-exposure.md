---
id: pol-MEXP
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.8.20
  - ISO27001:2022 A.8.21
  - ISO27001:2022 A.8.22
  - ISO27001:2022 A.8.24
review-by: "2027-08-04"
owner: paul.law
tags: [ exposure, network-security, segmentation ]
---

# Exposure is minimised and traffic is controlled

`Policy: pol-MEXP` `DRAFT`

## Purpose

A system is reachable only from where it needs to be reachable, and can itself reach only what it needs. Access across a
trust boundary is denied unless it has been deliberately allowed.

Connectivity that exists because nobody removed it is the path an attacker takes after the first foothold. Deciding
exposure deliberately, and denying by default, is what keeps a single compromise from becoming a general one.

## Scope

All networks, services and interfaces belonging to systems we build or operate, in every environment. Covers traffic
inbound to our systems, between them, and outbound from them.

## Clauses

| Id        | Clause                                                                                                                              | Alignment                                        |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `SEGMENT` | **MUST** segment by trust boundary and by environment                                                                               | [ISO 27001:2022].A.8.22                          |
| `DENY`    | **MUST** deny by default, and expose a service only through an intended, controlled route                                           | [ISO 27001:2022].A.8.20, [ISO 27001:2022].A.8.21 |
| `TRANSIT` | **MUST** protect traffic crossing a trust boundary                                                                                  | [ISO 27001:2022].A.8.20                          |
| `VERIFY`  | **MUST** verify the identity presented at the far end of a connection that crosses a trust boundary, and refuse it where that fails | [ISO 27001:2022].A.8.20, [ISO 27001:2022].A.8.24 |
| `PRIVATE` | **MUST** prefer private paths to public ones for access between our own services                                                    | [ISO 27001:2022].A.8.21                          |
| `EGRESS`  | **MUST** control and observe what leaves our systems, not only what arrives                                                         | [ISO 27001:2022].A.8.20                          |
| `ASCODE`  | **MUST** define network topology as reviewable code, under [pol-EVER]                                                               |                                                  |
| `PUBLIC`  | **MUST NOT** expose a management interface or a datastore directly to the public internet                                           | [ISO 27001:2022].A.8.21                          |
| `LATERAL` | **MUST NOT** permit unrestricted lateral traffic between systems that have no need to talk to each other                            | [ISO 27001:2022].A.8.22                          |
| `UNVERIF` | **MUST NOT** disable, bypass or weaken that identity verification                                                                   | [ISO 27001:2022].A.8.24                          |
| `ZEROTR`  | COULD authenticate and authorise every service-to-service call, so that network location grants no trust _(aspirational)_           |                                                  |

## Exceptions

Deliberately public services — those whose purpose is to be reached by anyone — are in scope for the rest of this
policy, not exempt from it: they are exposed through an intended route, and everything behind them stays private. Any
other exposure requires a recorded deviation under [pol-DEVI].

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-EVER]: ever-everything-in-version-control.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
