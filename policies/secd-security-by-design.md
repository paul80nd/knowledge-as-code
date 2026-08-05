---
id: pol-SECD
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.8
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.26
  - ISO27001:2022 A.8.27
  - ISO27001:2022 A.8.28
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ secure-coding, secure-design, threat-modelling ]
---

# `pol-SECD` Security is designed in, not added on

## Purpose

Security and privacy are decided at design time — as requirements, through recognised design principles, and by
thinking deliberately about how a system could be attacked — and then carried through into how the code is written.

Retrofitted security is expensive and shallow. A flaw in a design is inherited by every line of code that implements it,
and no amount of scanning later recovers what a half-hour of threat modelling would have prevented.

## Scope

New systems, new features and significant changes to existing ones. The depth is proportionate to risk: a high-risk or
externally exposed change warrants more than a routine internal one, but neither warrants none.

## Commitments

* We **will** capture security and privacy requirements for new systems and features, as requirements rather than as
  afterthoughts.
* We **will** apply established secure-design principles — least privilege, defence in depth, secure defaults, failing
  closed.
* We **will** think through how a significant new system or high-risk change could be attacked, and record what we
  found.
* We **will** turn the findings into tracked work rather than leaving them in a document.
* We **will** write code to a documented secure-coding standard appropriate to the stack it is written in, and treat
  review as covering correctness and security, not only style.
* We **will not** take a high-risk change into build with no security requirements and no consideration of threat.
* We **will not** rely on later testing to discover what design should have prevented.

## Alignment

| Reference                 | Area                                        |
|---------------------------|---------------------------------------------|
| ISO/IEC 27001:2022 A.5.8  | Information security in project management  |
| ISO/IEC 27001:2022 A.8.25 | Secure development lifecycle                |
| ISO/IEC 27001:2022 A.8.26 | Application security requirements           |
| ISO/IEC 27001:2022 A.8.27 | Secure system architecture and engineering  |
| ISO/IEC 27001:2022 A.8.28 | Secure coding                               |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Low-risk changes to systems that hold no sensitive data and have no external exposure need no separate threat
consideration; the secure-design principles and coding standard still apply. Where a high-risk change must proceed
before its security work is complete, that is a recorded deviation under
[pol-DEVI], not a judgement call made in the moment.

## Implemented by

Intended implementing standards: threat modelling and secure design, secure coding and code quality, and the security
provisions of the API standard.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
