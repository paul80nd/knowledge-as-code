---
id: adr-0001
type: adr
tier: decided
status: proposed
owner: human:paul.law
tags: [ documentation, knowledge-management, tooling ]
---

# Create a structured, validated wiki that AI sessions can read and contribute to (Knowledge as code)

`ADR: adr-0001` `PROPOSED`

> **In the context of** knowledge split across a wiki, a tracker and memory, **facing** AI sessions that cannot find
> the standards or record findings, **we decided** to treat knowledge as code: Markdown with validated frontmatter and
> generated indexes, **rather than** unstructured pages or a hosted platform, **to achieve** knowledge that accumulates
> and stays current, **accepting** schema obligations and CI gating.

## Context

This is the decision that brought the corpus into being, seeded so that the reasoning arrives with the mechanism. Read
it, rewrite the parts that are not true of your estate, and accept it in your own name. Until you do, it stands as
`proposed` and the corpus has no recorded reason to exist.

Engineering knowledge is usually split across three stores, and nothing connects them. A wiki keeps the architectural
reasoning and the rules. A work tracker keeps functional detail in epics, features and stories. Everything else is in
people's memory or in a chat message nobody can find: operational know-how, the fixes for recurring problems, and why
a particular tool was chosen.

The wiki content that exists is usually good. Its metadata is written in prose, so nothing can validate, cross-check or
index it. Contributors maintain the indexes by hand, so those indexes drift. Those documents also follow conventions
that nothing enforces: sequential IDs, bidirectional supersession, and standards citing the decisions they derive from.

An AI coding session that cannot find the relevant standards produces work that breaks them. It does so with
confidence. A session that discovers something worth knowing has nowhere to put it, whether that is a non-obvious
failure mode or a fix that took two hours to find. The discovery is lost when the session ends.

Three constraints shape the response.

* The wiki stays readable by people who will never open a terminal, so whatever replaces it keeps rendering as an
  ordinary wiki.
* Agent-written content cannot be trusted as far as reviewed content, so the corpus separates the two.
* The corpus grows from a few dozen documents towards several hundred, and anything relying on a person to maintain an
  index by hand fails at that size.

## Decision

We treat knowledge as code:

* **Every document has a type**, drawn from a [taxonomy](../knowledge-as-code/taxonomy.md), and is filed in a folder
  for that type.
* **Every document has YAML frontmatter** conforming to a [schema](../knowledge-as-code/metadata.md). A wiki that
  renders frontmatter as a metadata table replaces metadata in prose. A person reads it and automation uses it.
* **We group types into tiers by behaviour**: decided, normative, descriptive, procedural and observed. The tier
  decides the review bar, the validation rules and the lifecycle.
* **Observed knowledge is cheap to capture and deliberate to promote.** A session records an unverified discovery with
  no review. The discovery expires by default. Promotion to a fix or a standard needs a human.
* **CI validates** schema conformance, ID uniqueness, link resolution and bidirectional relationships. It generates
  the indexes, the reports and an always-loaded rules digest.
* **The mechanism is separable from the content.** The schema and the framework's own documentation contain no
  organisation specifics, arrive from upstream, and stand outside the taxonomy.

[Knowledge as Code](../knowledge-as-code.md) describes the full approach.

## Alternatives Considered

* **Keep the wiki as unstructured Markdown pages** (the status quo). Rejected: without a schema no tool can validate or
  generate anything, so every index stays hand-maintained and every convention stays unenforced. The status quo also
  gives an agent no reliable way to find or contribute knowledge. That gap is why this ADR exists.

* **Move to a hosted knowledge platform (Confluence, SharePoint, Notion).** Such a platform offers better discovery,
  better editing and broader reach across non-engineers. Rejected because an agent can read those platforms but cannot
  write to them. The Microsoft 365 connector exposes search and read tools only. Accumulating knowledge is the half of
  the problem nothing here solves today, so a platform that forbids writing cannot be the answer. The documentation
  also moves away from the code it describes, so nobody can review a page in the same pull request as the change that
  invalidated it.

* **A vector-indexed knowledge store with semantic search** solves retrieval at scale and finds related material
  without curated links. Rejected as premature: at a few dozen documents the index goes stale, cannot be diffed in
  review, and adds little to grep over a generated index. It's also risky here, because vector search returns *chunks*.
  An ADR's "Alternatives Considered" section contains confident descriptions of options we rejected, and a chunk
  retrieved out of context could read like a decision. Explicit `related` links, which we already write, are more
  precise, because they record *how* two documents relate. If a generated index stops being navigable, revisit this
  decision.

* **Adopt an off-the-shelf agent memory tool**: memcrate or something like it. It's a ready-made store with commands to
  save, load and pin. Rejected as a dependency: these are personal-scope tools with no multi-writer model, no review
  and no conflict handling. We adopt the ideas behind them, which are human-readable Markdown, and the explicit
  promotion of an insight. The dependency itself is not worth taking.

* **Keep knowledge in work items.** Functional detail already lives in the tracker, and the tracker has real search.
  Rejected: a work item is a delivery artefact whose lifecycle ends at "done". Durable knowledge outlives the work that
  produced it. A standard, a decision and a runbook have no natural home in a backlog. We link to the tracker and do
  not duplicate it.

## Consequences

* **Every document will acquire a schema obligation.** A contributor picks a type, allocates an ID and completes the
  required frontmatter. That adds friction, in exchange for validation and generation.
* **CI will become a gate on documentation.** A malformed document fails the build. Nothing blocks a documentation
  change today, so the first few failures could irritate.
* **Existing content will have to migrate.** The documents already written need frontmatter replacing their in-prose
  metadata.
* **The generator will own every index.** It marks each region it owns, and CI fails when a marked region is stale. A
  table can no longer disagree with its source unnoticed.
* **Agent-written content will enter the corpus**, at a lower authority tier and with an explicit promotion path. The
  value of the whole exercise depends on the promotion gate working. If the gate fails, the corpus fills with
  plausible, unverified assertions, and trust in everything else falls with them.
* **The corpus will grow from a few dozen documents towards several hundred** as the taxonomy fills. A generated index
  matters at that size, so we automate the indexes before the growth.
* **A rules digest will be generated into the repository root** and read by every AI session. It has a fixed size
  budget. Once that budget is reached, the rules are triaged rather than the budget raised.
* **Session logs will stay out of this corpus.** They routinely contain credentials and customer data. We commit only
  distilled, reviewed discoveries.
* **The mechanism will be reusable elsewhere.** Keeping the schema, the validators and the skills free of organisation
  specifics takes discipline, and makes the approach portable to another estate.

## References

* [Knowledge as Code](../knowledge-as-code.md) sets out the approach.
* [Taxonomy](../knowledge-as-code/taxonomy.md) lists the types and how to choose between them.
* [Metadata](../knowledge-as-code/metadata.md) defines the frontmatter schema.
* [Contributing](../knowledge-as-code/contributing.md) covers the review model and the promotion path.
* [Automation](https://paul80nd.github.io/knowledge-as-code/framework/automation/) says what CI validates and generates.
