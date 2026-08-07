---
id: pol-PIPE
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.9
  - ISO27001:2022 A.8.19
  - ISO27001:2022 A.8.32
implemented-by:
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

## Commitments

* We **will** deploy to production only through an automated pipeline.
* We **will** deploy only artifacts the pipeline itself produced, promoting the same artifact through environments
  rather than rebuilding per stage.
* We **will** be able to trace any production release to the change, the artifact and the approval behind it.
* We **will** have a defined rollback or recovery path before a change goes to production.
* We **will** hold the pipeline itself in version control, as a reviewed artifact like any other.
* We **will** carry the safeguards that change approval exists to provide inside the pipeline, rather than treating
  automation as a reason to drop them.
* We **will not** hand-edit production, whether code, configuration, infrastructure or schema.
* We **will not** deploy an artifact built locally or obtained from an unverified source.

## Alignment

| Reference                 | Area                                            |
|---------------------------|-------------------------------------------------|
| ISO/IEC 27001:2022 A.8.9  | Configuration management                        |
| ISO/IEC 27001:2022 A.8.19 | Installation of software on operational systems |
| ISO/IEC 27001:2022 A.8.32 | Change management                               |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Emergency change to restore service may bypass the normal path where the pipeline is itself unavailable or the delay
would extend an outage. It is recorded as a deviation under [pol-DEVI] at the time, and the change is reconciled back
into version control before the incident is closed — otherwise the fix becomes the next outage.

[pol-DEVI]: devi-deviations-are-recorded.md
