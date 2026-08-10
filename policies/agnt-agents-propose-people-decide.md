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

| Id        | Clause                                                                                                                                                                       | Alignment                                         |
|-----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------|
| `PROV`    | **MUST** record where an agent-produced contribution came from, in enough detail that reviewing it is a check rather than an act of faith                                    | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].MAP    |
| `ACCEPT`  | **MUST** require a person to accept agent-produced work before it carries any authority, and that person owns it afterwards as if they had written it                        | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].GOVERN |
| `EQUAL`   | **MUST** hold agent-produced changes to every gate that applies to our own — the same review, the same automated verification under [pol-AUTV], the same route to production | [ISO 27001:2022].A.8.25, [NIST AI RMF 1.0].MANAGE |
| `CONFID`  | **MUST** state the confidence an observation actually has, and let unverified ones expire rather than settle into the corpus by age                                          | [NIST AI RMF 1.0].MEASURE                         |
| `SELFVER` | **MUST NOT** treat an agent's own account of its work as verification of that work                                                                                           | [NIST AI RMF 1.0].MEASURE                         |
| `UNPROV`  | **MUST NOT** accept a proposal we cannot trace back to what produced it — an unverifiable proposal is a rejected one                                                         | [ISO 27001:2022].A.8.30, [NIST AI RMF 1.0].MAP    |
| `ACCESS`  | **MUST NOT** grant an agent access, privilege or a route to production that an individual doing the same work would not be granted                                           | [NIST AI RMF 1.0].MANAGE                          |
| `EXCUSE`  | **MUST NOT** let "an agent wrote it" stand as either a reason to scrutinise it less or an excuse for what it broke                                                           | [NIST AI RMF 1.0].GOVERN                          |
| `REACH`   | **MUST NOT** hold an agent to a rule we never put within its reach — under [pol-KNOW] that is our failure, not the agent's                                                   | [NIST AI RMF 1.0].GOVERN                          |

## Exceptions

The acceptance gate has none. Agent-produced work does not become authoritative because it was convenient, and no
recorded deviation makes it so — a corpus where some agent output is authoritative and some is not is one where a reader
cannot tell which they are holding.

What acceptance *looks like* scales with what is being accepted, and that is proportion rather than exception: taking an
agent's change forward for review is acceptance; promoting an observation into a standard needs whoever owns that
standard. Acceptance is authorship rather than approval, so [pol-ACCS] still requires a second person to approve the
release. Nor is an incident an exception — a fix an agent proposed during one is accepted by the person who applied it,
and [pol-PIPE] governs reconciling it afterwards.

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
[ISO 27001:2022]: /frameworks.md#iso-27001
[NIST AI RMF 1.0]: /frameworks.md#nist-ai-rmf
