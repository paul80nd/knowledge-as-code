---
id: pol-MEXP
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.8.20
  - ISO27001:2022 A.8.21
  - ISO27001:2022 A.8.22
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ exposure, network-security, segmentation ]
---

# `pol-MEXP` Exposure is minimised and traffic is controlled

## Purpose

A system is reachable only from where it needs to be reachable, and can itself reach only what it needs. Access across a
trust boundary is denied unless it has been deliberately allowed.

Connectivity that exists because nobody removed it is the path an attacker takes after the first foothold. Deciding
exposure deliberately, and denying by default, is what keeps a single compromise from becoming a general one.

## Scope

All networks, services and interfaces belonging to systems we build or operate, in every environment. Covers traffic
inbound to our systems, between them, and outbound from them.

## Commitments

* We **will** segment by trust boundary and by environment.
* We **will** deny by default, and expose a service only through an intended, controlled route.
* We **will** protect traffic crossing a trust boundary.
* We **will** prefer private paths to public ones for access between our own services.
* We **will** control and observe what leaves our systems, not only what arrives.
* We **will** define network topology as reviewable code, under [pol-EVER].
* We **will not** expose a management interface or a datastore directly to the public internet.
* We **will not** permit unrestricted lateral traffic between systems that have no need to talk to each other.

## Alignment

| Reference                 | Area                        |
|---------------------------|-----------------------------|
| ISO/IEC 27001:2022 A.8.20 | Network security            |
| ISO/IEC 27001:2022 A.8.21 | Security of network services |
| ISO/IEC 27001:2022 A.8.22 | Segregation of networks     |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Deliberately public services — those whose purpose is to be reached by anyone — are in scope for the rest of this
policy, not exempt from it: they are exposed through an intended route, and everything behind them stays private. Any
other exposure requires a recorded deviation under [pol-DEVI].

## Implemented by

Intended implementing standard: network security and segmentation.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until that standard id does._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-EVER]: ever-everything-in-version-control.md
