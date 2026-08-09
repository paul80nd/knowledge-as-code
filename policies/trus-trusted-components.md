---
id: pol-TRUS
tier: normative
category: security
status: draft
aligns-with:
  - ISO27001:2022 A.5.19
  - ISO27001:2022 A.5.21
  - ISO27001:2022 A.5.22
  - ISO27001:2022 A.5.23
  - ISO27001:2022 A.5.32
  - ISO27001:2022 A.8.7
  - ISO27001:2022 A.8.19
review-by: "2027-08-04"
owner: paul.law
tags: [ dependencies, provenance, supply-chain ]
---

# We ship only components we know and trust

`Policy: pol-TRUS` `DRAFT`

## Purpose

We know what our software is made of. Every third-party component we depend on is inventoried and screened, and every
artifact we deploy traces back to the source change that produced it.

Most of what we ship, we did not write. A dependency we cannot enumerate is a vulnerability we cannot answer questions
about when one is disclosed, and an artifact whose origin we cannot establish is one we are trusting on faith.

## Scope

All third-party and open-source components used by systems we build or operate, including transitive dependencies, base
images and build-time tooling; the third-party and cloud services those systems depend on; and all artifacts we deploy.

## Clauses

| Id        | Clause                                                                                                                                       | Alignment                                                              |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------|
| `INVENT`  | **MUST** maintain a resolvable inventory of what each solution depends on                                                                    | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PW.4                          |
| `SCREEN`  | **MUST** screen dependencies for known vulnerabilities and for licence terms we can accept                                                   | [ISO 27001:2022].A.5.21, [ISO 27001:2022].A.5.32, [NIST SSDF 1.1].PW.4 |
| `MALWARE` | **MUST** scan the artifacts we build for malicious content before we release them                                                            | [ISO 27001:2022].A.8.7                                                 |
| `SOURCE`  | **MUST** obtain components from sources we have reason to trust                                                                              | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4                          |
| `CLOUD`   | **MUST** establish which security responsibilities we hold and which the provider holds, before adopting a service                           | [ISO 27001:2022].A.5.23                                                |
| `EXIT`    | **MUST** know how we would leave a service before we depend on it                                                                            | [ISO 27001:2022].A.5.23                                                |
| `REPO`    | **MUST** hold build artifacts in a managed repository, versioned and retained so a release can be identified, rolled back and examined later | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3                          |
| `TRACE`   | **MUST** be able to trace a deployed artifact to the change and the build that produced it                                                   | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PS.2                          |
| `REVIEW`  | **MUST** review the components and services we depend on periodically, not only when we adopt them                                           | [ISO 27001:2022].A.5.22                                                |
| `UNTRUST` | **MUST NOT** introduce a dependency from an untrusted or unverifiable source                                                                 | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4                          |
| `KNOWN`   | **MUST NOT** ship a component with a known critical vulnerability without a recorded, risk-owned deviation ([pol-DEVI])                      | [NIST SSDF 1.1].RV.2                                                   |
| `MUTATE`  | **MUST NOT** alter a released artifact in place — a change produces a new version                                                            | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3                          |

## Exceptions

A component that can no longer be sourced or maintained may be retained under a recorded deviation naming the risk
owner, the compensating controls and the plan to replace it. "It still works" is not a plan.

[pol-DEVI]: devi-deviations-are-recorded.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
[NIST SSDF 1.1]: /frameworks.md#nist-ssdf-1-1
