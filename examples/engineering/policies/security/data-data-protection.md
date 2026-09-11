---
id: pol-DATA
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.9, A.5.12, A.5.14, A.5.33, A.5.34, A.8.3, A.8.10, A.8.11, A.8.12, A.8.24 ]
  - framework: UK GDPR
    clauses: [ Art.5(1)(b), Art.5(1)(c), Art.5(1)(e), Art.5(1)(f), Art.6, Art.15, Art.16, Art.17, Art.18, Art.20, Art.21, Art.25,
               Art.28, Art.30, Art.32, Art.32(1)(a), Art.44 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ classification, data-protection, encryption, privacy ]
---

# Data is protected according to its sensitivity

`Policy: pol-DATA` `DRAFT`

## Purpose

Data is classified by how sensitive it is, and handled accordingly. Personal data carries the additional obligations
owed to the people it describes.

Uniform handling is either wasteful or unsafe, and usually both: it over-protects trivial data while under-protecting
the records that would actually cause harm. Until we say which data is which, we cannot protect it in proportion to its
risk.

## Scope

All data held or processed by systems we build or operate, in every environment, including backups, exports, logs,
analytical copies and test data.

_Boundary: [pol-ENVS] governs the separation between the environments this data moves through, and [pol-SCRT] the
secrets that protect it. Recording a personal data breach and notifying it is [pol-INCR]'s. This policy owns what the
data itself requires, in whichever environment it sits._

## Clauses

| Id        | Clause                                                                                                                             | Alignment                                                                                                         |
|-----------|------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| `CLASS`   | **MUST** classify data by sensitivity and handle each class according to that classification                                       | [ISO 27001:2022].A.5.12, [ISO 27001:2022].A.8.3                                                                   |
| `MASK`    | **MUST** mask, pseudonymise or anonymise personal data wherever the full value is not needed to do the work                        | [ISO 27001:2022].A.8.11, [UK GDPR].Art.32(1)(a)                                                                   |
| `CRYPTO`  | **MUST** protect sensitive data in transit and at rest using current, well-regarded cryptographic algorithms                       | [ISO 27001:2022].A.5.14, [ISO 27001:2022].A.8.24, [UK GDPR].Art.32, [UK GDPR].Art.32(1)(a), [UK GDPR].Art.5(1)(f) |
| `RETIRE`  | **MUST** retire a cryptographic algorithm as it weakens                                                                            | [ISO 27001:2022].A.8.24, [UK GDPR].Art.32                                                                         |
| `LAWFUL`  | **MUST** handle personal data on a lawful basis                                                                                    | [ISO 27001:2022].A.5.34, [UK GDPR].Art.6                                                                          |
| `BASIS`   | **MUST** record which lawful basis covers each processing of personal data                                                         | [UK GDPR].Art.6                                                                                                   |
| `PURPOSE` | **MUST** state the purpose each store of personal data serves, and check a new use against it                                      | [UK GDPR].Art.5(1)(b)                                                                                             |
| `MINIMAL` | **MUST** limit personal data to what is needed, wherever it is collected, copied or derived                                        | [UK GDPR].Art.5(1)(c)                                                                                             |
| `RIGHTS`  | **MUST** be able to find, export, correct, delete and restrict one person's data in every store that holds it                      | [UK GDPR].Art.15, [UK GDPR].Art.16, [UK GDPR].Art.17, [UK GDPR].Art.18, [UK GDPR].Art.20, [UK GDPR].Art.21        |
| `INVENT`  | **MUST** maintain an inventory of the information we hold, naming what it is, where it lives, who owns it and who we share it with | [ISO 27001:2022].A.5.9, [UK GDPR].Art.30                                                                          |
| `XBORDER` | **MUST** hold and process personal data only where a lawful transfer mechanism covers it                                           | [UK GDPR].Art.44                                                                                                  |
| `KEEP`    | **MUST** keep the records we are required to retain intact and retrievable for the retention period set for them                   | [ISO 27001:2022].A.5.33                                                                                           |
| `DELETE`  | **MUST** delete data when its defined retention period ends                                                                        | [ISO 27001:2022].A.8.10, [UK GDPR].Art.5(1)(e)                                                                    |
| `UNMASK`  | **MUST NOT** place unmasked production or personal data into an environment below production. See [pol-ENVS]                       | [ISO 27001:2022].A.8.11, [UK GDPR].Art.25                                                                         |
| `SHARE`   | **MUST NOT** send personal data to a third party before a written processing agreement covers it                                   | [UK GDPR].Art.28                                                                                                  |
| `LINGER`  | **MUST NOT** retain sensitive or personal data beyond its defined lifetime without a recorded deviation ([pol-DEVI])               | [ISO 27001:2022].A.8.10, [UK GDPR].Art.5(1)(e)                                                                    |
| `REVIVE`  | **MUST NOT** let personal data erased on request return through a restore                                                          | [UK GDPR].Art.17                                                                                                  |
| `LOGS`    | **MUST NOT** write unmasked sensitive personal data into logs or telemetry. See [pol-SCRT]                                         | [ISO 27001:2022].A.8.12                                                                                           |
| `LEAK`    | SHOULD detect sensitive data leaving through a route that is open for other traffic                                                | [ISO 27001:2022].A.8.12                                                                                           |
| `AGILE`   | COULD change cryptographic algorithm without re-architecting what depends on it, rather than treating the choice as permanent      |                                                                                                                   |
| `CLEAR`   | COULD protect sensitive data so that it is never processed in the clear                                                            |                                                                                                                   |

## Exceptions

Where a defect genuinely cannot be reproduced without production data, a time-boxed, access-restricted copy may be used
under a recorded deviation ([pol-DEVI]) that names who approved it, who can see it and when it will be destroyed. Legal
hold overrides deletion, and is recorded when it does.

`LAWFUL` admits none. Where personal data may be copied, and for how long, is a question a recorded deviation can
answer; whether we were entitled to hold it at all is not.

[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-ENVS]: ../security/envs-environment-separation.md
[pol-INCR]: ../operations/incr-incident-response.md
[pol-SCRT]: ../security/scrt-secrets-are-never-embedded.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[UK GDPR]: ../../frameworks.md#uk-gdpr
