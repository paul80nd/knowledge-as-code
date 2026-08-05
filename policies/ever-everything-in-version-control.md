---
id: pol-EVER
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.8.4
  - ISO27001:2022 A.8.9
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.32
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ source-control, traceability, change-management ]
---

# Policy: Everything is in version control

## Purpose

Everything needed to build, configure, deploy, run and recover a solution is held in version control, with a complete
and attributable history. If an asset is required to reproduce the system and it is not versioned, the system cannot
honestly be said to be reproducible.

This is the foundation the rest of these policies stand on. Review, traceability, reproducible builds, controlled
release and recovery all assume a single authoritative source; without it each becomes a matter of trust rather than
evidence.

## Scope

All solutions we build or operate, and every asset required to rebuild one: application code, infrastructure
definitions, database schema and migrations, pipeline definitions, non-secret configuration, operational scripts and
the documentation that describes them.

Secrets are the deliberate exception — see [pol-SCRT].

## Commitments

* We **will** hold every asset needed to build, deploy, run and recover a solution in version control.
* We **will** preserve a complete change history that attributes each change to an individual.
* We **will** protect the default branch, so changes arrive by reviewed merge rather than direct push.
* We **will** subject changes to infrastructure, schema and configuration to the same review as changes to application
  code — the medium differs, the rigour does not.
* We **will not** allow an asset that is necessary to reproduce a system to exist only on an individual's machine, in a
  console, or in a shared drive.
* We **will not** accept shared or generic accounts that make a change unattributable.

## Alignment

| Reference                 | Area                                |
|---------------------------|-------------------------------------|
| ISO/IEC 27001:2022 A.8.4  | Access to source code               |
| ISO/IEC 27001:2022 A.8.9  | Configuration management            |
| ISO/IEC 27001:2022 A.8.25 | Secure development lifecycle        |
| ISO/IEC 27001:2022 A.8.32 | Change management                   |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Vendor-supplied binaries and third-party assets we cannot hold in source are referenced by version and provenance
instead — see [pol-TRUS]. Any other asset held outside version control requires a recorded
deviation under [pol-DEVI], naming the asset, the reason and the recovery plan if it is
lost.

## Implemented by

Intended implementing standards: source control, infrastructure as code, database lifecycle, application and runtime
configuration, deployment and release, documentation as code.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[pol-TRUS]: trus-trusted-components.md
