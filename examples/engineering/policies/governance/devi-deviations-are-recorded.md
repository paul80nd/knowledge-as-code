---
id: pol-DEVI
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.4, A.5.36 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ exceptions, governance, risk-acceptance ]
---

# Deviations are recorded, owned and time-bound

`Policy: pol-DEVI` `DRAFT`

## Purpose

Sometimes we knowingly break one of these policies, or a standard beneath it. When we do, we write down what we did,
name the person accepting the risk, and set a date to look at it again.

The same holds for a shortcut that breaks no rule: skipping the retry logic to ship on Friday, knowing someone else will
have to add it. That is technical debt. Nothing here forbids it, but the debt is real, so we record it the same way.

Almost every policy here leaves a way out, in the words "without a recorded deviation". Those words point here. A
departure someone decided on, wrote down and gave a review date is risk management. The same departure taken quietly is
erosion: a year later nobody can tell it from never having known the rule.

## Scope

Any knowing departure from a policy here, or from a standard that implements one, in any environment. Also, any shortcut
taken knowing someone will have to undo it. This binds whether the departure is permanent, temporary, or made under
pressure during an incident.

## Clauses

| Id        | Clause                                                                                                                                    | Alignment               |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `RECORD`  | **MUST** record a deviation before departing from a policy or standard, or immediately afterwards where an incident left no time          | [ISO 27001:2022].A.5.36 |
| `OWNER`   | **MUST** name an individual who accepts the risk: someone with the authority to accept it, never a team or a role in the abstract         | [ISO 27001:2022].A.5.4  |
| `CONTENT` | **MUST** state what the deviation is, why it is needed and what compensates for it                                                        |                         |
| `EXPIRY`  | **MUST** give every deviation a review date                                                                                               |                         |
| `SURFACE` | **MUST** make deviations visible to those affected by the risk, rather than filing them where only the person who raised them will look   |                         |
| `CLOSE`   | **MUST** close a deviation by fixing the underlying gap or by consciously re-accepting the risk, with the same scrutiny as the first time | [ISO 27001:2022].A.5.36 |
| `PERM`    | **MUST NOT** treat a deviation as permanent by default, or let an expired one stand unreviewed                                            | [ISO 27001:2022].A.5.36 |
| `CUSTOM`  | **MUST NOT** treat a long-standing practice as exempt from a policy it breaks                                                             | [ISO 27001:2022].A.5.4  |
| `DEBT`    | SHOULD record a shortcut taken knowingly, so that it is tracked work rather than something the next person discovers                      |                         |

## Exceptions

None. Departing from this policy means not recording a deviation, which is the one thing it exists to stop.

Some commitments admit no deviation at all, and no record makes any of them acceptable:

* [pol-SCRT].EMBED, [pol-SCRT].REUSE and [pol-SCRT].LOGS. The operational obligations beside them are deviable.
* [pol-DATA].LAWFUL. Its other obligations are deviable.
* [pol-KNOW].DOCS, at an effort proportionate to the system. The commitment does not vary with the effort.
* [pol-AGNT].ACCEPT. Being in a hurry does not make agent output authoritative.
* [pol-INCR].EVIDENC, whether or not a breach is notifiable.
* This policy, for the reason given above.

## Notes

No standard implements this directly. Standards cite it wherever they carry an exception clause, and so do the
[controls](../../controls.md) that check those clauses are honoured.

This policy says nothing about where a deviation is recorded. That belongs to the process that carries it, so that
changing where we file deviations does not change what we committed to.

[pol-AGNT]: ../governance/agnt-agents-propose-people-decide.md#clauses
[pol-DATA]: ../security/data-data-protection.md#clauses
[pol-INCR]: ../operations/incr-incident-response.md#clauses
[pol-KNOW]: ../governance/know-knowledge-is-written-down.md#clauses
[pol-SCRT]: ../security/scrt-secrets-are-never-embedded.md#clauses
[ISO 27001:2022]: ../../frameworks.md#iso-27001
