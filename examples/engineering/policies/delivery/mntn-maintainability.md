---
id: pol-MNTN
type: policy
tier: normative
status: draft
review-by: "2027-09-11"
owner: human:paul.law
tags: [ complexity, maintainability, technical-debt ]
---

# Software stays easy to change

`Policy: pol-MNTN` `DRAFT`

## Purpose

How hard our software is to change is a property we measure and own, the way we own what it costs to run and how fast
it responds. We watch it move, and we act when it moves the wrong way.

Any measure of this is arguable, and most are gameable. The direction it moves in is neither. We watch the trend
because the failure this policy exists to catch is a gradual one. Nobody sets out to make a system unmaintainable.
Everybody who adds one more special case is making a reasonable decision, and the bill arrives years later as a slower
answer to somebody else's request.

## Scope

Everything we author and keep: application code, infrastructure definitions, database schema and pipeline definitions.
A system small enough to skip is the one that outlives the team that wrote it, so it is in scope too. What good looks
like for each measure is recorded as an NFR, and this policy commits us to having the measures.

_Boundary: [pol-COST] owns what a system costs to run and [pol-PERF] owns how fast it responds. This policy owns what it
costs to change. A shortcut somebody took knowingly is [pol-DEVI]'s, under its own `DEBT`._

## Clauses

| Id       | Clause                                                                                         | Alignment                |
|----------|------------------------------------------------------------------------------------------------|--------------------------|
| `TREND`  | **MUST** measure how costly our software is to change, in terms that can be compared over time |                          |
| `SLIP`   | **MUST** treat a measure moving the wrong way as a defect rather than as the new normal        | [DORA metrics].lead-time |
| `DEAD`   | **MUST** remove code, configuration and definitions that nothing uses                          |                          |
| `HARDER` | **MUST NOT** knowingly make a component harder to change without recording why ([pol-DEVI])    | [DORA metrics].lead-time |

## Exceptions

A prototype written to answer a question and then deleted is exempt, and the deletion is what earns the exemption. A
prototype that ships is not a prototype. Keeping a component we can no longer afford to change is a recorded deviation
under [pol-DEVI], naming who accepts the slower delivery that follows.

## Notes

No framework on our register covers how easy software is to change, so this policy carries no `aligns-with`. See
[Policies](../../policies.md#why-we-use-them). `SLIP` and `HARDER` cite [DORA metrics].lead-time as a claim that acting
on either one moves that measure, rather than as a control we satisfy.

[pol-COST]: ../delivery/cost-cost-as-an-nfr.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-PERF]: ../delivery/perf-performance-targets.md
[DORA metrics]: ../../frameworks.md#dora-metrics
