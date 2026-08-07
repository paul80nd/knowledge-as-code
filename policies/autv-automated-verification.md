---
id: pol-AUTV
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.28
  - ISO27001:2022 A.8.29
  - ISO27001:2022 A.8.33
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ continuous-integration, quality-gates, testing ]
---

# Every change is verified automatically, and failures block

`Policy: pol-AUTV` `DRAFT`

## Purpose

Every change is built and checked automatically before it joins the mainline or moves towards production, and a failed
check stops it. Quality checks are gates, not advisories.

A check that warns but does not block is a check that will eventually be ignored, and the discipline of a permanently
releasable mainline is worth more than any individual gate in it. Automation is what makes this affordable: verification
that depends on someone remembering is verification we do not have.

## Scope

Every change to any solution we build or operate — application code, infrastructure definitions, database schema,
configuration, data pipelines and the documentation held with them.

## Commitments

* We **will** build and verify every change automatically, on integration, without anyone asking for it.
* We **will** treat a failing check as blocking: a red build does not merge and does not promote.
* We **will** be able to reproduce a build from version control alone, on any machine set up to do so.
* We **will** test at the levels the change warrants, fast enough that the feedback arrives while the work is still in
  hand.
* We **will** add a regression test for every defect we fix, so it can only be found once.
* We **will** treat a broken mainline as the team's first priority.
* We **will not** merge or release over a failing check without a recorded deviation ([pol-DEVI]).
* We **will not** disable, skip or suppress a check to make a release possible, and **will not** silence a warning
  without a recorded reason.
* We **will not** depend on a particular person's machine to produce a build.

## Alignment

| Reference                 | Area                                           |
|---------------------------|------------------------------------------------|
| ISO/IEC 27001:2022 A.8.25 | Secure development lifecycle                   |
| ISO/IEC 27001:2022 A.8.28 | Secure coding                                  |
| ISO/IEC 27001:2022 A.8.29 | Security testing in development and acceptance |
| ISO/IEC 27001:2022 A.8.33 | Test information                               |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

An emergency fix to restore service may bypass a non-security gate where the delay would extend an outage. It is
recorded as a deviation under [pol-DEVI], and the gate is satisfied afterwards rather than waived — the exception buys
time, not forgiveness.

## Implemented by

Intended implementing standards: continuous integration, test strategy and quality assurance, secure coding and code
quality, database lifecycle, application security testing, and — where data pipelines form part of the estate — the data
pipeline standard.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-DEVI]: devi-deviations-are-recorded.md
