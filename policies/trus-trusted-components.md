---
id: pol-TRUS
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.19
  - ISO27001:2022 A.5.21
  - ISO27001:2022 A.8.19
  - ISO27001:2022 A.8.30
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ dependencies, provenance, supply-chain ]
---

# `pol-TRUS` We ship only components we know and trust

## Purpose

We know what our software is made of. Every third-party component we depend on is inventoried and screened, and every
artifact we deploy traces back to the source change that produced it.

Most of what we ship, we did not write. A dependency we cannot enumerate is a vulnerability we cannot answer questions
about when one is disclosed, and an artifact whose origin we cannot establish is one we are trusting on faith.

## Scope

All third-party and open-source components used by systems we build or operate, including transitive dependencies, base
images and build-time tooling; and all artifacts we deploy.

## Commitments

* We **will** maintain a resolvable inventory of what each solution depends on.
* We **will** screen dependencies for known vulnerabilities and for licence terms we can accept.
* We **will** obtain components from sources we have reason to trust.
* We **will** hold build artifacts in a managed repository, versioned and retained so a release can be identified,
  rolled back and examined later.
* We **will** be able to trace a deployed artifact to the change and the build that produced it.
* We **will not** introduce a dependency from an untrusted or unverifiable source.
* We **will not** ship a component with a known critical vulnerability without a recorded, risk-owned deviation
  ([pol-DEVI]).
* We **will not** alter a released artifact in place — a change produces a new version.

## Alignment

| Reference                 | Area                                            |
|---------------------------|-------------------------------------------------|
| ISO/IEC 27001:2022 A.5.19 | Information security in supplier relationships  |
| ISO/IEC 27001:2022 A.5.21 | Security in the ICT supply chain                |
| ISO/IEC 27001:2022 A.8.19 | Installation of software on operational systems |
| ISO/IEC 27001:2022 A.8.30 | Outsourced development                          |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

A component that can no longer be sourced or maintained may be retained under a recorded deviation naming the risk
owner, the compensating controls and the plan to replace it. "It still works" is not a plan.

## Implemented by

Intended implementing standards: dependency and supply-chain management, and artifact management and provenance.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
