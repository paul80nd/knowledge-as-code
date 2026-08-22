---
id: pol-A11Y
tier: normative
category: governance
status: draft
aligns-with:
review-by: "2027-08-04"
owner: paul.law
tags: [ accessibility, inclusive-design, legal-obligation ]
---

# Software we build is usable by everyone

`Policy: pol-A11Y` `DRAFT`

## Purpose

The software we put in front of users works for people with disabilities, and meets [WCAG 2.2 AA]. It does that because
we designed it that way, not because we corrected it afterwards.

The law in the markets we serve requires this. It is also the difference between software that serves its users and
software that serves _most_ of them. Building it in costs a fraction of fixing it later.

## Scope

All user-facing applications and interfaces we build. Internal tools meet the same standard, because we do not know
which of the people using them needs it.

## Clauses

| Id        | Clause                                                                                                                                 | Alignment                      |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------|--------------------------------|
| `UPFRONT` | **MUST** establish accessibility requirements during design, alongside any other requirement                                           |                                |
| `CONFORM` | **MUST** verify conformance against [WCAG 2.2 AA] before a change reaches users                                                        | [WCAG 2.2 AA], [EN 301 549].§9 |
| `VENDOR`  | **MUST** assess third-party components against [WCAG 2.2 AA] before we adopt them, and record what falls short                         | [EN 301 549].§9                |
| `PUBLISH` | **MUST** publish an accessibility statement where we are required to, and keep it truthful about the gaps that remain                  | [PSBAR 2018].reg.8             |
| `WORSE`   | **MUST NOT** ship a change that knowingly makes accessibility worse without a recorded deviation ([pol-DEVI]) and a plan to correct it |                                |
| `ASSIST`  | SHOULD test with the assistive technologies people actually use, on the journeys that matter most                                      |                                |
| `INCLUDE` | COULD involve disabled users in research and testing directly, rather than inferring their experience from a checklist                 |                                |

## Exceptions

A third-party component we cannot replace may fall short of the target standard. We record the gap, we provide an
equivalent route to the same outcome where one exists, and the accessibility statement says so. Recording a gap honestly
is acceptable; concealing it is not.

## Notes

What drives this policy is legal obligation rather than an information-security framework, so it carries no ISO/IEC
27001 reference. See [Policies](../policies.md#why-we-use-them).

[pol-DEVI]: devi-deviations-are-recorded.md
[EN 301 549]: ../frameworks.md#en-301-549
[PSBAR 2018]: ../frameworks.md#psbar-2018
[WCAG 2.2 AA]: ../frameworks.md#wcag
