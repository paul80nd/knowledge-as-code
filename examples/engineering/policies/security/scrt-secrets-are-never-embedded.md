---
id: pol-SCRT
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.17, A.8.24 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ credentials, key-management, secrets ]
---

# Secrets are managed, never embedded

`Policy: pol-SCRT` `DRAFT`

## Purpose

Secrets (credentials, keys, tokens and certificates) live in a store we control, which limits who can reach them and
records who did. They never live in the thing that uses them.

An embedded secret cannot be rotated without a release, cannot be revoked in isolation, and survives in history long
after the file that carried it is deleted. A secret held in a store we control can be rotated the day it leaks, and
everything that used it keeps working.

## Scope

All secrets used by any system we build or operate, in every environment, including those used by pipelines, agents and
machine identities.

_Boundary: this policy owns the exception posture for every clause binding secrets, [pol-ENVS]'s included. Who may reach
a secret is [pol-ACCS]'s, and what a secret protects is [pol-DATA]'s._

## Clauses

| Id        | Clause                                                                                                                                       | Alignment               |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `STORE`   | **MUST** hold secrets in a controlled store, with access granted by identity, restricted to those who need it, and recorded                  | [ISO 27001:2022].A.5.17 |
| `ROTATE`  | **MUST** rotate any secret on a defined cycle and on any suspicion of compromise, without a code change                                      | [ISO 27001:2022].A.5.17 |
| `KEYS`    | **MUST** protect the keys and certificates that protect our data through their full lifecycle: issue, storage, rotation, revocation          | [ISO 27001:2022].A.8.24 |
| `LEAKED`  | **MUST** actively look for secrets that have leaked into places they should never reach                                                      | [ISO 27001:2022].A.5.17 |
| `EMBED`   | **MUST NOT** commit a secret to version control, place one in a configuration file or pipeline definition, or bake one into a build artefact | [ISO 27001:2022].A.5.17 |
| `REUSE`   | **MUST NOT** use a production secret anywhere outside production. See [pol-ENVS]                                                             | [ISO 27001:2022].A.5.17 |
| `LOGS`    | **MUST NOT** write a secret to a log, a console, an error message or a support ticket. See [pol-DATA]                                        | [ISO 27001:2022].A.5.17 |
| `ZEROSEC` | COULD reach a position where there is no static secret left to leak                                                                          |                         |

## Exceptions

`EMBED`, `REUSE` and `LOGS` admit none, and no recorded deviation makes them acceptable. A leaked secret does not care
why it was written down. They do not bend for expediency, prototypes or "temporary" work. A secret that has reached
source control is treated as compromised and rotated, not deleted and forgotten. [pol-ENVS] binds the same prohibition
on production credentials below production through its own `CREDS` and `REUSE`. The posture stated here covers those
too.

`STORE`, `ROTATE`, `KEYS` and `LEAKED` bend where they have to. A vendor-issued certificate only the vendor can rotate
is the ordinary case, and it is a recorded deviation under [pol-DEVI] naming who accepts the risk, what compensates for
it and when it is revisited.

[pol-ACCS]: ../security/accs-access-by-identity.md
[pol-DATA]: ../security/data-data-protection.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-ENVS]: ../security/envs-environment-separation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
