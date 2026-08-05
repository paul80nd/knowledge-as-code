---
id: pol-SCRT
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.17
  - ISO27001:2022 A.8.24
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ credentials, key-management, secrets ]
---

# Policy: Secrets are managed, never embedded

## Purpose

Secrets — credentials, keys, tokens and certificates — are held in a controlled store that restricts and records access
and allows rotation without changing code. They are never embedded in source, configuration, pipeline definitions or
build artifacts.

An embedded secret cannot be rotated without a release, cannot be revoked in isolation, and survives in history long
after the file that carried it is deleted. Treating secrets as managed assets rather than as text is what makes a
compromise recoverable.

## Scope

All secrets used by any system we build or operate, in every environment, including those used by pipelines, agents and
machine identities.

## Commitments

* We **will** hold secrets in a controlled store, with access granted by identity, restricted to those who need it, and
  recorded.
* We **will** be able to rotate any secret without a code change, and **will** rotate on a defined cycle and on any
  suspicion of compromise.
* We **will** protect the keys and certificates that protect our data through their full lifecycle — issue, storage,
  rotation, revocation.
* We **will** actively look for secrets that have leaked into places they should never reach.
* We **will not** commit a secret to version control, place one in a configuration file or pipeline definition, or bake
  one into a build artifact.
* We **will not** use a production secret anywhere outside production.
* We **will not** write a secret to a log, a console, an error message or a support ticket.

## Alignment

| Reference                 | Area                        |
|---------------------------|-----------------------------|
| ISO/IEC 27001:2022 A.5.17 | Authentication information  |
| ISO/IEC 27001:2022 A.8.24 | Use of cryptography         |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

None. This commitment does not bend for expediency, prototypes or "temporary" work — a leaked secret does not care why
it was written down. A secret that has reached source control is treated as compromised and rotated, not deleted and
forgotten.

## Implemented by

Intended implementing standards: secrets management, cryptography and key management, and the prohibition carried in
the source control standard.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
