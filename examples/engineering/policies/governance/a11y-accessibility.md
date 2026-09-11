---
id: pol-A11Y
type: policy
tier: normative
status: draft
aligns-with:
  - framework: Equality Act 2010
    clauses: [ s.20, s.29, s.39, Sch.2 ]
  - framework: WCAG 2.2 AA
review-by: "2027-08-04"
owner: human:paul.law
tags: [ accessibility, inclusive-design, legal-obligation ]
---

# Software we build is usable by everyone

`Policy: pol-A11Y` `DRAFT`

## Purpose

The software we put in front of users works for people with disabilities, and meets [WCAG 2.2 AA]. It does that because
we designed it that way, not because we corrected it afterwards.

The [Equality Act 2010] requires this of us, and it requires us to anticipate: we owe the adjustment before anybody
asks. Accessibility is also the difference between software that serves its users and software that serves *most* of
them. Building it in costs a fraction of fixing it later.

## Scope

All user-facing applications and interfaces we build, and the documents they produce: an exported report, a generated
invoice, an email a service sends. Internal tools meet the same standard, because we do not know which of the people
using them needs it.

## Clauses

| Id        | Clause                                                                                                                                 | Alignment                                                                                   |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------|
| `UPFRONT` | **MUST** establish accessibility requirements during design, alongside any other requirement                                           | [Equality Act 2010].Sch.2                                                                   |
| `CONFORM` | **MUST** verify conformance against [WCAG 2.2 AA] before a change reaches users                                                        | [WCAG 2.2 AA], [Equality Act 2010].s.20, [Equality Act 2010].s.29, [Equality Act 2010].s.39 |
| `VENDOR`  | **MUST** assess third-party components against [WCAG 2.2 AA] before we adopt them                                                      | [WCAG 2.2 AA], [Equality Act 2010].s.20                                                     |
| `RECORD`  | **MUST** record where a third-party component falls short of [WCAG 2.2 AA]                                                             |                                                                                             |
| `PUBLISH` | **MUST** publish an accessibility statement where we are required to, named to the individual who accepts it                           |                                                                                             |
| `CURRENT` | **MUST** keep an accessibility statement we publish current and truthful about the gaps that remain                                    |                                                                                             |
| `FIX`     | **MUST** set remediation timeframes for accessibility defects by severity, and track them to closure                                   |                                                                                             |
| `WORSE`   | **MUST NOT** ship a change that knowingly makes accessibility worse without a recorded deviation ([pol-DEVI]) and a plan to correct it |                                                                                             |
| `REPORT`  | SHOULD provide a route for people outside the organisation to report an accessibility barrier, and respond when they do                |                                                                                             |
| `ASSIST`  | SHOULD test with the assistive technologies people actually use, on the journeys that matter most                                      |                                                                                             |
| `INCLUDE` | COULD involve disabled users in research and testing directly, rather than inferring their experience from a checklist                 |                                                                                             |

## Exceptions

A third-party component we cannot replace may fall short of the target standard. We record the gap, we provide an
equivalent route to the same outcome where one exists, and the accessibility statement says so. Recording a gap honestly
is acceptable; concealing it is not.

## Notes

What drives this policy is legal obligation rather than an information-security framework, so it carries no ISO/IEC
27001 reference. See [Policies](../../policies.md#why-we-use-them).

[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[Equality Act 2010]: ../../frameworks.md#equality-act-2010
[WCAG 2.2 AA]: ../../frameworks.md#wcag
