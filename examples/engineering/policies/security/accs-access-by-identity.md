---
id: pol-ACCS
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.3, A.5.15, A.5.16, A.5.18, A.8.2, A.8.3, A.8.5, A.8.18 ]
review-by: "2027-08-04"
owner: paul.law
tags: [ access-control, identity, least-privilege ]
---

# Access is by individual identity, on least privilege

`Policy: pol-ACCS` `DRAFT`

## Purpose

We grant access to a named person or a named workload, never to a shared login. Each of them gets no more than the
work needs.

If we cannot tell who did something, we cannot hold anyone to any rule below. A shared account converts an audit trail
into a guess. Standing privilege that nobody needs is a breach waiting for an attacker to find the credential.

## Scope

All systems, environments, source repositories, pipelines and data stores we build or operate, for people and for
machine identities alike. Applies to routine and privileged access.

_Boundary: [pol-AGNT] governs the authority of what an agent produces. A person who accepts agent work becomes its
author, not its approver, so `DUTIES` still requires a second person to release it._

## Clauses

| Id        | Clause                                                                                                                | Alignment                                       |
|-----------|-----------------------------------------------------------------------------------------------------------------------|-------------------------------------------------|
| `NAMED`   | **MUST** grant access to a named individual or a distinctly identified workload, never to a shared persona            | [ISO 27001:2022].A.5.16                         |
| `LEAST`   | **MUST** grant the least privilege that allows the work to be done, and no more                                       | [ISO 27001:2022].A.5.15, [ISO 27001:2022].A.8.3 |
| `DUTIES`  | **MUST** keep the ability to make a change separate from the ability to approve or release it. See [pol-AGNT]                       | [ISO 27001:2022].A.5.3                          |
| `AUTHN`   | **MUST** require strong authentication for access to our systems, our code and our data                               | [ISO 27001:2022].A.8.5, [OWASP ASVS 4.0].V2     |
| `RECERT`  | **MUST** review access rights periodically, and remove them promptly when a role changes or a person leaves           | [ISO 27001:2022].A.5.18                         |
| `ADMIN`   | **MUST** control and record the use of privileged administrative tooling                                              | [ISO 27001:2022].A.8.2, [ISO 27001:2022].A.8.18 |
| `SHARED`  | **MUST NOT** operate shared or generic privileged accounts where individual attribution is lost. See [pol-EVER]       | [ISO 27001:2022].A.8.2                          |
| `PERSIST` | **MUST NOT** leave standing production access in place beyond what the role genuinely requires                        | [ISO 27001:2022].A.8.2                          |
| `DIRECT`  | SHOULD hold identity in a single directory, so that access granted or removed once takes effect everywhere            | [ISO 27001:2022].A.5.16                         |
| `ZERO`    | COULD hold no standing privilege at all, granting privileged access on request and only for as long as the work takes | [ISO 27001:2022].A.8.2                          |

## Exceptions

Incident response sometimes needs a break-glass account: one kept for emergencies, outside the normal grants. That is
allowed where a person cannot otherwise act. The account is still attributable, its use raises an alert and is recorded,
and someone reviews that use afterwards. Any other departure requires a recorded deviation under [pol-DEVI].

[pol-AGNT]: ../governance/agnt-agents-propose-people-decide.md
[pol-DEVI]: ../governance/devi-deviations-are-recorded.md
[pol-EVER]: ../delivery/ever-everything-in-version-control.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[OWASP ASVS 4.0]: ../../frameworks.md#owasp-asvs
