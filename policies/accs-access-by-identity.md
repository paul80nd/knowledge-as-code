---
id: pol-ACCS
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.15
  - ISO27001:2022 A.5.16
  - ISO27001:2022 A.5.18
  - ISO27001:2022 A.8.2
  - ISO27001:2022 A.8.3
  - ISO27001:2022 A.8.5
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ access-control, identity, least-privilege ]
---

# POL-ACCS: Access is by individual identity, on least privilege

## Purpose

Every access to a system, its code or its data is granted to an individual identity, authenticated strongly, limited to
what that person or workload actually needs, and reviewed as circumstances change.

Attribution is what makes everything else enforceable. A shared account converts an audit trail into a guess, and
standing privilege that nobody needs is simply a breach waiting for an attacker to find the credential.

## Scope

All systems, environments, source repositories, pipelines and data stores we build or operate, for people and for
machine identities alike. Applies to routine and privileged access.

## Commitments

* We **will** grant access to a named individual or a distinctly identified workload, never to a shared persona.
* We **will** grant the least privilege that allows the work to be done, and no more.
* We **will** require strong authentication for access to our systems, our code and our data.
* We **will** review access rights periodically, and remove them promptly when a role changes or a person leaves.
* We **will** control and record the use of privileged administrative tooling.
* We **will not** operate shared or generic privileged accounts where individual attribution is lost.
* We **will not** leave standing production access in place beyond what the role genuinely requires.

## Alignment

| Reference                 | Area                             |
|---------------------------|----------------------------------|
| ISO/IEC 27001:2022 A.5.15 | Access control                   |
| ISO/IEC 27001:2022 A.5.16 | Identity management              |
| ISO/IEC 27001:2022 A.5.18 | Access rights                    |
| ISO/IEC 27001:2022 A.8.2  | Privileged access rights         |
| ISO/IEC 27001:2022 A.8.3  | Information access restriction   |
| ISO/IEC 27001:2022 A.8.5  | Secure authentication            |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Break-glass access for incident response is permitted where an individual cannot otherwise act, provided the account is
attributable, its use is alerted and recorded, and its use is reviewed afterwards. Any other departure requires a
recorded deviation under [pol-DEVI].

## Implemented by

Intended implementing standards: identity and access management, and the access provisions of the source control
standard.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
