---
id: pol-0019
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.5.4
  - ISO27001:2022 A.5.36
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - exceptions
  - risk-acceptance
  - governance
---

# Policy: Deviations are recorded, owned and time-bound

## Purpose

Where we knowingly depart from one of these policies or the standards beneath them, the departure is written down,
carries a named person accepting the risk, and has a date by which it is revisited.

Almost every policy here contains an escape hatch — "without a recorded exception". This policy is what makes those
words mean something. An exception granted deliberately and reviewed is risk management; the same decision taken
silently is erosion, and it is indistinguishable afterwards from nobody having known the rule at all.

## Scope

Any knowing departure from a policy in this section or a standard that implements one, in any environment. Applies
whether the departure is permanent, temporary or made under pressure during an incident.

## Commitments

* We **will** record a deviation before departing from a policy or standard, or immediately afterwards where an incident
  left no time.
* We **will** name an individual who accepts the risk — someone with the authority to accept it, never a team or a
  role in the abstract.
* We **will** state what the deviation is, why it is needed and what compensates for it.
* We **will** give every deviation a review date, and honour it.
* We **will** make deviations visible to those affected by the risk, rather than filing them where only the person who
  raised them will look.
* We **will** close a deviation by fixing the underlying gap or by consciously re-accepting the risk — with the same
  scrutiny as the first time.
* We **will not** treat a deviation as permanent by default, or let an expired one stand unreviewed.
* We **will not** accept "we always do it this way" as a substitute for a recorded decision.

## Alignment

| Reference                 | Area                                       |
|---------------------------|--------------------------------------------|
| ISO/IEC 27001:2022 A.5.4  | Management responsibilities                |
| ISO/IEC 27001:2022 A.5.36 | Adherence to policies, rules and standards |

We **align with** these areas. Risk acceptance itself sits in the management-system clauses rather than in Annex A, so
the mapping here is partial by nature. We are not registered against ISO/IEC 27001:2022 and are not audited against it.

## Exceptions

None — an unrecorded exception to the exception policy is exactly the failure mode this exists to prevent.

Two commitments admit no deviation at all, and no record makes them acceptable: embedding a secret
([pol-0004](0004-secrets-are-never-embedded.md)), and processing personal data without a lawful basis
([pol-0005](0005-data-protection.md)). Where personal data may be copied, and for how long, is a question this policy
can answer; whether we were entitled to hold it at all is not.

## Implemented by

No standard implements this directly; it is cited by the standards that carry an exception clause, and by the
[controls](/controls) that verify those clauses are honoured. Where the mechanism for recording a deviation is defined,
it will be defined as a [process](/processes).

_This policy is deliberately mechanism-free — where deviations are recorded is a decision the implementing process
makes._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
