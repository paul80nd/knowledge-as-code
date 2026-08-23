---
id: pol-AUTV
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.28
  - ISO27001:2022 A.8.29
review-by: "2027-08-04"
owner: paul.law
tags: [ continuous-integration, quality-gates, testing ]
---

# Every change is verified automatically, and failures block

`Policy: pol-AUTV` `DRAFT`

## Purpose

We build and check every change automatically before it joins the default branch or moves towards production. A failed
check stops it. Quality checks are gates.

A check that warns but does not block is a check that will eventually be ignored. Keeping the default branch releasable
is worth more than any single gate in it. Gating every change is only affordable because machines do the checking. A
check somebody has to remember to run is a check we do not have.

## Scope

Every change to any solution we build or operate: application code, infrastructure definitions, database schema,
configuration, data pipelines and the documentation held with them.

## Clauses

| Id        | Clause                                                                                                                 | Alignment                                                                                        |
|-----------|------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| `INTEG`   | **MUST** build and verify every change automatically, on integration, without anyone asking for it                     | [ISO 27001:2022].A.8.25, [ISO 27001:2022].A.8.28, [DORA metrics].lead-time, [NIST SSDF 1.1].PO.3 |
| `BLOCK`   | **MUST** treat a failing check as blocking: a red build does not merge and does not promote                            | [ISO 27001:2022].A.8.29, [DORA metrics].change-failure-rate, [NIST SSDF 1.1].PO.4                |
| `REPRO`   | **MUST** be able to reproduce a build from version control alone, on any machine set up to do so                       | [NIST SSDF 1.1].PW.6                                                                             |
| `LEVELS`  | **MUST** test at the levels the change warrants, fast enough that the feedback arrives while the work is still in hand | [ISO 27001:2022].A.8.29, [DORA metrics].lead-time, [NIST SSDF 1.1].PW.8                          |
| `REGRESS` | **MUST** add a regression test for every defect we fix, so it can only be found once. See [pol-VURM]                   | [ISO 27001:2022].A.8.29, [DORA metrics].change-failure-rate, [NIST SSDF 1.1].PW.8                |
| `BROKEN`  | **MUST** treat a broken default branch as the team's first priority                                                    |                                                                                                  |
| `BYPASS`  | **MUST NOT** merge or release over a failing check without a recorded deviation ([pol-DEVI])                           | [ISO 27001:2022].A.8.29, [NIST SSDF 1.1].PO.4                                                    |
| `DISABLE` | **MUST NOT** disable, skip, silence or suppress a check or a warning without a recorded deviation ([pol-DEVI])         | [NIST SSDF 1.1].PO.4                                                                             |
| `MACHINE` | **MUST NOT** depend on a particular person's machine to produce a build                                                | [NIST SSDF 1.1].PO.3                                                                             |
| `OFTEN`   | SHOULD integrate to the default branch often enough that any one change is small enough to reason about                | [DORA metrics].lead-time                                                                         |
| `WARN`    | SHOULD treat a new warning as a defect to triage rather than noise to accumulate                                       |                                                                                                  |
| `COVER`   | SHOULD know what proportion of our code the tests exercise, and notice when it falls                                   |                                                                                                  |
| `BITWISE` | COULD produce byte-for-byte identical output from the same source                                                      | [NIST SSDF 1.1].PW.6                                                                             |

## Exceptions

An emergency fix to restore service may bypass a non-security gate where the delay would extend an outage. It is
recorded as a deviation under [pol-DEVI], and the gate is satisfied afterwards rather than waived.

[pol-DEVI]: devi-deviations-are-recorded.md
[pol-VURM]: vurm-vulnerability-remediation.md
[DORA metrics]: ../frameworks.md#dora-metrics
[ISO 27001:2022]: ../frameworks.md#iso-27001
[NIST SSDF 1.1]: ../frameworks.md#nist-ssdf
