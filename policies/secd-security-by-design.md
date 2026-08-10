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
review-by: "2027-08-04"
owner: paul.law
tags: [ secure-coding, secure-design, threat-modelling ]
---

# Security is designed in, not added on

`Policy: pol-SECD` `DRAFT`

## Purpose

Security and privacy are decided at design time — as requirements, through recognised design principles, and by thinking
deliberately about how a system could be attacked — and then carried through into how the code is written.

Retrofitted security is expensive and shallow. A flaw in a design is inherited by every line of code that implements it,
and no amount of scanning later recovers what a half-hour of threat modelling would have prevented.

## Scope

New systems, new features and significant changes to existing ones. The depth is proportionate to risk: a high-risk or
externally exposed change warrants more than a routine internal one, but neither warrants none.

## Clauses

| Id        | Clause                                                                                                                                                                      | Alignment                                                                                  |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| `REQS`    | **MUST** capture security and privacy requirements for new systems and features, as requirements rather than as afterthoughts                                               | [ISO 27001:2022].A.5.8, [ISO 27001:2022].A.8.26, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PO.1 |
| `DESIGN`  | **MUST** apply established secure-design principles — least privilege, defence in depth, secure defaults, failing closed                                                    | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `THREAT`  | **MUST** think through how a significant new system or high-risk change could be attacked, and record what we found                                                         | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `IMPACT`  | **MUST** assess the impact on people before starting processing likely to be high risk to them                                                                              | [UK GDPR].Art.35                                                                           |
| `ACTIONS` | **MUST** turn the findings into tracked work rather than leaving them in a document                                                                                         | [ISO 27001:2022].A.5.8                                                                     |
| `CODING`  | **MUST** write code to a documented secure-coding standard appropriate to the stack it is written in, and treat review as covering correctness and security, not only style | [ISO 27001:2022].A.8.28, [OWASP ASVS 4.0].V5, [NIST SSDF 1.1].PW.5                         |
| `HIRISK`  | **MUST NOT** take a high-risk change into build with no security requirements and no consideration of threat                                                                | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.26, [NIST SSDF 1.1].PO.1                     |
| `LATE`    | **MUST NOT** rely on later testing to discover what design should have prevented                                                                                            | [ISO 27001:2022].A.8.25                                                                    |

## Exceptions

Low-risk changes to systems that hold no sensitive data and have no external exposure need no separate threat
consideration; the secure-design principles and coding standard still apply. Where a high-risk change must proceed
before its security work is complete, that is a recorded deviation under
[pol-DEVI], not a judgement call made in the moment.

[pol-DEVI]: devi-deviations-are-recorded.md
[ISO 27001:2022]: /frameworks.md#iso-27001
[NIST SSDF 1.1]: /frameworks.md#nist-ssdf
[OWASP ASVS 4.0]: /frameworks.md#owasp-asvs
[UK GDPR]: /frameworks.md#uk-gdpr
