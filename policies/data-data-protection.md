---
id: pol-DATA
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.9
  - ISO27001:2022 A.5.12
  - ISO27001:2022 A.5.14
  - ISO27001:2022 A.5.34
  - ISO27001:2022 A.8.10
  - ISO27001:2022 A.8.11
  - ISO27001:2022 A.8.12
  - ISO27001:2022 A.8.24
review-by: "2027-08-04"
owner: paul.law
tags: [ classification, data-protection, encryption, privacy ]
---

# Data is protected according to its sensitivity

`Policy: pol-DATA` `DRAFT`

## Purpose

Data is classified by how sensitive it is and handled accordingly — protected in transit and at rest, kept away from
places it does not belong, and deleted when its purpose ends. Personal data carries the additional obligations owed to
the people it describes.

Uniform handling is either wasteful or unsafe, and usually both: it over-protects trivial data while under-protecting
the records that would actually cause harm. Classification is what makes proportionate protection possible.

## Scope

All data held or processed by systems we build or operate, in every environment, including backups, exports, logs,
analytical copies and test data.

_Boundary: [pol-ENVS] governs the separation between the environments this data moves through, and [pol-SCRT] the
secrets that protect it. This policy owns what the data itself requires, in whichever environment it sits._

## Clauses

| Id        | Clause                                                                                                                                             | Alignment                                                          |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------|
| `CLASS`   | **MUST** classify data by sensitivity and handle each class according to that classification                                                       | [ISO 27001:2022].A.5.12                                            |
| `CRYPTO`  | **MUST** protect sensitive data in transit and at rest using current, well-regarded cryptographic algorithms, and retire algorithms as they weaken | [ISO 27001:2022].A.5.14, [ISO 27001:2022].A.8.24, [UK GDPR].Art.32 |
| `LAWFUL`  | **MUST** handle personal data on a lawful basis                                                                                                    | [ISO 27001:2022].A.5.34, [UK GDPR].Art.6                           |
| `MINIMAL` | **MUST** collect only the personal data that is needed                                                                                             | [UK GDPR].Art.5(1)(c)                                              |
| `RIGHTS`  | **MUST** support the rights of the people the data concerns                                                                                        | [UK GDPR].Ch.III                                                   |
| `LOCATE`  | **MUST** know where our sensitive and personal data lives                                                                                          | [ISO 27001:2022].A.5.9, [ISO 27001:2022].A.5.12, [UK GDPR].Art.30  |
| `XBORDER` | **MUST** hold and process personal data only where a lawful transfer mechanism covers it                                                           | [ISO 27001:2022].A.5.14, [UK GDPR].Art.44                          |
| `DELETE`  | **MUST** delete data when its defined retention period ends                                                                                        | [ISO 27001:2022].A.8.10, [UK GDPR].Art.17                          |
| `UNMASK`  | **MUST NOT** place unmasked production or personal data into an environment below production — see [pol-ENVS]                                      | [ISO 27001:2022].A.8.11, [UK GDPR].Art.25                          |
| `SHARE`   | **MUST NOT** send personal data to a third party before a written processing agreement covers it                                                   | [ISO 27001:2022].A.5.14, [UK GDPR].Art.28                          |
| `LINGER`  | **MUST NOT** retain sensitive or personal data beyond its defined lifetime without a recorded deviation ([pol-DEVI])                               | [ISO 27001:2022].A.8.10, [UK GDPR].Art.5(1)(e)                     |
| `LOGS`    | **MUST NOT** write unmasked sensitive personal data into logs or telemetry — see [pol-SCRT]                                                        | [ISO 27001:2022].A.8.12, [UK GDPR].Art.5(1)(f)                     |
| `AGILE`   | COULD change cryptographic algorithm without re-architecting what depends on it, rather than treating the choice as permanent                      |                                                                    |
| `CLEAR`   | COULD protect sensitive data so that it is never processed in the clear                                                                            | [UK GDPR].Art.32(1)(a)                                             |

## Exceptions

Where a defect genuinely cannot be reproduced without production data, a time-boxed, access-restricted copy may be used
under a recorded deviation ([pol-DEVI]) that names who approved it, who can see it and when it will be destroyed. Legal
hold overrides deletion, and is recorded when it does.

`LAWFUL` admits none. Where personal data may be copied, and for how long, is a question a recorded deviation can
answer; whether we were entitled to hold it at all is not.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-ENVS]: envs-environment-separation.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
[UK GDPR]: /frameworks.md#uk-gdpr
