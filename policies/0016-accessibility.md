---
id: pol-0016
tier: normative
status: draft
aligns-with:
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - accessibility
  - inclusive-design
  - legal-obligation
---

# Policy: Software we build is usable by everyone

## Purpose

The software we put in front of users is usable by people with disabilities, meeting the accessibility standard we
target, because it was designed that way rather than corrected afterwards.

This is an obligation in law in the markets we serve, and it is also simply the difference between software that serves
its users and software that serves most of them. Accessibility designed in costs a fraction of accessibility retrofitted,
because the expensive failures are structural ones no late fix reaches.

## Scope

All user-facing applications and interfaces we build. Internal tools are held to the same standard where anyone using
them may need it — which is to say, always, since we do not know who that is.

## Commitments

* We **will** meet the accessibility standard the organisation targets for user-facing software.
* We **will** consider accessibility at design, as a requirement alongside any other.
* We **will** test for accessibility, including with the assistive technologies people actually use, on the journeys
  that matter most.
* We **will** publish an accessibility statement where we are required to, and keep it truthful about the gaps that
  remain.
* We **will not** ship a change that knowingly makes accessibility worse without a recorded deviation
  ([pol-0019](0019-recorded-deviations.md)) and a plan to correct it.
* We **will not** treat accessibility as a phase that follows delivery.

## Exceptions

Where a third-party component we cannot replace falls short of the target standard, the gap is recorded, an equivalent
route to the same outcome is provided where one is possible, and the statement says so. Recording a gap honestly is
acceptable; concealing it is not.

## Implemented by

Intended implementing standard: accessibility.

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until that standard id does._

_This policy's driver is legal obligation rather than an information-security framework, so it carries no ISO alignment —
an invented mapping would be worse than none._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
