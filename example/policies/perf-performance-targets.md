---
id: pol-PERF
tier: normative
category: delivery
status: draft
aligns-with:
  - ISO27001:2022 A.8.6
review-by: "2027-08-04"
owner: paul.law
tags: [ capacity, non-functional-requirements, performance ]
---

# Performance targets are stated and verified

`Policy: pol-PERF` `DRAFT`

## Purpose

Where performance matters, we say what "fast enough" means before we build, and we check against it before we release
rather than discovering the answer from customers.

An unstated performance target is met by definition, right up to the point where it is not. Stating it converts an
argument about whether something feels slow into a measurement, and makes a regression something the pipeline catches.

## Scope

Systems where throughput, latency, concurrency or capacity affect whether the system does its job, which is most
customer-facing systems and many internal ones. Targets themselves are recorded as [NFRs](/nfrs). This policy commits
us to having them.

## Clauses

| Id        | Clause                                                                                                                                        | Alignment                                                  |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| `TARGETS` | **MUST** state performance and capacity targets for systems where performance matters, in terms that can be measured                          | [ISO 27001:2022].A.8.6, [Azure WAF].performance-efficiency |
| `MEASURE` | **MUST** validate against those targets before significant releases, under conditions representative of real load rather than convenient load | [ISO 27001:2022].A.8.6, [Azure WAF].performance-efficiency |
| `DEFECT`  | **MUST** treat a target we no longer meet as a defect, not as a new baseline                                                                  | [Azure WAF].performance-efficiency                         |
| `PEAK`    | **MUST** understand behaviour at peak and over time, not only at the average case on a quiet afternoon                                        | [ISO 27001:2022].A.8.6, [Azure WAF].performance-efficiency |
| `NOTEST`  | **MUST NOT** release a significant change to a performance-sensitive system with no performance validation at all                             | [Azure WAF].performance-efficiency                         |

## Exceptions

Systems where performance is genuinely not a concern (low-volume internal tooling with no user waiting on it) need no
targets. That judgement is a recorded deviation under [pol-DEVI] rather than an assumption repeated, and its review date
is what catches the system that has since acquired users.

[pol-DEVI]: devi-deviations-are-recorded.md
[Azure WAF]: /frameworks.md#azure-well-architected-framework
[ISO 27001:2022]: /frameworks.md#iso-27001
