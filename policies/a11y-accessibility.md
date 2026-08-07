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

The software we put in front of users is usable by people with disabilities, meeting [WCAG 2.2 AA], because it was
designed that way rather than corrected afterwards.

This is an obligation in law in the markets we serve, and it is also simply the difference between software that serves
its users and software that serves most of them. Accessibility designed in costs a fraction of accessibility
retrofitted, because the expensive failures are structural ones no late fix reaches.

## Scope

All user-facing applications and interfaces we build. Internal tools are held to the same standard where anyone using
them may need it — which is to say, always, since we do not know who that is.

## Clauses

| Id        | Clause                                                                                                                                  | Alignment                      |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------|--------------------------------|
| `DESIGN`  | **MUST** establish accessibility requirements during design, alongside any other requirement                                            |                                |
| `VERIFY`  | **MUST** verify conformance against [WCAG 2.2 AA] before a change reaches users                                                         | [WCAG 2.2 AA], [EN 301 549].§9 |
| `VENDOR`  | **MUST** assess third-party components against [WCAG 2.2 AA] before we adopt them, and record what falls short                          | [EN 301 549].§9                |
| `PUBLISH` | **MUST** publish an accessibility statement where we are required to, and keep it truthful about the gaps that remain                   | [PSBAR 2018].reg.8             |
| `REGRESS` | **MUST NOT** ship a change that knowingly makes accessibility worse without a recorded deviation ([pol-DEVI]) and a plan to correct it  |                                |

## Exceptions

Where a third-party component we cannot replace falls short of the target standard, the gap is recorded, an equivalent
route to the same outcome is provided where one is possible, and the statement says so. Recording a gap honestly is
acceptable; concealing it is not.

## Notes

This policy's driver is legal obligation rather than an information-security framework. Framework alignment is recorded
per clause where a genuine mapping exists and left absent where it does not; an invented mapping would be worse than
none.

[pol-DEVI]: devi-deviations-are-recorded.md
[EN 301 549]: /frameworks.md#en-301-549
[PSBAR 2018]: /frameworks.md#psbar-2018
[WCAG 2.2 AA]: /frameworks.md#wcag-22-aa
