---
id: pol-KNOW
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.5.37
review-by: "2027-08-04"
owner: paul.law
tags: [ documentation, knowledge-management ]
---

# Knowledge is written down and kept with what it describes

`Policy: pol-KNOW` `DRAFT`

## Purpose

The knowledge needed to build, run, decide about and recover a system is written down, versioned, and kept alongside the
thing it describes — not held in individuals' heads or in chat histories nobody can search. Undocumented work is
unfinished work.

Undocumented knowledge is a single point of failure that no amount of redundancy elsewhere compensates for. Keeping
documentation next to the code also means a change that invalidates it can be reviewed in the same breath as the change
itself, which is the only reliable way documentation stays true.

## Scope

All solutions we build or operate. Covers setup and build instructions, operational runbooks, architecturally
significant decisions, and the reference material a person or an agent needs to work on the system safely.

## Clauses

| Id       | Clause                                                                                                                                                                                                                                                              | Alignment               |
|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------|
| `DOCS`   | **MUST** document what is needed to build, run and recover each solution, and version that documentation                                                                                                                                                            | [ISO 27001:2022].A.5.37 |
| `SYNC`   | **MUST** change documentation alongside the code, configuration or process it describes, in the same review                                                                                                                                                         | [ISO 27001:2022].A.5.37 |
| `DECIDE` | **MUST** record architecturally significant decisions, with the reasoning and the alternatives weighed, so a future reader can tell a considered choice from an accident                                                                                            |                         |
| `AGENTS` | **MUST** write documentation to be usable by both people and the agents working in our codebases, treating them as readers of the same source of truth rather than maintaining two versions of it — including the rules we expect their work to follow ([pol-AGNT]) |                         |
| `HEADS`  | **MUST NOT** allow knowledge that is critical to operating or recovering a system to exist only in someone's head or in an ephemeral conversation                                                                                                                   | [ISO 27001:2022].A.5.37 |

## Exceptions

None. The effort is proportionate — a small internal tool needs less than a customer-facing platform — but the
commitment to write down what is needed does not vary.

[pol-AGNT]: agnt-agents-propose-people-decide.md
[ISO 27001:2022]: /frameworks.md#iso-27001
