---
id: pol-ENVS
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.8.3
  - ISO27001:2022 A.8.31
  - ISO27001:2022 A.8.33
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - environments
  - separation
  - production-data
---

# Policy: Environments are separated, and production stays in production

## Purpose

Production is separated from every environment below it — by access, by network and by credential — so that a change can
be validated before it reaches customers, and so that nothing outside production can reach production's data or
identity.

Separation is what makes a lower environment safe to experiment in. Where the boundary is soft, a mistake in
development becomes an incident in production, and every copy of production data multiplies the blast radius of a breach
without adding any protection of its own.

## Scope

All environments hosting systems we build or operate, from a developer's machine through to production, including
temporary and on-demand environments.

## Commitments

* We **will** separate production from non-production, with distinct access controls at each tier.
* We **will** keep production credentials and secrets unreachable from any lower environment.
* We **will** provision environments from the same definitions, so that what passes below production is a fair test of
  what will run in it.
* We **will** promote changes between environments through automation rather than by manual copy.
* We **will** mask or synthesise the data used below production.
* We **will not** develop, test or debug against production.
* We **will not** reuse a production secret in any environment below production.
* We **will not** place unmasked production or personal data into a lower environment — see
  [pol-DATA].

## Alignment

| Reference                 | Area                                                       |
|---------------------------|------------------------------------------------------------|
| ISO/IEC 27001:2022 A.8.3  | Information access restriction                             |
| ISO/IEC 27001:2022 A.8.31 | Separation of development, test and production environments |
| ISO/IEC 27001:2022 A.8.33 | Test information                                            |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Not every solution needs a full set of tiers; a reduced set is acceptable where the rationale is recorded and the
production boundary itself is unaffected. Diagnosing a live incident in production is incident response, not
development, and is governed by [pol-INCR]. Any other departure requires a recorded deviation
under [pol-DEVI].

## Implemented by

Intended implementing standards: environments and promotion, network security and segmentation, secrets management, and
data protection.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DATA]: data-data-protection.md
[pol-DEVI]: devi-deviations-are-recorded.md
[pol-INCR]: incr-incident-response.md
