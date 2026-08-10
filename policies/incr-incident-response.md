---
id: pol-INCR
tier: normative
category: operations
status: draft
aligns-with:
  - ISO27001:2022 A.5.5
  - ISO27001:2022 A.5.24
  - ISO27001:2022 A.5.25
  - ISO27001:2022 A.5.26
  - ISO27001:2022 A.5.27
  - ISO27001:2022 A.5.28
  - ISO27001:2022 A.5.29
  - ISO27001:2022 A.6.8
review-by: "2027-08-04"
owner: paul.law
tags: [ incident-response, learning, postmortem ]
---

# Incidents are managed and learned from

`Policy: pol-INCR` `DRAFT`

## Purpose

When something goes wrong, there is a defined way to respond: someone is in charge, severity determines the response,
what happened is recorded, and the organisation is measurably better afterwards for having been through it.

The response is what turns an incident into either a recovery or a compounding failure, and the review afterwards is the
only mechanism that converts an expensive hour into something we never pay for twice. Both need to be decided before the
incident, because nobody designs a good process at three in the morning.

## Scope

Security and operational incidents affecting systems we build or operate, including those reported to us from outside
the organisation. A personal data breach is a security incident and is in scope here; what the data itself requires of
us is [pol-DATA]'s.

_Boundary: [pol-RECV] owns being able to recover — the objectives, the backups, and the proof that a restore works. This
policy owns deciding to recover and doing it, held to the objectives [pol-RECV] set._

## Clauses

| Id        | Clause                                                                                                                                             | Alignment                                                             |
|-----------|----------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------|
| `PROCESS` | **MUST** maintain a defined incident-response process with clear roles, so that during an incident it is never ambiguous who decides               | [ISO 27001:2022].A.5.24                                               |
| `TRIAGE`  | **MUST** classify incidents by severity, and escalate and communicate according to it                                                              | [ISO 27001:2022].A.5.25                                               |
| `RECOVER` | **MUST** invoke the recovery path defined for the affected system rather than improvising one                                                      | [ISO 27001:2022].A.5.29                                               |
| `RECORD`  | **MUST** preserve evidence and produce a record for every significant incident, and for every personal data breach whether or not it is notifiable | [ISO 27001:2022].A.5.26, [ISO 27001:2022].A.5.28, [UK GDPR].Art.33(5) |
| `NOTIFY`  | **MUST** notify the supervisory authority of a personal data breach within the statutory window                                                    | [ISO 27001:2022].A.5.5, [UK GDPR].Art.33                              |
| `INFORM`  | **MUST** tell the people a personal data breach puts at high risk, without undue delay                                                             | [UK GDPR].Art.34                                                      |
| `REPORT`  | **MUST** encourage anyone to report a suspected incident, and make it easy to do so                                                                | [ISO 27001:2022].A.6.8                                                |
| `REVIEW`  | **MUST** review significant incidents afterwards, looking for the conditions that allowed them rather than for someone to blame                    | [ISO 27001:2022].A.5.27                                               |
| `ACTIONS` | **MUST** turn the findings of those reviews into tracked work                                                                                      | [ISO 27001:2022].A.5.27                                               |
| `DRILL`   | **MUST** rehearse the process rather than first exercising it for real                                                                             | [ISO 27001:2022].A.5.24                                               |
| `ADHOC`   | **MUST NOT** handle a significant incident informally, with no record and no named owner                                                           | [ISO 27001:2022].A.5.26                                               |
| `TOOSOON` | **MUST NOT** close an incident at the point service is restored — it closes when the learning is captured                                          | [ISO 27001:2022].A.5.27                                               |

## Exceptions

Low-severity events are handled through routine work rather than the full process; the severity classification is what
decides, and it is applied deliberately rather than by whoever wants the least paperwork. There is no exception to
recording a significant incident, and none at all to recording a personal data breach — `RECORD` binds at every
severity, because the assessment of whether a breach is notifiable is itself a thing we have to be able to show.

[pol-DATA]: data-data-protection.md
[pol-RECV]: recv-recoverability.md
[ISO 27001:2022]: /frameworks.md#iso27001-2022
[UK GDPR]: /frameworks.md#uk-gdpr
