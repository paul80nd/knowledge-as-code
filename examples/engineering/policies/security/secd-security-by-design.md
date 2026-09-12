---
id: pol-SECD
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.8, A.8.25, A.8.26, A.8.27, A.8.28 ]
  - framework: UK GDPR
    clauses: [ Art.35 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ secure-coding, secure-design, threat-modelling ]
---

# Security is designed in, not added on

`Policy: pol-SECD` `DRAFT`

## Purpose

We decide security and privacy at design time, and apply that decision in how the code is written.

Retrofitted security is expensive and shallow. Every line of code that implements a flawed design inherits the flaw.
Scanning finds the symptoms afterwards. Threat modelling at the start removes the cause.

## Scope

New systems, new features and significant changes to existing ones. The depth is proportionate to risk. A high-risk or
externally exposed change gets more attention than a routine internal one. Every change in scope gets some.

_Boundary: this policy owns the security decisions we make while building: the requirements, the design, the threats we
work through, and how the code is written. [pol-VURM] owns finding weaknesses in what is already built. [pol-AUTV] owns
running the checks that look for them on every change. `ACTIONS` is shared with [pol-INCR]: findings become tracked
work whether threat modelling here or an incident review there produced them._

## Clauses

| Id        | Clause                                                                                                                                                                  | Alignment                                                                                  |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| `REQS`    | **MUST** capture security and privacy requirements for new systems and features, as requirements rather than as afterthoughts                                           | [ISO 27001:2022].A.5.8, [ISO 27001:2022].A.8.26, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PO.1 |
| `DESIGN`  | **MUST** apply established secure-design principles: least privilege, defence in depth, secure defaults, and failing closed so that a failure denies rather than allows | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `THREAT`  | **MUST** think through how a significant new system or high-risk change could be attacked, and record what we found                                                     | [ISO 27001:2022].A.8.27, [OWASP ASVS 4.0].V1, [NIST SSDF 1.1].PW.1                         |
| `IMPACT`  | **MUST** assess the impact on people before starting processing likely to be high risk to them, and record what the assessment found                                    | [UK GDPR].Art.35                                                                           |
| `ACTIONS` | **MUST** turn the findings into tracked work rather than leaving them in a document. See [pol-INCR]                                                                     | [ISO 27001:2022].A.5.8                                                                     |
| `CODING`  | **MUST** write code to a documented secure-coding standard for the stack it is written in                                                                               | [ISO 27001:2022].A.8.28, [NIST SSDF 1.1].PW.5                                              |
| `CODEREV` | **MUST** review code for security and correctness, not only for style                                                                                                   | [ISO 27001:2022].A.8.28, [NIST SSDF 1.1].PW.7                                              |
| `HIRISK`  | **MUST NOT** take a high-risk change into build with no security requirements and no consideration of threat                                                            | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.26, [NIST SSDF 1.1].PO.1                     |

## Exceptions

A low-risk change to a system that contains no sensitive data and has no external exposure needs no separate threat
consideration. The secure-design principles and the coding standard still apply. A high-risk change sometimes has to
proceed before its security work is complete. That is a recorded deviation under [pol-DEVI], not a judgement call made
in the moment.

[pol-AUTV]: ../delivery/autv-automated-verification.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-INCR]: ../operations/incr-incident-response.md
[pol-VURM]: ../security/vurm-vulnerability-remediation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../../frameworks.md#nist-ssdf
[OWASP ASVS 4.0]: ../../frameworks.md#owasp-asvs
[UK GDPR]: ../../frameworks.md#uk-gdpr
