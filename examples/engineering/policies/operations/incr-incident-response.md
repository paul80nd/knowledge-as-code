---
id: pol-INCR
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.5, A.5.24, A.5.25, A.5.26, A.5.27, A.5.28, A.5.29, A.6.8 ]
  - framework: UK GDPR
    clauses: [ Art.33, Art.33(5), Art.34 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ incident-response, learning, postmortem ]
---

# Incidents are managed and learned from

`Policy: pol-INCR` `DRAFT`

## Purpose

When something goes wrong, we have a defined way to respond. Someone is in charge and severity decides how large the
response is. We record what happened, and we can show we are better for having been through it.

Respond well and an incident ends in a recovery. Respond badly and it becomes a second failure on top of the first.
Review it afterwards and we do not pay for it twice.

## Scope

Security and operational incidents affecting systems we build or operate, including those reported to us from outside
the organisation. A personal data breach is a security incident and is in scope here. What the data itself requires of
us is [pol-DATA]'s.

_Boundary: [pol-RECV] owns being able to recover, and that covers the objectives, the backups, and the proof that a
restore works. This policy owns deciding to recover and doing it, held to the objectives [pol-RECV] set. `ACTIONS` is
shared with [pol-SECD]: findings become tracked work whether they came from an incident review here or from threat
modelling there._

## Clauses

| Id        | Clause                                                                                                                                             | Alignment                                                             |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------|
| `PROCESS` | **MUST** maintain a defined incident-response process with clear roles, so that during an incident it is never ambiguous who decides               | [ISO 27001:2022].A.5.24                                               |
| `TRIAGE`  | **MUST** classify incidents by severity, and escalate according to it                                                                              | [ISO 27001:2022].A.5.25                                               |
| `COMMS`   | **MUST** communicate an incident's status to those it affects, at the cadence its severity sets                                                    | [ISO 27001:2022].A.5.26                                               |
| `RECOVER` | **MUST** invoke the recovery path defined for the affected system rather than improvising one                                                      | [ISO 27001:2022].A.5.29                                               |
| `EVIDENC` | **MUST** preserve evidence and produce a record for every significant incident, and for every personal data breach whether or not it is notifiable | [ISO 27001:2022].A.5.26, [ISO 27001:2022].A.5.28, [UK GDPR].Art.33(5) |
| `NOTIFY`  | **MUST** notify the supervisory authority of a personal data breach within the statutory window                                                    | [ISO 27001:2022].A.5.5, [UK GDPR].Art.33                              |
| `INFORM`  | **MUST** tell the people a personal data breach puts at high risk, without undue delay                                                             | [UK GDPR].Art.34                                                      |
| `REPORT`  | **MUST** encourage anyone to report a suspected incident, and make it easy to do so                                                                | [ISO 27001:2022].A.6.8                                                |
| `LEARN`   | **MUST** review significant incidents afterwards, looking for the conditions that allowed them rather than for someone to blame                    | [ISO 27001:2022].A.5.27                                               |
| `ACTIONS` | **MUST** turn the findings of those reviews into tracked work. See [pol-SECD]                                                                      | [ISO 27001:2022].A.5.27                                               |
| `DRILL`   | **MUST** rehearse the process rather than first exercising it for real                                                                             | [ISO 27001:2022].A.5.24                                               |
| `ADHOC`   | **MUST NOT** handle a significant incident informally, with no record and no named owner                                                           | [ISO 27001:2022].A.5.26                                               |
| `TOOSOON` | **MUST NOT** close an incident before the learning from it is captured                                                                             | [ISO 27001:2022].A.5.27                                               |

## Exceptions

Low-severity events are handled through routine work rather than the full process. The severity classification decides
which, and we apply it deliberately rather than letting whoever wants the least paperwork set it. There is no exception
to recording a significant incident, and none at all to recording a personal data breach. `EVIDENC` binds at every
severity. Deciding that a breach was not notifiable is itself a decision we have to be able to show.

[pol-DATA]: ../security/data-data-protection.md
[pol-RECV]: ../operations/recv-recoverability.md
[pol-SECD]: ../security/secd-security-by-design.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[UK GDPR]: ../../frameworks.md#uk-gdpr
