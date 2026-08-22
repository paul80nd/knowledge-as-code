---
id: pol-INTC
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.26
  - ISO27001:2022 A.8.27
review-by: "2027-08-04"
owner: paul.law
tags: [ api, contracts, interoperability, versioning ]
---

# Interfaces are contracts we honour

`Policy: pol-INTC` `DRAFT`

## Purpose

An interface we publish is a promise to whoever depends on it. We define it explicitly and secure it by default. We do
not change it under its consumers without a version and reasonable notice.

Every integration is built against the behaviour an interface had on the day it was written. Change it without warning
and we save ourselves an afternoon while costing someone else a day. That cost lands hardest on the consumers least able
to respond quickly.

## Scope

Interfaces we publish for consumption beyond the team that owns them: public APIs, internal service-to-service
interfaces, webhooks and any other integration surface others build against.

## Clauses

| Id        | Clause                                                                                                                                                 | Alignment                                     |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| `SPEC`    | **MUST** define each interface against an explicit, documented contract, and treat that contract as the source of truth rather than the implementation | [ISO 27001:2022].A.8.27                       |
| `VERSION` | **MUST** version interfaces so that a change we need does not become a break others must absorb                                                        | [ISO 27001:2022].A.8.27                       |
| `DEPREC`  | **MUST** publish a deprecation approach and give notice before removing or changing something consumers depend on                                      |                                               |
| `SECURE`  | **MUST** secure interfaces by default: authenticated, authorised, and validating what they are given                                                  | [ISO 27001:2022].A.8.26, [OWASP ASVS 4.0].V13 |
| `HOLDS`   | **MUST** verify our contracts hold, rather than trusting that they do                                                                                  | [ISO 27001:2022].A.8.26                       |
| `BREAK`   | **MUST NOT** make a breaking change to a published interface without a version increment and notice to its consumers                                   |                                               |
| `EXPOSE`  | **MUST NOT** expose sensitive data or a sensitive operation through an unauthenticated interface                                                       | [ISO 27001:2022].A.8.26, [OWASP ASVS 4.0].V4  |

## Exceptions

An interface with a single consumer inside the team that owns it falls outside the Scope above rather than being
excepted. It comes into scope the moment anyone beyond that team builds against it. Teams are usually wrong about who is
already calling them.

A security fix may break a contract where leaving it intact would leave data exposed. That is a recorded deviation under
[pol-DEVI], and consumers are told as soon as it is safe to tell them.

[pol-DEVI]: devi-deviations-are-recorded.md
[ISO 27001:2022]: ../frameworks.md#iso-27001
[OWASP ASVS 4.0]: ../frameworks.md#owasp-asvs
