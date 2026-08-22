---
id: pol-AGNT
tier: normative
category: governance
status: draft
aligns-with:
  - ISO27001:2022 A.8.25
  - ISO27001:2022 A.8.30
review-by: "2027-08-05"
owner: paul.law
tags: [ ai-agents, knowledge-management, provenance ]
---

# Agents propose, people decide

`Policy: pol-AGNT` `DRAFT`

## Purpose

An agent can write code, change configuration, draft documentation, or report something it noticed about a system. None
of it carries any authority until a person reads it and accepts it. Not just any person: someone whose job it is to say
yes to work of that kind.

An agent is fast, and it sounds just as confident when it is wrong. A guess and a checked answer look the same on the
page. A reader who cannot tell them apart soon trusts neither. So we keep the speed and add two things: a record of what
produced the work, and a person who puts their name on it. Where the agent was only guessing, we say so.

## Scope

Anything an agent contributes to the systems we run or the documentation we keep: code, configuration, prose, or a
recorded observation. This applies whether a person told the agent what to do or it ran on a schedule with nobody
watching.

_Boundary: [pol-ACCS] governs who the agent logs in as, and [pol-SCRT] the credentials it holds. This policy is about
the authority of what it produces._

## Clauses

| Id        | Clause                                                                                                                                                                       | Alignment                                         |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------|
| `PROV`    | **MUST** record where an agent-produced contribution came from, in enough detail that reviewing it is a check rather than an act of faith                                    | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].MAP    |
| `ACCEPT`  | **MUST** require a person to accept agent-produced work before it carries any authority, and that person owns it afterwards as if they had written it                        | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].GOVERN |
| `EQUAL`   | **MUST** hold agent-produced changes to every gate that applies to our own: the same review, the same automated verification under [pol-AUTV], the same route to production | [ISO 27001:2022].A.8.25, [NIST AI RMF 1.0].MANAGE |
| `CONFID`  | **MUST** state how confident we are in an observation, and let unverified ones expire rather than stay on unchallenged                                                       | [NIST AI RMF 1.0].MEASURE                         |
| `SELFVER` | **MUST NOT** treat an agent's own account of its work as verification of that work                                                                                           | [NIST AI RMF 1.0].MEASURE                         |
| `UNPROV`  | **MUST NOT** accept a proposal we cannot trace back to what produced it                                                                                                      | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].MAP    |
| `ACCESS`  | **MUST NOT** grant an agent access, privilege or a route to production that an individual doing the same work would not be granted                                           | [NIST AI RMF 1.0].MANAGE                          |

## Exceptions

There is no exception to a person accepting the work. Being in a hurry does not give agent output authority, and neither
does a recorded deviation. If some agent output had authority and some did not, nobody could tell which they were
reading.

How much accepting takes depends on what is being accepted. Putting an agent's change up for review is enough. Turning
one of its observations into a standard takes the person who owns that standard. Neither is an exception. They are the
same rule at different sizes.

A person who accepts agent work becomes its author, not its approver, so [pol-ACCS] still requires a second person to
approve the release. An incident is not an exception either. If an agent proposes a fix during one, the person who
applies it has accepted it. [pol-PIPE] governs putting that fix back into version control afterwards.

## Notes

This corpus already works the way this policy describes. When someone records an observation here, it goes in as a
[discovery](/discoveries). A discovery names its source, says how confident we are, and expires unless someone confirms
it. It cannot confirm itself. It becomes an [FAQ](/faqs) or a [standard](/standards) only when a person promotes it.
[adr-0001] records why, and says what we lose if that promotion step is skipped.

[adr-0001]: /adrs/0001-knowledge-as-code.md
[pol-ACCS]: accs-access-by-identity.md
[pol-AUTV]: autv-automated-verification.md
[pol-PIPE]: pipe-pipeline-to-production.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
[ISO 27001:2022]: /frameworks.md#iso-27001
[NIST AI RMF 1.0]: /frameworks.md#nist-ai-rmf
