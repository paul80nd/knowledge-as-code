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

Everything needed to build, configure, deploy, run and recover a solution is held in version control, with a complete
and attributable history. If an asset is required to reproduce the system and it is not versioned, the system cannot
honestly be said to be reproducible.

This is the foundation the rest of these policies stand on. Review, traceability, reproducible builds, controlled
release and recovery all assume a single authoritative source; without it each becomes a matter of trust rather than
evidence.

## Scope

All solutions we build or operate, and every asset required to rebuild one: application code, infrastructure
definitions, database schema and migrations, pipeline definitions, non-secret configuration, operational scripts and the
documentation that describes them.

Secrets are the deliberate exception — see [pol-SCRT].

## Clauses

| Id        | Clause                                                                                                                                                           | Alignment                                        |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| `ASSETS`  | **MUST** hold every asset needed to build, deploy, run and recover a solution in version control                                                                 | [ISO 27001:2022].A.8.9                           |
| `HISTORY` | **MUST** preserve a complete change history that attributes each change to an individual                                                                         | [ISO 27001:2022].A.8.32                          |
| `BRANCH`  | **MUST** protect the default branch, so changes arrive by reviewed merge rather than direct push                                                                 | [ISO 27001:2022].A.8.4, [ISO 27001:2022].A.8.32  |
| `PARITY`  | **MUST** subject changes to infrastructure, schema and configuration to the same review as changes to application code — the medium differs, the rigour does not | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.32 |
| `ORPHAN`  | **MUST NOT** allow an asset that is necessary to reproduce a system to exist only on an individual's machine, in a console, or in a shared drive                 | [ISO 27001:2022].A.8.9                           |
| `SHARED`  | **MUST NOT** accept shared or generic accounts that make a change unattributable                                                                                 | [ISO 27001:2022].A.8.4                           |

## Exceptions

Vendor-supplied binaries and third-party assets we cannot hold in source are referenced by version and provenance
instead — see [pol-TRUS]. Any other asset held outside version control requires a recorded deviation under [pol-DEVI],
naming the asset, the reason and the recovery plan if it is lost.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[pol-TRUS]: trus-trusted-components.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
