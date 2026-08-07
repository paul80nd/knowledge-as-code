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

Work produced by an AI agent — code, configuration, documentation, or an observation about how something behaves —
enters our estate as a proposal. It carries a reference back to whatever produced it, it claims no more confidence than
it has earned, and it becomes authoritative only when a person with the standing to accept it does so.

Agents are productive and plausible in roughly equal measure, and the plausibility is the problem: an unverified
assertion written in the same voice as a reviewed one degrades everything around it. Provenance and a human acceptance
gate are what let us take the productivity without spending the trust that makes the rest of this corpus worth reading.

## Scope

Any contribution to the systems or the knowledge we own that was produced by an AI agent, whether it arrives as code,
configuration, documentation or a recorded observation. Applies to agents working under a person's direction and to
autonomous or scheduled ones alike.

_Boundary: the identity an agent authenticates as is governed by [pol-ACCS] and the credentials it holds by [pol-SCRT].
This policy is about the authority of what it produces._

## Clauses

| Id        | Clause                                                                                                                                                                       |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `SOURCE`  | **MUST** record where an agent-produced contribution came from, in enough detail that reviewing it is a check rather than an act of faith                                    |
| `ACCEPT`  | **MUST** require a person to accept agent-produced work before it carries any authority, and that person owns it afterwards as if they had written it                        |
| `GATES`   | **MUST** hold agent-produced changes to every gate that applies to our own — the same review, the same automated verification under [pol-AUTV], the same route to production |
| `CONFID`  | **MUST** state the confidence an observation actually has, and let unverified ones expire rather than settle into the corpus by age                                          |
| `SELFVER` | **MUST NOT** treat an agent's own account of its work as verification of that work                                                                                           |
| `TRACE`   | **MUST NOT** accept a proposal we cannot trace back to what produced it — an unverifiable proposal is a rejected one                                                         |
| `ACCESS`  | **MUST NOT** grant an agent access, privilege or a route to production that an individual doing the same work would not be granted                                           |
| `EXCUSE`  | **MUST NOT** let "an agent wrote it" stand as either a reason to scrutinise it less or an excuse for what it broke                                                           |
| `REACH`   | **MUST NOT** hold an agent to a rule we never put within its reach — under [pol-KNOW] that is our failure, not the agent's                                                   |

## Alignment

| Reference                 | Area                         |
|---------------------------|------------------------------|
| ISO/IEC 27001:2022 A.8.25 | Secure development lifecycle |
| ISO/IEC 27001:2022 A.8.30 | Outsourced development       |

We **align with** these areas. The second is an analogy and worth naming as one: ISO/IEC 27001:2022 has no concept of a
non-human contributor, but the obligation to direct, monitor and review development carried out beyond the team is the
same obligation, and it is the closest the framework comes. We are not registered against ISO/IEC 27001:2022 and are not
audited against it.

## Exceptions

The acceptance gate has none. Agent-produced work does not become authoritative because it was convenient, and no
recorded deviation makes it so — a corpus where some agent output is authoritative and some is not is one where a reader
cannot tell which they are holding.

What acceptance *looks like* scales with what is being accepted, and that is proportion rather than exception: a
reviewer merging a change is acceptance; promoting an observation into a standard needs whoever owns that standard. Nor
is an incident an exception — a fix an agent proposed during one is accepted by the person who applied it, and
[pol-PIPE] governs reconciling it afterwards.

## Notes

The mechanism this policy describes already exists in this wiki's own taxonomy: [discoveries](/discoveries) carry their
source, confidence and provenance, expire by default, cannot confirm themselves, and reach [FAQs](/faqs) or
[standards](/standards) only by promotion. [adr-0001] records why, and states the consequence plainly — the value of the
exercise depends on the promotion gate holding.

[adr-0001]: /adrs/0001-knowledge-as-code.md
[pol-ACCS]: accs-access-by-identity.md
[pol-AUTV]: autv-automated-verification.md
[pol-KNOW]: know-knowledge-is-written-down.md
[pol-PIPE]: pipe-pipeline-to-production.md
[pol-SCRT]: scrt-secrets-are-never-embedded.md
