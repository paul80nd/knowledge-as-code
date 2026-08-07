---
id: pol-PERF
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.6
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ capacity, non-functional-requirements, performance ]
---

# Performance targets are stated and verified

`Policy: pol-PERF` `DRAFT`

## Purpose

Where performance matters, we say what "fast enough" means before we build, and we check against it before we release
rather than discovering the answer from customers.

An unstated performance target is met by definition, right up to the point where it isn't. Stating it converts an
argument about whether something feels slow into a measurement, and makes a regression something the pipeline can catch
rather than something the support queue reports.

## Scope

Systems where throughput, latency, concurrency or capacity affect whether the system does its job — which is most
customer-facing systems and many internal ones. Targets themselves are recorded as [NFRs](/nfrs); this policy commits us
to having them.

## Commitments

* We **will** state performance and capacity targets for systems where performance matters, in terms that can be
  measured.
* We **will** validate against those targets before significant releases, under conditions representative of real load
  rather than convenient load.
* We **will** treat a target we no longer meet as a defect, not as a new baseline.
* We **will** understand behaviour at peak and over time, not only at the average case on a quiet afternoon.
* We **will not** release a significant change to a performance-sensitive system with no performance validation at all.
* We **will not** allow an unstated target to become the reason nobody is accountable for a slow system.

## Alignment

| Reference                | Area                |
|--------------------------|---------------------|
| ISO/IEC 27001:2022 A.8.6 | Capacity management |

We **align with** this area, which covers the availability half of this policy. The rest is engineering practice with no
corresponding control. We are not registered against ISO/IEC 27001:2022 and are not audited against it.

## Exceptions

Systems where performance is genuinely not a concern — low-volume internal tooling with no user waiting on it — need no
targets. That judgement is recorded once rather than assumed repeatedly, because systems acquire users.
