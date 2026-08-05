---
id: pol-INCR
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.5.24
  - ISO27001:2022 A.5.25
  - ISO27001:2022 A.5.26
  - ISO27001:2022 A.5.27
  - ISO27001:2022 A.6.8
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags:
  - incident-response
  - postmortem
  - learning
---

# Policy: Incidents are managed and learned from

## Purpose

When something goes wrong, there is a defined way to respond: someone is in charge, severity determines the response,
what happened is recorded, and the organisation is measurably better afterwards for having been through it.

The response is what turns an incident into either a recovery or a compounding failure, and the review afterwards is
the only mechanism that converts an expensive hour into something we never pay for twice. Both need to be decided
before the incident, because nobody designs a good process at three in the morning.

## Scope

Security and operational incidents affecting systems we build or operate, including those reported to us from outside
the organisation.

## Commitments

* We **will** maintain a defined incident-response process with clear roles, so that during an incident it is never
  ambiguous who decides.
* We **will** classify incidents by severity, and escalate and communicate according to it.
* We **will** preserve evidence and produce a record for every significant incident.
* We **will** encourage anyone to report a suspected incident, and make it easy to do so.
* We **will** review significant incidents afterwards, looking for the conditions that allowed them rather than for
  someone to blame.
* We **will** turn the findings of those reviews into tracked work.
* We **will** rehearse the process rather than first exercising it for real.
* We **will not** handle a significant incident informally, with no record and no named owner.
* We **will not** close an incident at the point service is restored — it closes when the learning is captured.

## Alignment

| Reference                 | Area                                                         |
|---------------------------|--------------------------------------------------------------|
| ISO/IEC 27001:2022 A.5.24 | Incident management planning and preparation                 |
| ISO/IEC 27001:2022 A.5.25 | Assessment and decision on information security events       |
| ISO/IEC 27001:2022 A.5.26 | Response to information security incidents                   |
| ISO/IEC 27001:2022 A.5.27 | Learning from information security incidents                 |
| ISO/IEC 27001:2022 A.6.8  | Information security event reporting                         |

We **align with** these areas. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

Low-severity events are handled through routine work rather than the full process; the severity classification is what
decides, and it is applied deliberately rather than by whoever wants the least paperwork. There is no exception to
recording a significant incident.

## Implemented by

Intended implementing standard: incident response — with the operational detail carried in
[runbooks](/runbooks) and the learning in [postmortems](/postmortems).

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until that standard id does._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.
