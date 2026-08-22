---
id: pol-EVER
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.4
  - ISO27001:2022 A.8.9
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.32
review-by: "2027-08-04"
owner: paul.law
tags: [ change-management, source-control, traceability ]
---

# Everything is in version control

`Policy: pol-EVER` `DRAFT`

## Purpose

We hold everything needed to build, configure, deploy, run and recover a solution in version control, with a complete
and attributable history. If one asset needed to rebuild the system is not versioned, we cannot claim the system is
reproducible. A schema migration and an application change differ in medium, not in the rigour they get.

Almost every other policy here assumes there is one authoritative copy of everything. Review, traceability, reproducible
builds, controlled release and recovery all rest on it. Without it, each of them becomes something we assert rather than
something we can show.

## Scope

All solutions we build or operate, and every asset required to rebuild one: application code, infrastructure
definitions, database schema and migrations, pipeline definitions, non-secret configuration, operational scripts and the
documentation that describes them.

Secrets are the deliberate exception. See [pol-SCRT].

## Clauses

| Id        | Clause                                                                                                                                           | Alignment                                                              |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------|
| `ASSETS`  | **MUST** hold every asset needed to build, deploy, run and recover a solution in version control                                                 | [ISO 27001:2022].A.8.9, [NIST SSDF 1.1].PS.1                           |
| `HISTORY` | **MUST** preserve a complete change history that attributes each change to an individual                                                         | [ISO 27001:2022].A.8.32, [NIST SSDF 1.1].PS.1                          |
| `INTENT`  | **MUST** link each change to the work that asked for it                                                                                          | [ISO 27001:2022].A.8.32                                                |
| `BRANCH`  | **MUST** protect the default branch, so changes arrive by reviewed merge rather than direct push                                                 | [ISO 27001:2022].A.8.4, [ISO 27001:2022].A.8.32, [NIST SSDF 1.1].PS.1  |
| `PARITY`  | **MUST** subject changes to infrastructure, schema and configuration to the same review as changes to application code                           | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.32, [NIST SSDF 1.1].PW.7 |
| `ORPHAN`  | **MUST NOT** allow an asset that is necessary to reproduce a system to exist only on an individual's machine, in a console, or in a shared drive | [ISO 27001:2022].A.8.9, [NIST SSDF 1.1].PS.1                           |
| `SHARED`  | **MUST NOT** accept shared or generic accounts that make a change unattributable. See [pol-ACCS]                                                 | [ISO 27001:2022].A.8.4, [NIST SSDF 1.1].PS.1                           |
| `SIGNED`  | COULD prove the authorship of a change cryptographically, rather than trusting what it claims                                                    | [NIST SSDF 1.1].PS.1                                                   |

## Exceptions

Vendor-supplied binaries and third-party assets we cannot hold in source are referenced by version and provenance
instead. See [pol-TRUS]. Any other asset held outside version control requires a recorded deviation under [pol-DEVI],
naming the asset, the reason and the recovery plan if it is lost.

[pol-ACCS]: accs-access-by-identity.md
[pol-DEVI]: devi-deviations-are-recorded.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[pol-TRUS]: trus-trusted-components.md
[ISO 27001:2022]: ../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../frameworks.md#nist-ssdf
