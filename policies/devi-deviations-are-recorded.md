---
id: pol-DEVI
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.5.4
  - ISO27001:2022 A.5.36
review-by: "2027-08-04"
owner: paul.law
tags: [ exceptions, governance, risk-acceptance ]
---

# Deviations are recorded, owned and time-bound

`Policy: pol-DEVI` `DRAFT`

## Purpose

Where we knowingly depart from one of these policies or the standards beneath them, the departure is written down,
carries a named person accepting the risk, and has a date by which it is revisited. A shortcut taken knowing it will
have to be revisited is the same decision without a rule to break, and is held the same way.

Almost every policy here contains an escape hatch — "without a recorded deviation". This policy is what makes those
words mean something. An exception granted deliberately and reviewed is risk management; the same decision taken
silently is erosion, and it is indistinguishable afterwards from nobody having known the rule at all.

## Scope

Any knowing departure from a policy in this section or a standard that implements one, in any environment, and any
shortcut taken knowing it will have to be revisited. Applies whether the departure is permanent, temporary or made under
pressure during an incident.

## Clauses

| Id        | Clause                                                                                                                                     | Alignment               |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `RECORD`  | **MUST** record a deviation before departing from a policy or standard, or immediately afterwards where an incident left no time           | [ISO 27001:2022].A.5.36 |
| `OWNER`   | **MUST** name an individual who accepts the risk — someone with the authority to accept it, never a team or a role in the abstract         | [ISO 27001:2022].A.5.4  |
| `CONTENT` | **MUST** state what the deviation is, why it is needed and what compensates for it                                                         |                         |
| `EXPIRY`  | **MUST** give every deviation a review date, and honour it                                                                                 |                         |
| `VISIBLE` | **MUST** make deviations visible to those affected by the risk, rather than filing them where only the person who raised them will look    |                         |
| `CLOSE`   | **MUST** close a deviation by fixing the underlying gap or by consciously re-accepting the risk — with the same scrutiny as the first time | [ISO 27001:2022].A.5.36 |
| `PERM`    | **MUST NOT** treat a deviation as permanent by default, or let an expired one stand unreviewed                                             | [ISO 27001:2022].A.5.36 |
| `CUSTOM`  | **MUST NOT** accept "we always do it this way" as a substitute for a recorded decision                                                     | [ISO 27001:2022].A.5.4  |
| `DEBT`    | SHOULD record a shortcut taken knowingly, so that it is tracked work rather than something the next person discovers                       |                         |

## Exceptions

None — an unrecorded exception to the exception policy is exactly the failure mode this exists to prevent.

Six commitments in this section admit no deviation at all, and no record makes any of them acceptable:

* [pol-SCRT] — embedding a secret, reusing a production secret outside production, or writing one to a log. Its four
  operational obligations are deviable; those three prohibitions are not.
* [pol-DATA] — handling personal data on a lawful basis. Its other obligations are deviable.
* [pol-KNOW] — writing down what is needed to build, run and recover a system. The effort is proportionate to the
  system; the commitment does not vary.
* [pol-AGNT] — the acceptance gate. Agent-produced work carries no authority until a person accepts it, and convenience
  does not make it authoritative.
* [pol-INCR] — recording a significant incident, and recording a personal data breach whether or not it is notifiable.
* This policy, for the reason above.

## Notes

No standard implements this directly; it is cited by the standards that carry an exception clause, and by the
[controls](/controls) that verify those clauses are honoured. Where the mechanism for recording a deviation is defined,
it will be defined as a [process](/processes) — this policy is deliberately mechanism-free, because where deviations are
recorded is a decision the implementing process makes.

[pol-AGNT]: agnt-agents-propose-people-decide.md
[pol-DATA]: data-data-protection.md
[pol-INCR]: incr-incident-response.md
[pol-KNOW]: know-knowledge-is-written-down.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
