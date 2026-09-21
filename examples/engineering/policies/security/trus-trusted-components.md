---
id: pol-TRUS
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.19, A.5.21, A.5.32, A.8.7, A.8.19 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ dependencies, provenance, supply-chain ]
---

# We ship only components we know and trust

`Policy: pol-TRUS` `DRAFT`

## Purpose

We know what our software is made of, and where every artefact we ship came from.

Most of what we ship, we did not write. When a vulnerability is disclosed, the first question is whether we use the
affected component. A dependency missing from the inventory leaves that question open. Where we cannot establish an
artefact's origin, we have no evidence of what is in it.

## Scope

All third-party and open-source components used by systems we build or operate, including transitive dependencies, base
images and build-time tooling. A tool required for the pipeline to build, verify or release software counts as
build-time tooling, whether or not it ships inside the artefact. A tool used only incidentally, with no place in that
pipeline, is not, and stays procurement's or IT operations' asset register to keep. It also covers every artefact we
deploy, whether we built it or obtained it from someone else.

_Boundary: this policy governs what we admit into the estate and what we can prove about it. [pol-SUPL] owns a
third-party or cloud service we depend on but do not ship anything of. [pol-VURM] owns finding, prioritising and
closing the vulnerabilities in what we have admitted, including whether a finding blocks a release. [pol-PIPE] owns
the route an artefact takes into production and the approval behind it._

## Clauses

| Id        | Clause                                                                                                                                                 | Alignment                                     |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| `INVENT`  | **MUST** maintain an inventory of what each solution depends on, naming each component and the version in use                                          | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PW.4 |
| `PINNED`  | **MUST** keep the exact versions a build resolved in version control, so two builds of the same source take the same components                        | [ISO 27001:2022].A.5.21                       |
| `SCREEN`  | **MUST** screen a component for known vulnerabilities before we adopt it                                                                               | [ISO 27001:2022].A.5.21, [NIST SSDF 1.1].PW.4 |
| `SUPPORT` | **MUST** keep a component on a version the vendor still supports                                                                                       | [ISO 27001:2022].A.8.19                       |
| `PATCH`   | **MUST** update dependencies and runtimes on a routine cadence, independent of a specific vulnerability finding                                        | [ISO 27001:2022].A.8.19                       |
| `LICENCE` | **MUST** screen a component's licence for terms we cannot accept before we adopt it                                                                    | [ISO 27001:2022].A.5.32                       |
| `OBLIGE`  | **MUST** honour the terms of a licence for as long as we ship what it covers                                                                           | [ISO 27001:2022].A.5.32                       |
| `MALWARE` | **MUST** scan the artefacts we build for malicious content before we release them                                                                      | [ISO 27001:2022].A.8.7                        |
| `RUNMAL`  | **MUST** protect the systems we run from malicious code, and act on what is found                                                                      | [ISO 27001:2022].A.8.7                        |
| `SOURCE`  | **MUST** obtain components from sources we have reason to trust                                                                                        | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4 |
| `REPO`    | **MUST** keep build artefacts in a managed repository, versioned and retained so a release can be identified, rolled back and examined later           | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3 |
| `TRACE`   | **MUST** be able to trace a deployed artefact to the change and the build that produced it ([pol-PIPE].TRACE states the same for a production release) | [ISO 27001:2022].A.5.21, [SLSA 1.1].build-L1  |
| `UNTRUST` | **MUST NOT** introduce a component or artefact from an untrusted or unverifiable source                                                                | [ISO 27001:2022].A.5.19, [NIST SSDF 1.1].PW.4 |
| `MUTATE`  | **MUST NOT** alter a released artefact in place                                                                                                        | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.3 |
| `ATTEST`  | SHOULD refuse into production any artefact whose origin cannot be cryptographically proven                                                             | [NIST SSDF 1.1].PS.2, [SLSA 1.1].build-L2     |

## Exceptions

A component that can no longer be sourced or maintained is kept only under a recorded deviation ([pol-DEVI]). The
deviation names the risk owner, the compensating controls and the plan to replace it. "It still works" is not a plan.

[pol-DEVI]: ../governance/devi-deviations.md
[pol-PIPE]: ../delivery/pipe-pipeline-to-production.md#clauses
[pol-SUPL]: ../security/supl-supplier-services.md#clauses
[pol-VURM]: ../security/vurm-vulnerability-remediation.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../../frameworks.md#nist-ssdf
[SLSA 1.1]: ../../frameworks.md#slsa
