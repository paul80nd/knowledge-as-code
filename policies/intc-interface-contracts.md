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

An interface we publish is a promise to whoever depends on it. It is defined explicitly, it is secured by default, and
it does not change under its consumers without a version and reasonable notice.

Every integration is built against the behaviour an interface had on the day it was written. Breaking that quietly
transfers our convenience into someone else's outage, and the cost lands hardest on the consumers least able to respond
quickly.

## Scope

Interfaces we publish for consumption beyond the team that owns them: public APIs, internal service-to-service
interfaces, webhooks and any other integration surface others build against.

## Clauses

| Id        | Clause                                                                                                                                                 |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `SPEC`    | **MUST** define each interface against an explicit, documented contract, and treat that contract as the source of truth rather than the implementation |
| `VERSION` | **MUST** version interfaces so that a change we need does not become a break others must absorb                                                        |
| `DEPREC`  | **MUST** publish a deprecation approach and give notice before removing or changing something consumers depend on                                      |
| `SECURE`  | **MUST** secure interfaces by default — authenticated, authorised, and validating what they are given                                                  |
| `VERIFY`  | **MUST** verify our contracts hold, rather than trusting that they do                                                                                  |
| `BREAK`   | **MUST NOT** make a breaking change to a published interface without a version increment and notice to its consumers                                   |
| `EXPOSE`  | **MUST NOT** expose sensitive data or a sensitive operation through an unauthenticated interface                                                       |

## Alignment

| Reference                 | Area                                       |
|---------------------------|--------------------------------------------|
| ISO/IEC 27001:2022 A.8.26 | Application security requirements          |
| ISO/IEC 27001:2022 A.8.27 | Secure system architecture and engineering |

We **align with** these areas — they cover the security half of this policy. The interoperability half is engineering
practice with no corresponding control. We are not registered against ISO/IEC 27001:2022 and are not audited against it.

## Exceptions

An interface with a single known consumer inside the team that owns it may evolve by agreement rather than by version,
provided both sides genuinely know every consumer. That assumption is what usually turns out to be wrong, so it is worth
checking before relying on it. A security fix may break a contract where leaving it intact would leave data exposed;
consumers are told as soon as it is safe to tell them.
