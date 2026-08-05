---
id: pol-KNOW
tier: normative
status: draft
aligns-with:
  - ISO27001:2022 A.5.37
implemented-by:
review-by: "2027-08-04"
owner: paul.law
tags: [ documentation, knowledge-management ]
---

# Policy: Knowledge is written down and kept with what it describes

## Purpose

The knowledge needed to build, run, decide about and recover a system is written down, versioned, and kept alongside
the thing it describes — not held in individuals' heads or in chat histories nobody can search.

Undocumented knowledge is a single point of failure that no amount of redundancy elsewhere compensates for. Keeping
documentation next to the code also means a change that invalidates it can be reviewed in the same breath as the change
itself, which is the only reliable way documentation stays true.

## Scope

All solutions we build or operate. Covers setup and build instructions, operational runbooks, architecturally
significant decisions, and the reference material a person or an agent needs to work on the system safely.

## Commitments

* We **will** document what is needed to build, run and recover each solution, and version that documentation.
* We **will** change documentation alongside the code, configuration or process it describes, in the same review.
* We **will** record architecturally significant decisions, with the reasoning and the alternatives weighed, so a future
  reader can tell a considered choice from an accident.
* We **will** write documentation to be usable by both people and the agents working in our codebases, treating them as
  readers of the same source of truth rather than maintaining two versions of it — including the rules we expect their
  work to follow ([pol-AGNT]).
* We **will not** allow knowledge that is critical to operating or recovering a system to exist only in someone's head
  or in an ephemeral conversation.
* We **will not** treat documentation as a task that follows delivery — undocumented work is unfinished work.

## Alignment

| Reference                 | Area                            |
|---------------------------|---------------------------------|
| ISO/IEC 27001:2022 A.5.37 | Documented operating procedures |

We **align with** this area. We are not registered against ISO/IEC 27001:2022 and are not audited against it.
Alignment exists because the framework covers the right ground.

## Exceptions

None. The effort is proportionate — a small internal tool needs less than a customer-facing platform — but the
commitment to write down what is needed does not vary.

## Implemented by

Intended implementing standards: documentation as code, and the decision-record conventions this wiki already applies to
[ADRs](/adrs).

_No implementing standard exists in this wiki yet; `implemented-by` stays empty until those standard ids do._

## Review

Reviewed annually by the owner named above. Last reviewed: not yet — drafted 2026-08-04.

[pol-AGNT]: agnt-agents-propose-people-decide.md
