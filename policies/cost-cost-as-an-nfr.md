---
id: pol-COST
tier: normative
status: draft
aligns-with:
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - cost
  - efficiency
  - non-functional-requirements
---

# Policy: Cost is a non-functional requirement

## Purpose

What a system costs to run is an engineering property of that system, like its latency or its availability. It has an
owner, it is visible to the people whose decisions move it, and it is designed for rather than discovered in an invoice.

Treating cost as a finance concern puts the accountability a long way from the design decisions that set it. The
architecture, the data volumes and the resource choices are made by engineers, so engineers are the people who can act
on them — and the same discipline we apply to any other non-functional requirement works here without modification.

## Scope

All systems we operate on metered infrastructure, in every environment. Targets and thresholds are recorded as
[NFRs](/nfrs) where they matter enough to be stated; this policy commits us to the ownership and the visibility that
make them meaningful.

## Commitments

* We **will** attribute the running cost of a system to the service and the team that owns it.
* We **will** give the people who own a system visibility of what it costs, at a cadence that lets them act.
* We **will** treat cost as a design consideration, weighed alongside performance and resilience rather than after them.
* We **will** size resources against what they actually use, and revisit that as usage changes.
* We **will** notice unexpected cost quickly, and treat a sharp unexplained rise as a signal worth investigating —
  runaway spend is often a defect, and sometimes a compromise.
* We **will** remove what we no longer use, including in environments below production.
* We **will not** run a production workload that nobody owns the cost of.
* We **will not** let cost efficiency erode resilience, security or accessibility — this policy sets a constraint to
  optimise within, not a licence to cut protections.

## Exceptions

Deliberate over-provisioning for resilience, performance headroom or a known event is not a breach of this policy; it is
a cost decision, recorded as one and owned by whoever made it. Short-lived experiments are exempt from right-sizing but
not from ownership or from being cleaned up.

## Implemented by

Intended implementing standard: cloud cost and efficiency.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until that standard id does._

_No ISO/IEC 27001:2022 Annex A area corresponds to cost efficiency — the framework's concern is availability, not
expenditure — so this policy records no alignment rather than an invented one._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
