---
id: pol-ENVS
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.8.3
  - ISO27001:2022 A.8.9
  - ISO27001:2022 A.8.31
  - ISO27001:2022 A.8.33
review-by: "2027-08-04"
owner: paul.law
tags: [ environments, production-data, separation ]
---

# Environments are separated, and production stays in production

`Policy: pol-ENVS` `DRAFT`

## Purpose

Production is separated from every environment below it — by access, by network and by credential — so that a change can
be validated before it reaches customers, and so that nothing outside production can reach production's data or
identity.

Separation is what makes a lower environment safe to experiment in. Where the boundary is soft, a mistake in development
becomes an incident in production, and every copy of production data multiplies the blast radius of a breach without
adding any protection of its own.

## Scope

All environments hosting systems we build or operate, from a developer's machine through to production, including
temporary and on-demand environments.

## Clauses

| Id        | Clause                                                                                                                                | Alignment                                     |
|-----------|---------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| `SPLIT`   | **MUST** separate production from non-production, with distinct access controls at each tier                                          | [ISO 27001:2022].A.8.31, [NIST SSDF 1.1].PO.5 |
| `CREDS`   | **MUST** keep production credentials and secrets unreachable from any lower environment — see [pol-SCRT]                              | [ISO 27001:2022].A.8.3, [NIST SSDF 1.1].PO.5  |
| `SAMEDEF` | **MUST** provision environments from the same definitions, so that what passes below production is a fair test of what will run in it | [ISO 27001:2022].A.8.31, [NIST SSDF 1.1].PO.5 |
| `BASELIN` | **MUST** be able to state the configuration an environment is running, and reproduce it                                               | [ISO 27001:2022].A.8.9                        |
| `PROMOTE` | **MUST** promote changes between environments through automation rather than by manual copy                                           | [ISO 27001:2022].A.8.31, [NIST SSDF 1.1].PO.5 |
| `MASK`    | **MUST** mask or synthesise the data used below production                                                                            | [ISO 27001:2022].A.8.33, [NIST SSDF 1.1].PO.5 |
| `DEBUG`   | **MUST NOT** develop, test or debug against production                                                                                | [ISO 27001:2022].A.8.31, [NIST SSDF 1.1].PO.5 |
| `REUSE`   | **MUST NOT** reuse a production secret in any environment below production — see [pol-SCRT]                                           | [ISO 27001:2022].A.8.3, [NIST SSDF 1.1].PO.5  |
| `UNMASK`  | **MUST NOT** place unmasked production or personal data into a lower environment — see [pol-DATA]                                     | [ISO 27001:2022].A.8.33, [NIST SSDF 1.1].PO.5 |
| `EPHEM`   | SHOULD be able to create an environment on demand and discard it when the work is done                                                |                                               |

## Exceptions

Not every solution needs a full set of tiers; a reduced set is acceptable where the rationale is recorded and the
production boundary itself is unaffected. Diagnosing a live incident in production is incident response, not
development, and is governed by [pol-INCR]. `CREDS` and `REUSE` carry a secrets prohibition that [pol-SCRT] owns and
that admits no exception. Any other departure requires a recorded deviation under [pol-DEVI].

[pol-DATA]: data-data-protection.md
[pol-DEVI]: devi-deviations-are-recorded.md
[pol-INCR]: incr-incident-response.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
[NIST SSDF 1.1]: /frameworks.md#nist-ssdf-1-1
