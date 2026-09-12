---
id: pol-KNOW
type: policy
tier: normative
status: draft
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.37 ]
review-by: "2027-08-04"
owner: human:paul.law
tags: [ documentation, knowledge-management ]
---

# Knowledge is written down and kept with what it describes

`Policy: pol-KNOW` `DRAFT`

## Purpose

The knowledge needed to build, run, decide about and recover a system is written down, versioned, and kept next to the
thing it describes. It does not sit in someone's head, or in a chat history nobody can search. Agents read the
same documentation people do.

If one person is the only one who knows how a system recovers, the system is one resignation away from being
unrecoverable. Redundant infrastructure does not help with that. Keeping the documentation next to the code does help,
because a change that makes the documentation wrong is then reviewed alongside the change itself. Undocumented work is
unfinished work.

## Scope

All solutions we build or operate. That covers what a feature is meant to do, setup and build instructions, operational
runbooks, and architecturally significant decisions. It also covers the reference material a person or an agent needs to
work on the system safely.

## Clauses

| Id        | Clause                                                                                                                                                                   | Alignment               |
|-----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `DOCS`    | **MUST** document what is needed to build, run and recover each solution, and version that documentation                                                                 | [ISO 27001:2022].A.5.37 |
| `SYNC`    | **MUST** change documentation alongside the code, configuration or process it describes, in the same review                                                              | [ISO 27001:2022].A.5.37 |
| `DECIDE`  | **MUST** record architecturally significant decisions, with the reasoning and the alternatives weighed, so a future reader can tell a considered choice from an accident |                         |
| `AGENTS`  | **MUST** document the rules we expect agent-produced work to follow, where the agents doing that work will read them ([pol-AGNT])                                        |                         |
| `HEADS`   | **MUST NOT** allow knowledge that is critical to operating or recovering a system to exist only in someone's head or in an ephemeral conversation                        | [ISO 27001:2022].A.5.37 |
| `COPY`    | **MUST NOT** maintain a separate copy of documentation for agents to read                                                                                                |                         |
| `BEHAVE`  | SHOULD state a new feature's expected behaviour as scenarios the tests can run, before it is built                                                                       |                         |
| `RUNBOOK` | SHOULD write the steps for each operational scenario we can foresee, before the day we need them                                                                         |                         |

## Exceptions

None. The effort is proportionate: a small internal tool needs less than a customer-facing platform. The commitment to
write down what is needed does not vary.

[pol-AGNT]: ../governance/agnt-agents-propose-people-decide.md
[ISO 27001:2022]: ../../frameworks.md#iso-27001
