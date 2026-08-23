---
id: adr-0001
tier: decided
status: proposed
owner: {{owner}}
tags: [ documentation, knowledge-management, tooling ]
---

# Create a structured, validated wiki that AI sessions can read and contribute to (Knowledge as code)

`ADR: adr-0001` `PROPOSED`

> **In the context of** knowledge split across wikis, trackers and heads, **facing** AI sessions that cannot find our
> standards or record findings, **we decided** to treat knowledge as code: Markdown with validated frontmatter and
> generated indexes, **rather than** unstructured pages or a hosted platform, **to achieve** knowledge that compounds
> rather than decays, **accepting** schema obligations and CI gating.

## Context

This is the decision that brought the corpus into being, seeded so that the reasoning arrives with the mechanism. Read
it, rewrite the parts that are not true of your estate, and accept it in your own name. Until you do, it stands as
`proposed` and the corpus has no recorded reason to exist.

Engineering knowledge is typically spread across places that do not talk to each other. A wiki holds architectural
reasoning and the rulebook. A work tracker holds functional detail in epics, features and stories. Everything else
(operational know-how, the fixes for recurring problems, why a particular tool was chosen) lives in people's memory or
in a chat message nobody can find.

The wiki content that does exist is usually good and structurally inert. Metadata sits in handwritten Markdown tables,
so nothing can be validated, cross-checked or indexed by machine. Indexes are maintained by hand and drift. The
conventions those documents follow (sequential IDs, bidirectional supersession, standards citing the decisions they
derive from) are real rules that nothing enforces.

AI coding sessions sharpen the problem at both ends. A session that cannot efficiently find the relevant standard will
confidently produce work that violates it. In the other direction, a session that discovers something worth knowing (a
non-obvious failure mode, a fix that took two hours to find) has nowhere to put it, so the discovery dies when the
session ends. Knowledge neither reaches the point of use nor accumulates from the point of discovery.

Three constraints shape the response. The wiki must stay readable by people who will never open a terminal, so whatever
we do has to keep rendering as an ordinary wiki. Agent-written content cannot be trusted at the same level as reviewed
content, so a single undifferentiated corpus is not acceptable. And the corpus will grow from a few dozen documents
towards several hundred, so anything relying on a human maintaining an index by hand will fail.

## Decision

We treat knowledge as code. Concretely:

* **Every document has a type**, drawn from an explicit [taxonomy](../knowledge-as-code/taxonomy.md), and lives in the
  folder for that type.
* **Every document carries YAML frontmatter** conforming to a [schema](../knowledge-as-code/metadata.md). A wiki that
  renders frontmatter as a metadata table makes this visible to human readers rather than hidden plumbing, and the
  handwritten metadata tables documents currently carry are replaced by it.
* **Types are grouped into tiers by behaviour**: decided, normative, descriptive, procedural and observed. The tier,
  not the type, determines the review bar, the validation rules and the lifecycle.
* **A cheap-capture, deliberate-promotion path exists** for observed knowledge. Unverified discoveries are recorded with
  no review and expire by default; promotion to an FAQ or a standard requires a human.
* **CI validates** schema conformance, ID uniqueness, link resolution and bidirectional relationships, and **generates**
  indexes, reports and an always-loaded rules digest.
* **The mechanism is separable from the content.** The schema and the framework's own documentation carry no
  organisation specifics, arrive from upstream, and are not themselves governed by the taxonomy.

The full approach is described in [Knowledge as Code](../knowledge-as-code.md).

## Alternatives Considered

* **Keep the wiki as unstructured Markdown pages** (the status quo). Rejected: no schema means nothing can be validated
  or generated, so every index stays hand-maintained and every convention stays unenforced. It also gives agents no
  reliable way to find or contribute knowledge, which is the problem prompting this ADR.

* **Move to a hosted knowledge platform (Confluence, SharePoint, Notion).** Such a platform offers better discovery,
  better editing and broader reach across non-engineers. Rejected: agents can read such platforms at best and generally
  cannot write to them. The Microsoft 365 connector exposes search and read tools only, with no write path. Since the
  accumulation half of the problem is the half we cannot currently solve at all, a platform that structurally forbids
  it cannot be the answer. Documentation also stops travelling with the code it describes, so it can no longer be
  reviewed in the same pull request as the change that invalidated it.

* **A vector-indexed knowledge store with semantic search** solves retrieval at scale and finds related material
  without curated links. Rejected as premature and, for this corpus, actively risky. At a few dozen documents an index
  adds a build artefact that goes stale, cannot be diffed in review, and buys little over grep plus a generated index.
  More seriously, vector search returns *chunks*: an ADR's "Alternatives Considered" section consists of
  confidently-worded descriptions of options we **rejected**, and a chunk retrieved out of context reads exactly like a
  decision. For a corpus whose purpose is recording what we chose and what we didn't, that failure mode is unacceptable.
  Explicit `related` links, which we already write, are more precise because they record *how* two documents relate.
  Revisit if the corpus grows past the point where a generated index is navigable.

* **Adopt an off-the-shelf agent memory tool.** It is a ready-made vault with verbs to save, load and pin. Rejected as
  a dependency: these are personal-scope tools with no multi-writer model, no review and no conflict handling. The
  underlying ideas (human-readable Markdown, explicit promotion of an insight into durable knowledge) are good, and we
  adopt them. The dependency is not worth taking for what amounts to a directory convention and three skills.

* **Keep knowledge in work items.** The tracker is already where functional detail lives, and it has real search.
  Rejected: work items are delivery artefacts with a lifecycle that ends at "done". Durable knowledge outlives the work
  that produced it, and standards, decisions and runbooks have no natural home in a backlog. We link to the tracker
  rather than duplicating it.

## Consequences

* **Every document acquires a schema obligation.** Contributors must pick a type, allocate an ID and complete required
  frontmatter. This is friction, deliberately placed where it buys validation and generation.
* **CI becomes a gate on documentation.** A malformed document fails a build. Nothing blocks a documentation change
  today, and the first few failures will irritate.
* **Existing content must migrate.** Documents already written need frontmatter, handwritten metadata tables are
  replaced by it, and hand-maintained indexes become generated.
* **Hand-maintained indexes stop being hand-maintained.** Generated regions are marked and CI fails if they are stale,
  so the tables can no longer silently disagree with their source.
* **Agent-written content enters the corpus**, at a lower authority tier, with an explicit promotion path. The value of
  the whole exercise depends on the promotion gate holding; if it doesn't, the corpus fills with plausible, unverified
  assertions and trust in everything else falls with it.
* **The corpus will grow from a few dozen documents towards several hundred** as the taxonomy fills. Generated indexes
  become load-bearing rather than convenient, and must be built before the growth rather than after.
* **A rules digest is generated into the repository root** and consumed automatically by AI sessions. It carries a hard
  size budget; when the budget binds, rules must be triaged rather than the budget raised.
* **Session logs are explicitly out of scope for this repository.** They routinely contain credentials and customer
  data; only distilled, reviewed discoveries are committed.
* **The mechanism is reusable elsewhere.** Keeping the schema and the framework's documentation free of organisation
  specifics costs a little discipline and makes the approach portable to another estate.

## References

* [Knowledge as Code](../knowledge-as-code.md) sets out the approach.
* [Taxonomy](../knowledge-as-code/taxonomy.md) lists the types and how to choose between them.
* [Metadata](../knowledge-as-code/metadata.md) defines the frontmatter schema.
* [Contributing](../knowledge-as-code/contributing.md) covers the review model and the promotion path.
* [Automation](../knowledge-as-code/automation.md) says what CI validates and generates.
