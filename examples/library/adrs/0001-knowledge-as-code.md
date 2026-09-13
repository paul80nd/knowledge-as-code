---
id: adr-0001
type: adr
tier: decided
status: proposed
owner: human:paul.law
tags: [ documentation, knowledge-management, tooling ]
---

# Knowledge as code: a structured, validated wiki that AI sessions can read and contribute to

`ADR: adr-0001` `PROPOSED`

> **In the context of** knowledge split across the wiki, ADO and people's memory, **facing** AI sessions that cannot
> find our standards or record findings, **we decided** to treat knowledge as code: Markdown with validated
> frontmatter and generated indexes, **rather than** unstructured pages or a hosted platform, **to achieve** knowledge
> that accumulates and stays current, **accepting** schema obligations and CI gating.

## Context

Our engineering knowledge sits in three places, and nothing connects them. The wiki keeps the architectural reasoning
in ADRs and the rules in standards. Azure DevOps (ADO) keeps functional detail in epics, features and stories.
Everything else is in people's memory or in a chat message nobody can find: operational know-how, the fixes for
recurring problems, and why a particular tool was chosen.

The wiki content that exists is good. Its metadata is written in prose, so nothing can validate, cross-check or index
it. Contributors maintain the indexes by hand, so those indexes drift. Those documents also follow conventions that
nothing enforces: sequential IDs, and standards citing the ADRs they derive from.

An AI coding session that cannot find the relevant standards produces work that breaks them. It does so with
confidence. A session that discovers something worth knowing has nowhere to put it, whether that is a non-obvious
failure mode or a fix that took two hours to find. The discovery is lost when the session ends.

Three constraints shape our response.

* The wiki stays readable by people who will never open a terminal, so whatever we build keeps rendering as an ADO
  wiki.
* Agent-written content cannot be trusted as far as reviewed content, so the corpus separates the two.
* The corpus grows from a few dozen documents towards several hundred, and anything relying on a person to maintain an
  index by hand fails at that size.

## Decision

We treat knowledge as code:

* **Every document has a type**, drawn from a [taxonomy](../knowledge-as-code/taxonomy.md), and is filed in a folder
  for that type.
* **Every document has YAML frontmatter** conforming to a [schema](../knowledge-as-code/metadata.md). ADO renders that
  frontmatter as a metadata table, which replaces metadata in prose. A person reads it and automation uses it.
* **We group types into tiers by behaviour**: decided, normative, descriptive, procedural and observed. The tier
  decides the review bar, the validation rules and the lifecycle.
* **Observed knowledge is cheap to capture and deliberate to promote.** A session records an unverified discovery with
  no review. The discovery expires by default. Promotion to a fix or a standard needs a human.
* **CI validates** schema conformance, ID uniqueness, link resolution and bidirectional relationships. It generates
  the indexes, the reports and an always-loaded rules digest.
* **The mechanism is separable from the content.** Managed packages provide the tooling. A template set provides the
  schema and the skills, and a corpus adopts it in whole or in part.

[Knowledge as Code](../knowledge-as-code.md) describes the full approach.

## Alternatives Considered

* **Keep the wiki as unstructured Markdown pages** (the status quo). Rejected: without a schema no tool can validate or
  generate anything, so every index stays hand-maintained and every convention stays unenforced. The status quo also
  gives an agent no reliable way to find or contribute knowledge. That gap is why this ADR exists.

* **Move to a hosted knowledge platform**: Confluence, SharePoint, Notion or similar. Such platforms offer better
  discovery, better editing and broader reach across non-engineers. Rejected because an agent can read those platforms
  but cannot write to them. The Microsoft 365 connector exposes search and read tools only. The documentation also
  moves away from the code it describes, so nobody can review a page in the same pull request as the change that
  invalidated it.

* **A vector-indexed knowledge store with semantic search** solves retrieval at scale and finds related material without
  curated links. Rejected as premature: at a few dozen documents the index goes stale and adds little to grep over a
  generated index. It's also risky here, because vector search returns *chunks*. An ADR's "Alternatives Considered"
  section contains confident descriptions of options we rejected, and a chunk retrieved out of context could read like a
  decision. Explicit `related` links, which we already write, are more precise, because they record *how* two documents
  relate. If a generated index stops being navigable, revisit this decision.

* **Adopt an off-the-shelf agent memory tool**: memcrate or something like it. It's a ready-made store with commands to
  save, load and pin. Rejected as a dependency: these tools are personal-scope, with no multi-writer model, no review
  and no conflict handling, and memcrate itself is pre-release. The underlying ideas are good and we adopt them:
  human-readable Markdown, and the explicit promotion of an insight. The dependency itself is not worth taking.

* **Keep knowledge in ADO work items.** Functional detail already lives there, and ADO has real search. Rejected: a work
  item is a delivery artefact whose lifecycle ends at "done". Durable knowledge outlives the work that produced it. A
  standard, a decision and a runbook have no natural home in a backlog. We link to ADO and do not duplicate it.

## Consequences

* **Every document will acquire a schema obligation.** A contributor picks a type, allocates an ID and completes the
  required frontmatter. That adds friction, in exchange for validation and generation.
* **CI will become a gate on documentation.** A malformed document fails the build. Nothing blocks a documentation
  change today, so the first few failures could irritate.
* **Existing content will have to migrate.** The ADRs and standards need frontmatter replacing their in-prose metadata.
* **The generator will own every index.** It marks each region it owns, and CI fails when a marked region is stale. A
  table can no longer disagree with its source unnoticed.
* **Agent-written content will enter the corpus**, at a lower authority tier and with an explicit promotion path. The
  value of the whole exercise depends on the promotion gate working. If the gate fails, the corpus fills with
  plausible, unverified assertions, and trust in everything else falls with them.
* **The corpus will grow from a few dozen documents towards several hundred** as the taxonomy fills. A generated index
  matters at that size, so we automate the indexes before the growth.
* **Session logs will stay out of this corpus.** They routinely contain credentials and customer data. We commit only
  distilled, reviewed discoveries.
* **The mechanism will be reusable elsewhere.** Keeping the schema, the validators and the skills free of organisation
  specifics takes discipline, and makes the approach portable to another estate.

## References

* [Knowledge as Code](../knowledge-as-code.md) describes the approach.
* [Taxonomy](../knowledge-as-code/taxonomy.md) lists the types and how to choose between them.
* [Metadata](../knowledge-as-code/metadata.md) defines the frontmatter schema.
* [Contributing](../knowledge-as-code/contributing.md) covers the review model and the promotion path.
* [Automation](https://paul80nd.github.io/knowledge-as-code/framework/automation/) says what CI validates and generates.
