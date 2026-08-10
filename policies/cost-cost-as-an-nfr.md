---
id: pol-COST
tier: normative
category: delivery
status: draft
aligns-with:
review-by: "2027-08-04"
owner: paul.law
tags: [ cost, efficiency, non-functional-requirements ]
---

# Cost is a non-functional requirement

`Policy: pol-COST` `DRAFT`

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

## Clauses

| Id        | Clause                                                                                                                                                                    | Alignment                     |
|-----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------|
| `ATTRIB`  | **MUST** attribute the running cost of a system to the service and the team that owns it                                                                                  | [Azure WAF].cost-optimization |
| `VISIBLE` | **MUST** give the people who own a system visibility of what it costs, at a cadence that lets them act                                                                    | [Azure WAF].cost-optimization |
| `DESIGN`  | **MUST** treat cost as a design consideration, weighed alongside performance and resilience rather than after them                                                        | [Azure WAF].cost-optimization |
| `SIZING`  | **MUST** size resources against what they actually use, and revisit that as usage changes                                                                                 | [Azure WAF].cost-optimization |
| `ANOMALY` | **MUST** notice unexpected cost quickly, and treat a sharp unexplained rise as a signal worth investigating — runaway spend is often a defect, and sometimes a compromise | [Azure WAF].cost-optimization |
| `UNUSED`  | **MUST** remove what we no longer use, including in environments below production                                                                                         | [Azure WAF].cost-optimization |
| `UNOWNED` | **MUST NOT** run a production workload that nobody owns the cost of                                                                                                       | [Azure WAF].cost-optimization |
| `ERODE`   | **MUST NOT** let cost efficiency erode resilience, security or accessibility — this policy sets a constraint to optimise within, not a licence to cut protections         | [Azure WAF].cost-optimization |

## Exceptions

Deliberate over-provisioning for resilience, performance headroom or a known event departs from `SIZING`. It is a cost
decision rather than an oversight, and it is recorded as a deviation under [pol-DEVI], owned by whoever made it.
Short-lived experiments are exempt from right-sizing but not from ownership or from being cleaned up.

## Notes

No ISO/IEC 27001:2022 Annex A area corresponds to cost efficiency — the framework's concern is availability, not
expenditure — so this policy carries no Annex A reference rather than an invented one, and `aligns-with` stays empty.
What it does align with is the [Azure WAF] Cost Optimization pillar, which is the only kind of alignment this policy
will ever have: nothing external obliges an organisation to manage its own spend.

[pol-DEVI]: devi-deviations-are-recorded.md
[Azure WAF]: /frameworks.md#azure-waf
