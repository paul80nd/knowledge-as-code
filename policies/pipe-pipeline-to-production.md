---
id: pol-PIPE
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.9
  - ISO27001:2022 A.8.19
  - ISO27001:2022 A.8.32
review-by: "2027-08-04"
owner: paul.law
tags: [ change-management, deployment, release-management ]
---

# Changes reach production through the pipeline

`Policy: pol-PIPE` `DRAFT`

## Purpose

The pipeline is the only route into production. Every production change — application, infrastructure, configuration or
schema — arrives through an automated path that is traceable to its source change and its approval, and that can be
reversed.

A single controlled route is what makes production knowable. Every hand-applied change creates a system whose real state
exists nowhere but the system itself, and the cost of that lands later, on whoever is trying to rebuild or recover it.

## Scope

Production and any environment where a change affects customers or holds real data. Applies equally to application
deployments, infrastructure changes, configuration changes and database changes.

_Boundary: this policy governs the route a change takes into production and the approval behind it. What an artefact is
made of, where it is held and whether its origin can be proved is [pol-TRUS]'s._

## Clauses

| Id        | Clause                                                                                                                                             | Alignment                                                                      |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| `DEPLOY`  | **MUST** deploy to production only through an automated pipeline                                                                                   | [ISO 27001:2022].A.8.19, [DORA metrics].deploy-frequency, [NIST SSDF 1.1].PO.3 |
| `SAMEART` | **MUST** deploy only artefacts the pipeline itself produced, promoting the same artefact through environments rather than rebuilding per stage     | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.2                                  |
| `CONFIG`  | **MUST** hold environment-specific configuration outside the artefact, so the artefact promoted between environments is the one that was built     | [ISO 27001:2022].A.8.9                                                         |
| `TRACE`   | **MUST** be able to trace any production release to the change, the artefact and the approval behind it — see [pol-TRUS]                           | [ISO 27001:2022].A.8.32, [NIST SSDF 1.1].PS.2                                  |
| `REVERT`  | **MUST** have a defined rollback or recovery path before a change goes to production                                                               | [ISO 27001:2022].A.8.32, [DORA metrics].recovery-time                          |
| `ASCODE`  | **MUST** hold the pipeline itself in version control under [pol-EVER], as a reviewed artefact like any other                                       | [ISO 27001:2022].A.8.9, [NIST SSDF 1.1].PO.3                                   |
| `GATES`   | **MUST** carry the safeguards that change approval exists to provide inside the pipeline, rather than treating automation as a reason to drop them | [ISO 27001:2022].A.8.32, [NIST SSDF 1.1].PO.4                                  |
| `FLAGS`   | **MUST** treat a flag that changes production behaviour as a controlled, auditable change                                                          | [ISO 27001:2022].A.8.32                                                        |
| `MANUAL`  | **MUST NOT** hand-edit production, whether code, configuration, infrastructure or schema, other than a flag change made under `FLAGS`              | [ISO 27001:2022].A.8.9, [ISO 27001:2022].A.8.32                                |
| `LOCAL`   | **MUST NOT** deploy an artefact built outside the pipeline                                                                                         | [ISO 27001:2022].A.8.19, [NIST SSDF 1.1].PS.2                                  |
| `PROGDEL` | COULD release progressively, promoting or reversing on live signals rather than on a decision made beforehand                                      | [DORA metrics].recovery-time                                                   |

## Exceptions

Emergency change to restore service may bypass the normal path where the pipeline is itself unavailable or the delay
would extend an outage. It is recorded as a deviation under [pol-DEVI] at the time, and the change is reconciled back
into version control before the incident is closed — otherwise the fix becomes the next outage.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-EVER]: ever-everything-in-version-control.md
[pol-TRUS]: trus-trusted-components.md
[DORA metrics]: /frameworks.md#dora-metrics
[ISO 27001:2022]: /frameworks.md#iso27001-2022
[NIST SSDF 1.1]: /frameworks.md#nist-ssdf-1-1
