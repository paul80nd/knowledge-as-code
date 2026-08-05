# Knowledge as Code

This wiki is not a document store. It is a structured, validated, machine-readable body of knowledge that happens to
render as a wiki: maintained in git, reviewed through pull requests, and readable *and writable* by AI coding sessions
as well as people.

This page explains the approach. The detail lives in four places:

* [Taxonomy](knowledge-as-code/taxonomy.md) — the kinds of knowledge we hold, and how to choose between them.
* [Metadata](knowledge-as-code/metadata.md) — the frontmatter schema every document carries.
* [Contributing](knowledge-as-code/contributing.md) — how knowledge is added, reviewed and promoted.
* [Automation](knowledge-as-code/automation.md) — what CI validates and generates.

The decision to work this way is recorded in [adr-0001](/adrs/0001-knowledge-as-code.md).

## The problem

Engineering knowledge is spread thin. Architectural reasoning lives here; functional detail lives in ADO epics and
features; operational know-how lives in people's heads or in a Teams message from eight months ago; the answer to "why
does the build fail when I do X" is rediscovered roughly every quarter by whoever hits it next.

AI coding sessions make this sharper in both directions. A session that doesn't know our standards will confidently
produce work that violates them. A session that *discovers* something useful has nowhere to put it, so the discovery
dies when the session ends. Knowledge neither arrives where it's needed nor accumulates from where it's found.

## The commitment

Knowledge is treated as code:

* **Plain Markdown in git.** Diffable, reviewable, versioned, blamable. No proprietary format, no export step.
* **Structured metadata.** Every document carries frontmatter, so it can be validated, indexed and cross-referenced by
  machine rather than by memory.
* **Reviewed through pull requests**, at a rigour proportionate to what the document is (see [tiers](#tiers) below).
* **Published as this wiki**, so it stays readable by anyone without a terminal or IDE.
* **Readable and writable by agents.** Sessions consult it before working and contribute back what they learn.

The last point is the one that changes the character of the thing. A wiki that only humans write grows at the rate
humans remember to write. A wiki that sessions can also write to grows at the rate work happens — which is why the
review model below matters more than it otherwise would.

## The normative hierarchy

Four layers, each citing the one above it. This is the conventional policy hierarchy and we use it deliberately rather
than inventing our own vocabulary.

| Layer                                        | Answers                         | Changes         | Example                                                                |
|----------------------------------------------|---------------------------------|-----------------|------------------------------------------------------------------------|
| [Policy](/policies)                          | What do we commit to, and why?  | Rarely          | "Secrets are never stored in source control."                          |
| [Standard](/standards)                       | What must I do as a result?     | With practice   | "Services **MUST** read secrets from Key Vault via workload identity." |
| [Control](/controls)                         | How do we know it's being done? | With tooling    | "CI runs secret scanning on every PR; failures block merge."           |
| [Process](/processes) / [Runbook](/runbooks) | How do I actually do it?        | With the estate | "Rotating a Key Vault secret."                                         |

[ADRs](/adrs) sit alongside rather than inside this hierarchy. An ADR records a *decision* and is immutable; a standard
records the *resulting practice* and is maintained in place. The ADR owns the "why", the standard owns the "what", and
the standard cites the ADR rather than restating it.

Policies are aligned with ISO/IEC 27001:2022 where relevant, and say which Annex A areas they correspond to. This is
**alignment, not certification** — aligning with Annex A does not imply the organisation is ISO 27001 registered, and
the alignment exists because the framework covers the right ground, not because anyone is auditing against it.

## Tiers

The single most useful idea here: **what a document is about** (its type) and **how it behaves** (its tier) are
different things, and it is behaviour that determines the rules.

| Tier            | Behaviour                                                   | Review                               |
|-----------------|-------------------------------------------------------------|--------------------------------------|
| **Decided**     | Immutable once accepted. Superseded, never rewritten.       | PR, two reviewers                    |
| **Normative**   | Living. Owned. Edited in place with a changelog.            | PR, owner approves                   |
| **Descriptive** | Living. Must mirror reality. Verifiable against the estate. | PR, but drift detection catches more |
| **Procedural**  | Living. Must be rehearsed to stay true.                     | PR + evidence of last rehearsal      |
| **Observed**    | Perishable. Unreviewed until promoted. Expires by default.  | None until promotion                 |

A new kind of knowledge doesn't need new machinery — it needs a tier. Every validation rule, review expectation and
generated report keys off the tier rather than the type.

**Lifecycle** (`immutable` / `living` / `perishable`) follows from tier and is not stated separately. Two fields that
can contradict each other is a defect waiting to happen.

### Why Observed exists

The most important tier is the one that carries the least authority. Capture has to be nearly free or it doesn't
happen — nobody writes up a gotcha if doing so requires a template, an owner and two reviewers. So observations are
recorded with no review at all, marked as unverified, and expire on their own if nothing promotes them.

Promotion is where the rigour lives. A [discovery](/discoveries) becomes an [FAQ](/faqs) only when a human confirms it,
and the FAQ carries provenance back to the observation it came from. This gradient — cheap capture, deliberate
promotion — is what lets the corpus grow without the average trustworthiness falling.

## What this is not

* **Not a replacement for ADO.** Work items own delivery. This wiki owns durable knowledge. Where they overlap, the wiki
  links to ADO rather than copying it.
* **Not a document dump.** Every document has a type, a tier, an owner and a reason to exist. Content that fits no type
  is a prompt to discuss the taxonomy, not to create an `misc/` folder.
* **Not certified compliance.** See the note on ISO alignment above.
* **Not automatically true.** Documents carry a status and, in the Observed tier, a confidence level. Read them.

## Getting oriented

| If you want to…                              | Go to                                             |
|----------------------------------------------|---------------------------------------------------|
| Understand what kinds of knowledge live here | [Taxonomy](knowledge-as-code/taxonomy.md)         |
| Add or change something                      | [Contributing](knowledge-as-code/contributing.md) |
| Know what metadata a document needs          | [Metadata](knowledge-as-code/metadata.md)         |
| Understand what CI checks and builds         | [Automation](knowledge-as-code/automation.md)     |
| Know why we work this way                    | [adr-0001](/adrs/0001-knowledge-as-code.md)       |

---

**A note on scope.** The documents under `knowledge-as-code/` describe the system; they are not themselves part of the
taxonomy and carry no taxonomy frontmatter. The constitution is not one of the laws. This also keeps the mechanism —
schema, validators, generators, skills — cleanly separable from the corpus's content.
