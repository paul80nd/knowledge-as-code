---
id: pol-INCR
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.5, A.5.24, A.5.25, A.5.26, A.5.27, A.5.28, A.5.29, A.6.8 ]
  - framework: UK GDPR
    clauses: [ Art.32(1)(d), Art.33, Art.33(5), Art.34 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ incident-response, learning, postmortem ]
---

# Incidents are managed and learned from

`Policy: pol-INCR` `DRAFT`

## Purpose

When something goes wrong, we have a defined way to respond. Someone is in charge, and the severity sets how large the
response is. We record what happened, and we can show we are better for it.

An undefined response costs time while people work out who decides. A review finds the conditions that allowed the
incident, so the next one is less likely.

## Scope

Security and operational incidents affecting systems we build or operate, including those reported to us from outside
the organisation. A personal data breach is a security incident and is in scope here. [pol-DATA] states what the data
itself requires of us.

_Boundary: [pol-RECV] owns being able to recover: the objectives, the backups and the proof that a restore works. This
policy owns the decision to recover and the recovery itself, against the objectives [pol-RECV] sets. `ACTIONS` is shared
with [pol-SECD]: findings become tracked work whether an incident review here or threat modelling there produced them._

## Clauses

| Id        | Clause                                                                                                                                                                  | Alignment                                       |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------|
| `PROCESS` | **MUST** maintain a defined incident-response process with clear roles, so that during an incident it is never ambiguous who decides                                    | [ISO 27001:2022].A.5.24                         |
| `DECLARE` | **MUST** decide whether a security event is an incident against stated criteria, and record who decided                                                                 | [ISO 27001:2022].A.5.25                         |
| `HARM`    | **MUST** triage and assess impact as soon as possible, where an incident could cause personal harm, leaving enough of the statutory window for `NOTIFY` to still be met | [ISO 27001:2022].A.5.25                         |
| `AVERTED` | **MUST** record a near miss: a risk identified before it becomes an incident                                                                                            | [ISO 27001:2022].A.5.24                         |
| `TRIAGE`  | **MUST** classify incidents by severity, and escalate according to it                                                                                                   |                                                 |
| `COMMS`   | **MUST** communicate an incident's status to those it affects, at the cadence its severity sets                                                                         | [ISO 27001:2022].A.5.26                         |
| `RECOVER` | **MUST** invoke the recovery path defined for the affected system rather than improvising one                                                                           |                                                 |
| `HOLD`    | **MUST** keep security controls in force during an incident, or record what compensates for one that is set aside                                                       | [ISO 27001:2022].A.5.29                         |
| `EVIDENC` | **MUST** preserve evidence and produce a record for every significant incident, and for every personal data breach whether or not it is notifiable                      | [ISO 27001:2022].A.5.26, [UK GDPR].Art.33(5)    |
| `FREEZE`  | **MUST** preserve the state an incident is investigated from, before recovery destroys it                                                                               | [ISO 27001:2022].A.5.28                         |
| `NOTIFY`  | **MUST** notify the supervisory authority of a personal data breach within the statutory window                                                                         | [ISO 27001:2022].A.5.5, [UK GDPR].Art.33        |
| `INFORM`  | **MUST** tell the people a personal data breach puts at high risk, without undue delay                                                                                  | [UK GDPR].Art.34                                |
| `REPORT`  | **MUST** encourage anyone to report a suspected incident, and make it easy to do so                                                                                     | [ISO 27001:2022].A.6.8                          |
| `WINDOW`  | **MUST** report a suspected incident within 24 hours of it being identified                                                                                             | [ISO 27001:2022].A.6.8                          |
| `CLOSED`  | **MUST** notify whoever reported an incident of the outcome, once it is closed                                                                                          | [ISO 27001:2022].A.6.8                          |
| `LEARN`   | **MUST** review significant incidents afterwards, looking for the conditions that allowed them rather than for someone to blame                                         | [ISO 27001:2022].A.5.27                         |
| `ACTIONS` | **MUST** turn the findings of those reviews into tracked work ([pol-SECD].ACTIONS states the same for a security design review)                                         | [ISO 27001:2022].A.5.27                         |
| `TRENDS`  | **MUST** review incidents in aggregate on a defined cadence, looking for a pattern no single review would surface                                                       | [ISO 27001:2022].A.5.27                         |
| `DRILL`   | **MUST** rehearse the process rather than first exercising it for real                                                                                                  | [ISO 27001:2022].A.5.24, [UK GDPR].Art.32(1)(d) |
| `ADHOC`   | **MUST NOT** handle a significant incident informally, with no record and no named owner                                                                                | [ISO 27001:2022].A.5.26                         |
| `TOOSOON` | **MUST NOT** close an incident before the learning from it is captured                                                                                                  | [ISO 27001:2022].A.5.27                         |
| `BUILDER` | SHOULD put the people who built a system in the response path when it fails                                                                                             |                                                 |

## Exceptions

Routine work handles a low-severity event, in place of the full process. The severity classification decides which
events those are, and we apply it deliberately: convenience does not set it. Recording a significant incident has no
exception, and recording a personal data breach has none. `EVIDENC` binds at every severity. A decision that a breach
was not notifiable is itself a decision we have to be able to show.

## Notes

`WINDOW` and `HARM` read inconsistently side by side: one names a fixed deadline, the other doesn't. `WINDOW` binds
before anyone knows whether personal harm is possible, so it cannot be conditioned the way `HARM` is, and it mirrors a
company-wide staff commitment rather than an engineering-specific figure. `HARM` protects `NOTIFY`'s statutory window
instead of naming its own ceiling, since a fixed number there risks leaving whoever handles notification too little time
regardless of how long `WINDOW`'s step took.

[pol-DATA]: ../security/data-data-protection.md
[pol-RECV]: ../operations/recv-recoverability.md
[pol-SECD]: ../security/secd-security-by-design.md#clauses
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[UK GDPR]: ../../frameworks.md#uk-gdpr
