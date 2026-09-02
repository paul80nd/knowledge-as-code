---
id: pol-SECD
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.8, A.8.25, A.8.26, A.8.27, A.8.28 ]
  - framework: UK GDPR
    clauses: [ Art.35 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ secure-coding, secure-design, threat-modelling ]
---

# Security is designed in, not added on

`Policy: pol-SECD` `DRAFT`

## Purpose

We decide security and privacy at design time, and carry that decision through into how the code is written.

Retrofitted security is expensive and shallow. A flaw in a design is inherited by every line of code that implements it.
Scanning finds the symptoms afterwards. Half an hour of threat modelling at the start removes the cause.

## Scope

New systems, new features and significant changes to existing ones. The depth is proportionate to risk: a high-risk or
externally exposed change warrants more than a routine internal one, but neither warrants none.

_Boundary: this policy owns the security decisions we make while building: the requirements, the design, the threats we
work through, and how the code is written. Finding weaknesses in what is already built is [pol-VURM]'s. Running the
checks that look for them on every change is [pol-AUTV]'s. `ACTIONS` is shared with [pol-INCR]: findings become tracked
work whether they came from threat modelling here or from an incident review there._

## Clauses

| Id        | Clause                                                                                                                                                                  | Alignment                                                                                  |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| `REQS`    | **MUST** capture security and privacy requirements for new systems and features, as requirements rather than as afterthoughts                                           | [ISO 27001:2022].A.5.8, [ISO 27001:2022].A.8.26, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PO.1 |
| `DESIGN`  | **MUST** apply established secure-design principles: least privilege, defence in depth, secure defaults, and failing closed so that a failure denies rather than allows | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `THREAT`  | **MUST** think through how a significant new system or high-risk change could be attacked, and record what we found                                                     | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `IMPACT`  | **MUST** assess the impact on people before starting processing likely to be high risk to them                                                                          | [UK GDPR].Art.35                                                                           |
| `ACTIONS` | **MUST** turn the findings into tracked work rather than leaving them in a document. See [pol-INCR]                                                                     | [ISO 27001:2022].A.5.8                                                                     |
| `CODING`  | **MUST** write code to a documented secure-coding standard for the stack it is written in                                                                               | [ISO 27001:2022].A.8.28, [OWASP ASVS 4.0].V5, [NIST SSDF 1.1].PW.5                         |
| `CODEREV` | **MUST** review code for security and correctness, not only for style                                                                                                   | [ISO 27001:2022].A.8.28                                                                    |
| `HIRISK`  | **MUST NOT** take a high-risk change into build with no security requirements and no consideration of threat                                                            | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.26, [NIST SSDF 1.1].PO.1                     |

## Exceptions

Low-risk changes to systems that hold no sensitive data and have no external exposure need no separate threat
consideration. The secure-design principles and the coding standard still apply. A high-risk change may have to proceed
before its security work is complete. That is a recorded deviation under [pol-DEVI], not a judgement call made in the
moment.

[pol-AUTV]: ../delivery/autv-automated-verification.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-INCR]: ../operations/incr-incident-response.md
[pol-VURM]: ../security/vurm-vulnerability-remediation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../../frameworks.md#nist-ssdf
[OWASP ASVS 4.0]: ../../frameworks.md#owasp-asvs
[UK GDPR]: ../../frameworks.md#uk-gdpr
