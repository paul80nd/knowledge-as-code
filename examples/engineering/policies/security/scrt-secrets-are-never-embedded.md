---
id: pol-SCRT
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.17, A.8.15, A.8.24 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ credentials, key-management, secrets ]
---

# Secrets are managed, never embedded

`Policy: pol-SCRT` `DRAFT`

## Purpose

We keep secrets (credentials, keys, tokens and certificates) in a store we control. The store limits who can read a
secret and records who did. A secret is never embedded in the thing that uses it.

An embedded secret cannot be rotated without a release. It cannot be revoked on its own, and it stays in version
history after the file that contained it is deleted. A secret in a controlled store is rotated the day it leaks, and
everything using it keeps working.

## Scope

All secrets used by any system we build or operate, in every environment, including those used by pipelines, agents and
machine identities.

_Boundary: this policy owns the exception posture for every clause that binds secrets, including [pol-ENVS]'s.
[pol-ACCS] governs who can read a secret, and [pol-DATA] governs what a secret protects._

## Clauses

| Id        | Clause                                                                                                                                       | Alignment                                        |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `STORE`   | **MUST** keep secrets in a controlled store, with access granted by identity, restricted to those who need it, and recorded                  | [ISO 27001:2022].A.5.17                          |
| `ROTATE`  | **MUST** rotate any secret on a defined cycle and on any suspicion of compromise, without a code change                                      | [ISO 27001:2022].A.5.17                          |
| `KEYS`    | **MUST** protect the keys and certificates that protect our data through their full lifecycle: issue, storage, rotation, revocation          | [ISO 27001:2022].A.8.24                          |
| `LEAKED`  | **MUST** actively look for secrets that have leaked into places they should never reach                                                      | [ISO 27001:2022].A.5.17                          |
| `EMBED`   | **MUST NOT** commit a secret to version control, place one in a configuration file or pipeline definition, or bake one into a build artefact | [ISO 27001:2022].A.5.17                          |
| `REUSE`   | **MUST NOT** use a production secret anywhere outside production. See [pol-ENVS]                                                             | [ISO 27001:2022].A.5.17                          |
| `LOGS`    | **MUST NOT** write a secret to a log, a console, an error message or a support ticket. See [pol-DATA]                                        | [ISO 27001:2022].A.5.17, [ISO 27001:2022].A.8.15 |
| `ZEROSEC` | COULD operate with no static secret left to leak                                                                                             |                                                  |

## Exceptions

`EMBED`, `REUSE` and `LOGS` admit no exception, and no recorded deviation makes them acceptable. Expediency, a
prototype and "temporary" work are not exceptions either. A secret committed to source control is treated as
compromised and rotated, and deleting the file is not enough. [pol-ENVS] states the same prohibition on production
credentials below production, in its own `CREDS` and `REUSE`, and the posture stated here covers those too.

`STORE`, `ROTATE`, `KEYS` and `LEAKED` admit an exception where one is unavoidable. A vendor-issued certificate only
the vendor can rotate is the ordinary case, and it is a recorded deviation under [pol-DEVI] naming who accepts the
risk, what compensates for it and when it is revisited.

[pol-ACCS]: ../security/accs-access-by-identity.md
[pol-DATA]: ../security/data-data-protection.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-ENVS]: ../security/envs-environment-separation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
