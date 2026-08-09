---
id: pol-SCRT
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.17
  - ISO27001:2022 A.8.24
review-by: "2027-08-04"
owner: paul.law
tags: [ credentials, key-management, secrets ]
---

# Secrets are managed, never embedded

`Policy: pol-SCRT` `DRAFT`

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

## Clauses

| Id        | Clause                                                                                                                                       | Alignment               |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `STORE`   | **MUST** hold secrets in a controlled store, with access granted by identity, restricted to those who need it, and recorded                  | [ISO 27001:2022].A.5.17 |
| `ROTATE`  | **MUST** rotate any secret on a defined cycle and on any suspicion of compromise, without a code change                                      | [ISO 27001:2022].A.5.17 |
| `KEYS`    | **MUST** protect the keys and certificates that protect our data through their full lifecycle — issue, storage, rotation, revocation         | [ISO 27001:2022].A.8.24 |
| `SCAN`    | **MUST** actively look for secrets that have leaked into places they should never reach                                                      | [ISO 27001:2022].A.5.17 |
| `EMBED`   | **MUST NOT** commit a secret to version control, place one in a configuration file or pipeline definition, or bake one into a build artifact | [ISO 27001:2022].A.5.17 |
| `REUSE`   | **MUST NOT** use a production secret anywhere outside production                                                                             | [ISO 27001:2022].A.5.17 |
| `LOGS`    | **MUST NOT** write a secret to a log, a console, an error message or a support ticket                                                        | [ISO 27001:2022].A.5.17 |
| `ZEROSEC` | COULD reach a position where there is no static secret left to leak _(aspirational)_                                                         |                         |

## Exceptions

None. This commitment does not bend for expediency, prototypes or "temporary" work — a leaked secret does not care why
it was written down. A secret that has reached source control is treated as compromised and rotated, not deleted and
forgotten.

[ISO 27001:2022]: /frameworks.md#iso27001-2022
