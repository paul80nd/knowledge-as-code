---
id: pol-ACCS
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.3, A.5.15, A.5.16, A.5.18, A.5.19, A.8.2, A.8.3, A.8.4, A.8.5, A.8.18 ]
  - framework: UK GDPR
    clauses: [ Art.5(1)(f) ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ access-control, identity, least-privilege ]
---

# Access is by individual identity, on least privilege

`Policy: pol-ACCS` `DRAFT`

## Purpose

We grant access to a named person or a named workload, never to a shared login. Each identity gets no more access than
the work needs.

Where we cannot tell who did something, we cannot enforce any clause below against anyone. A shared account leaves an
audit trail nobody can attribute. Standing privilege nobody needs gives an attacker who finds the credential more access
than the work required.

## Scope

All systems, environments, source repositories, pipelines and data stores we build or operate, for people and for
machine identities alike. It applies to routine and to privileged access.

_Boundary: [pol-AGNT] governs the authority of what an agent produces. A person who accepts agent work becomes its
author and not its approver, so `DUTIES` still requires a second person to release it._

## Clauses

| Id        | Clause                                                                                                                                                    | Alignment                                                                                                           |
|-----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------|
| `NAMED`   | **MUST** grant access to a named individual or a distinctly identified workload, never to a shared persona                                                | [ISO 27001:2022].A.5.16                                                                                             |
| `LEAST`   | **MUST** grant the least privilege that allows the work to be done, and no more                                                                           | [ISO 27001:2022].A.5.15, [ISO 27001:2022].A.8.3, [ISO 27001:2022].A.8.4, [OWASP ASVS 4.0].V4, [UK GDPR].Art.5(1)(f) |
| `DUTIES`  | **MUST** keep the ability to make a change separate from the ability to approve or release it ([pol-AGNT].DUTIES states the agent case)                   | [ISO 27001:2022].A.5.3                                                                                              |
| `AUTHN`   | **MUST** require more than one factor wherever a person authenticates to our systems, our code or our data                                                | [ISO 27001:2022].A.8.5, [OWASP ASVS 4.0].V2                                                                         |
| `GRANT`   | **MUST** have an access grant authorised by someone accountable for what it gives access to, before it is made                                            | [ISO 27001:2022].A.5.18                                                                                             |
| `RECERT`  | **MUST** review every access grant at least annually, confirming it is still needed                                                                       | [ISO 27001:2022].A.5.15, [ISO 27001:2022].A.5.18                                                                    |
| `VALID`   | **MUST** confirm every active account still maps to a current person or workload at least monthly                                                         | [ISO 27001:2022].A.5.15                                                                                             |
| `REVOKE`  | **MUST** remove access promptly when a role changes or a person leaves                                                                                    | [ISO 27001:2022].A.5.18                                                                                             |
| `TERM`    | **MUST** remove a third party's access when the contract, integration or dependency relationship it was granted for ends                                  | [ISO 27001:2022].A.5.19                                                                                             |
| `DORMANT` | **MUST** automatically disable an account after a defined period of inactivity, and require review before it is reactivated                               | [ISO 27001:2022].A.5.15                                                                                             |
| `EXPIRE`  | **MUST** remove temporary access by the time period agreed when it was granted                                                                            | [ISO 27001:2022].A.5.15                                                                                             |
| `ADMIN`   | **MUST** control and record the use of privileged administrative tooling, through an identity kept separate from the person's routine, day-to-day account | [ISO 27001:2022].A.8.2                                                                                              |
| `UTILS`   | **MUST** limit tooling that can override a system's own controls to people whose work requires it                                                         | [ISO 27001:2022].A.8.18                                                                                             |
| `SHARED`  | **MUST NOT** operate shared or generic privileged accounts where individual attribution is lost ([pol-EVER].SHARED states the same for change authorship) | [ISO 27001:2022].A.8.2                                                                                              |
| `PERSIST` | **MUST NOT** leave standing production access in place beyond what the role genuinely requires                                                            | [ISO 27001:2022].A.8.2                                                                                              |
| `UNIQUE`  | **MUST NOT** reissue a retired individual or workload identifier to a new person or workload                                                              | [ISO 27001:2022].A.5.16                                                                                             |
| `DIRECT`  | SHOULD keep identity in a single directory, so that access granted or removed once takes effect everywhere                                                | [ISO 27001:2022].A.5.16                                                                                             |
| `SCALED`  | SHOULD require greater scrutiny for a grant reaching more sensitive data, in proportion to its classification                                             | [ISO 27001:2022].A.5.18                                                                                             |
| `ZERO`    | COULD keep no standing privilege at all, granting privileged access on request and only for as long as the work takes                                     | [ISO 27001:2022].A.8.2                                                                                              |

## Exceptions

Incident response sometimes needs a break-glass account: one kept for emergencies, outside the normal grants. That is
allowed where a person cannot otherwise act. The account is still attributable. Its use raises an alert and is
recorded, and someone reviews that use afterwards. Any other departure requires a recorded deviation under [pol-DEVI].

[pol-AGNT]: ../governance/agnt-agent-oversight.md#clauses
[pol-DEVI]: ../governance/devi-deviations.md
[pol-EVER]: ../delivery/ever-version-control.md#clauses
[ISO 27001:2022]: ../../frameworks.md#iso-27001
[UK GDPR]: ../../frameworks.md#uk-gdpr
[OWASP ASVS 4.0]: ../../frameworks.md#owasp-asvs
