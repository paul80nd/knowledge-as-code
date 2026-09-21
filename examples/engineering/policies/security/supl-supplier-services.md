---
id: pol-SUPL
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.22, A.5.23 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ cloud, service-management, third-party ]
---

# Third-party services are chosen deliberately, and watched

`Policy: pol-SUPL` `DRAFT`

## Purpose

We choose a third-party or cloud service with its risks understood, not for its features alone, and we keep checking
that the choice still holds.

Adopting a service hands part of our security posture to someone else. Where we have not asked who holds which
responsibility, where our data sits, or how we would leave, we find out only when something goes wrong. A supplier's
own incident, or a silent change to what we depend on, stays invisible unless something is watching for it.

## Scope

Every third-party or cloud service a system we build or operate depends on: infrastructure, platform or software
delivered as a service, whether it holds our data, performs a function we chose not to build, or sits behind an API we
call.

_Boundary: [pol-TRUS] owns a component or artefact we ship: where it came from and what it is made of. This policy
owns a service we depend on but do not ship anything of. [pol-ACCS].TERM covers removing a third party's access when
that relationship ends; this policy covers the relationship itself, before and during it._

## Clauses

| Id        | Clause                                                                                                                                           | Alignment               |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `CLOUD`   | **MUST** establish which security responsibilities we hold and which the provider holds, before adopting a service                               | [ISO 27001:2022].A.5.23 |
| `LOCATE`  | **MUST** establish where a cloud provider will store our data, and who at the provider can access it, before adopting the service                | [ISO 27001:2022].A.5.23 |
| `CHANNEL` | **MUST** establish how a cloud provider will notify us of a security incident, before adopting the service                                       | [ISO 27001:2022].A.5.23 |
| `EXIT`    | **MUST** know how we would leave a service before we depend on it                                                                                | [ISO 27001:2022].A.5.23 |
| `REVIEW`  | **MUST** review the services we depend on periodically, checking at minimum for a disclosed security incident or near miss since the last review | [ISO 27001:2022].A.5.22 |
| `SUPCHG`  | **MUST** notice when a service we depend on changes under us, and decide what it means for us                                                    | [ISO 27001:2022].A.5.22 |

## Exceptions

A service that cannot be fully assessed before adoption, or that we cannot yet leave, is kept only under a recorded
deviation ([pol-DEVI]). The deviation names the risk owner, the compensating controls and the plan to close the gap.

[pol-ACCS]: ../security/accs-access-by-identity.md#clauses
[pol-DEVI]: ../governance/devi-deviations.md
[pol-TRUS]: ../security/trus-trusted-components.md#clauses
[ISO 27001:2022]: ../../frameworks.md#iso-27001
