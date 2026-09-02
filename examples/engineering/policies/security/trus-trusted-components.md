---
id: pol-TRUS
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.19, A.5.21, A.5.22, A.5.23, A.5.32, A.8.7, A.8.19 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ dependencies, provenance, supply-chain ]
---

# We ship only components we know and trust

`Policy: pol-TRUS` `DRAFT`

## Purpose

We know what our software is made of, and where every artefact we ship came from.

Most of what we ship, we did not write. When a vulnerability is disclosed, the first question is whether we use the
affected component. A dependency we cannot list is a question we cannot answer. An artefact whose origin we cannot
establish is one we are trusting on faith.

## Scope

All third-party and open-source components used by systems we build or operate, including transitive dependencies, base
images and build-time tooling. It also covers the third-party and cloud services those systems depend on, and every
artefact we deploy, whether we built it or obtained it from someone else.

_Boundary: this policy governs what we admit into the estate and what we can prove about it. Finding, prioritising and
closing the vulnerabilities in what we have admitted is [pol-VURM]'s, including whether a finding blocks a release. The
route an artefact takes into production, and the approval behind it, is [pol-PIPE]'s._

## Clauses

| Id        | Clause                                                                                                                                       | Alignment                                     |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| `INVENT`  | **MUST** maintain an inventory of what each solution depends on, naming each component and the version in use                                | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PW.4 |
| `SCREEN`  | **MUST** screen a component for known vulnerabilities before we adopt it                                                                     | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PW.4 |
| `LICENCE` | **MUST** screen a component's licence for terms we cannot accept before we adopt it                                                          | [ISO 27001:2022].A.5.32                       |
| `MALWARE` | **MUST** scan the artefacts we build for malicious content before we release them                                                            | [ISO 27001:2022].A.8.7                        |
| `SOURCE`  | **MUST** obtain components from sources we have reason to trust                                                                              | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4 |
| `CLOUD`   | **MUST** establish which security responsibilities we hold and which the provider holds, before adopting a service                           | [ISO 27001:2022].A.5.23                       |
| `EXIT`    | **MUST** know how we would leave a service before we depend on it                                                                            | [ISO 27001:2022].A.5.23                       |
| `REPO`    | **MUST** hold build artefacts in a managed repository, versioned and retained so a release can be identified, rolled back and examined later | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3 |
| `TRACE`   | **MUST** be able to trace a deployed artefact to the change and the build that produced it. See [pol-PIPE]                                   | [ISO 27001:2022].A.5.21, [SLSA 1.1].build-L1  |
| `REVIEW`  | **MUST** review the components and services we depend on periodically, not only when we adopt them                                           | [ISO 27001:2022].A.5.22                       |
| `UNTRUST` | **MUST NOT** introduce a component or artefact from an untrusted or unverifiable source                                                      | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4 |
| `MUTATE`  | **MUST NOT** alter a released artefact in place                                                                                              | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3 |
| `ATTEST`  | SHOULD refuse into production any artefact whose origin cannot be cryptographically proven                                                   | [NIST SSDF 1.1].PS.2, [SLSA 1.1].build-L2     |

## Exceptions

A component that can no longer be sourced or maintained may be retained under a recorded deviation ([pol-DEVI]). The
deviation names the risk owner, the compensating controls and the plan to replace it. "It still works" is not a plan.

[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-PIPE]: ../delivery/pipe-pipeline-to-production.md
[pol-VURM]: ../security/vurm-vulnerability-remediation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../../frameworks.md#nist-ssdf
[SLSA 1.1]: ../../frameworks.md#slsa
