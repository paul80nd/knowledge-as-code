---
id: pol-DATA
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.5.12
  - ISO27001:2022 A.5.34
  - ISO27001:2022 A.8.10
  - ISO27001:2022 A.8.11
  - ISO27001:2022 A.8.12
  - ISO27001:2022 A.8.24
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - data-protection
  - privacy
  - classification
  - encryption
---

# Policy: Data is protected according to its sensitivity

## Purpose

Data is classified by how sensitive it is and handled accordingly — protected in transit and at rest, kept away from
places it does not belong, and deleted when its purpose ends. Personal data carries the additional obligations owed to
the people it describes.

Uniform handling is either wasteful or unsafe, and usually both: it over-protects trivial data while under-protecting
the records that would actually cause harm. Classification is what makes proportionate protection possible.

## Scope

All data held or processed by systems we build or operate, in every environment, including backups, exports, logs,
analytical copies and test data.

## Commitments

* We **will** classify data by sensitivity and handle each class according to that classification.
* We **will** protect sensitive data in transit and at rest using current, well-regarded cryptographic algorithms, and
  retire algorithms as they weaken.
* We **will** handle personal data on a lawful basis, collect only what is needed, and support the rights of the people
  it concerns.
* We **will** know where our sensitive and personal data lives.
* We **will** delete data when its defined retention period ends.
* We **will not** place unmasked production or personal data into an environment below production.
* We **will not** retain sensitive or personal data beyond its defined lifetime without a recorded reason.
* We **will not** write unmasked sensitive personal data into logs or telemetry.

## Alignment

| Reference                 | Area                                        |
|---------------------------|---------------------------------------------|
| ISO/IEC 27001:2022 A.5.12 | Classification of information               |
| ISO/IEC 27001:2022 A.5.34 | Privacy and protection of personal data     |
| ISO/IEC 27001:2022 A.8.10 | Information deletion                        |
| ISO/IEC 27001:2022 A.8.11 | Data masking                                |
| ISO/IEC 27001:2022 A.8.12 | Data leakage prevention                     |
| ISO/IEC 27001:2022 A.8.24 | Use of cryptography                         |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Where a defect genuinely cannot be reproduced without production data, a time-boxed, access-restricted copy may be used
under a recorded deviation ([pol-DEVI]) that names who approved it, who can see it and when
it will be destroyed. Legal hold overrides deletion, and is recorded when it does.

## Implemented by

Intended implementing standards: data protection, classification and privacy; cryptography and key management; and the
data-handling provisions of the environments standard.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
