---
id: pol-INCR
tier: normative
category: operations
status: draft
aligns-with:
  - ISO27001:2022 A.5.24
  - ISO27001:2022 A.5.25
  - ISO27001:2022 A.5.26
  - ISO27001:2022 A.5.27
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
the organisation.

## Clauses

| Id        | Clause                                                                                                                               | Alignment               |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `PROCESS` | **MUST** maintain a defined incident-response process with clear roles, so that during an incident it is never ambiguous who decides | [ISO 27001:2022].A.5.24 |
| `TRIAGE`  | **MUST** classify incidents by severity, and escalate and communicate according to it                                                | [ISO 27001:2022].A.5.25 |
| `RECORD`  | **MUST** preserve evidence and produce a record for every significant incident                                                       | [ISO 27001:2022].A.5.26 |
| `REPORT`  | **MUST** encourage anyone to report a suspected incident, and make it easy to do so                                                  | [ISO 27001:2022].A.6.8  |
| `REVIEW`  | **MUST** review significant incidents afterwards, looking for the conditions that allowed them rather than for someone to blame      | [ISO 27001:2022].A.5.27 |
| `ACTIONS` | **MUST** turn the findings of those reviews into tracked work                                                                        | [ISO 27001:2022].A.5.27 |
| `DRILL`   | **MUST** rehearse the process rather than first exercising it for real                                                               | [ISO 27001:2022].A.5.24 |
| `ADHOC`   | **MUST NOT** handle a significant incident informally, with no record and no named owner                                             | [ISO 27001:2022].A.5.26 |
| `CLOSE`   | **MUST NOT** close an incident at the point service is restored — it closes when the learning is captured                            | [ISO 27001:2022].A.5.27 |

## Exceptions

Low-severity events are handled through routine work rather than the full process; the severity classification is what
decides, and it is applied deliberately rather than by whoever wants the least paperwork. There is no exception to
recording a significant incident.

[ISO 27001:2022]: /frameworks.md#iso27001-2022
