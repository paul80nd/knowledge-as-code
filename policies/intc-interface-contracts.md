---
id: pol-INTC
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.26
  - ISO27001:2022 A.8.27
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ api, contracts, interoperability, versioning ]
---

# Policy: Interfaces are contracts we honour

## Purpose

An interface we publish is a promise to whoever depends on it. It is defined explicitly, it is secured by default, and
it does not change under its consumers without a version and reasonable notice.

Every integration is built against the behaviour an interface had on the day it was written. Breaking that quietly
transfers our convenience into someone else's outage, and the cost lands hardest on the consumers least able to
respond quickly.

## Scope

Interfaces we publish for consumption beyond the team that owns them: public APIs, internal service-to-service
interfaces, webhooks and any other integration surface others build against.

## Commitments

* We **will** define each interface against an explicit, documented contract, and treat that contract as the source of
  truth rather than the implementation.
* We **will** version interfaces so that a change we need does not become a break others must absorb.
* We **will** publish a deprecation approach and give notice before removing or changing something consumers depend on.
* We **will** secure interfaces by default — authenticated, authorised, and validating what they are given.
* We **will** verify our contracts hold, rather than trusting that they do.
* We **will not** make a breaking change to a published interface without a version increment and notice to its
  consumers.
* We **will not** expose sensitive data or a sensitive operation through an unauthenticated interface.

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

## Implemented by

Intended implementing standard: API design and lifecycle.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until that standard id does._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
