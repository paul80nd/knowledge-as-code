---
id: adr-0001
type: adr
tier: decided
status: accepted
decided-on: "2026-08-05"
owner: human:mira.okonjo
deciders: [ human:alex.doe, human:mira.okonjo, human:robin.hale ]
tags: [ documentation, knowledge-management, tooling ]
---

# Knowledge as code: a structured, validated wiki that AI sessions can read and contribute to

`ADR: adr-0001` `ACCEPTED`

> **In the context of** knowledge split across the staff wiki, the tracker and the team's memory, **facing** AI
> sessions that cannot find it or add to it, **we decided** to treat knowledge as code: Markdown with validated
> frontmatter and generated indexes, **rather than** a hosted platform, **to achieve** knowledge that accumulates and
> stays current, **accepting** schema obligations and CI gating.

## Context

What the consortium knows about its own systems sits in three places, and nothing connects them. The staff wiki
explains the catalogue to branch staff. The work tracker keeps delivery detail in epics and stories. Everything else is
in the platform team's memory or in a chat message nobody can find: why a nightly check still runs, which of the
components in one repository deploy separately, and which supplier owns an API a service calls.

The wiki content that exists is good. Its metadata is written in prose, so nothing can validate, cross-check or index
it. Contributors maintain the indexes by hand, so those indexes drift. A page listing which services call another is
correct the day somebody writes it, and nothing tells the next reader when it stopped being correct.

An AI coding session that cannot find the conventions produces work that breaks them. It does so with confidence. A
session that discovers something worth knowing has nowhere to put it, whether that's a deployment step nobody wrote
down or a fix that took two hours. The discovery is lost when the session ends.

Three constraints shape the response.

* Branch staff read this material and will never open a terminal, so whatever replaces the wiki still renders as a
  wiki.
* Agent-written content cannot be trusted as far as reviewed content, so the corpus separates the two.
* The estate spans several repositories, so anything relying on a person to maintain an index by hand fails as the
  corpus fills.

## Decision

We treat knowledge as code:

* **Every document has a type**, drawn from a [taxonomy](../knowledge-as-code/taxonomy.md), and is filed in a folder
  for that type.
* **Every document has YAML frontmatter** conforming to a [schema](../knowledge-as-code/metadata.md). The wiki renders
  that frontmatter as a metadata table, which replaces metadata in prose. A person reads it and automation uses it.
* **We group types into tiers by behaviour**: decided, normative, descriptive and procedural. The tier sets the review
  bar, the validation rules and the lifecycle.
* **Cheap capture happens in the tracker.** A session files an observation as a work item, with no review. It becomes a
  service record or a runbook only when a person verifies it.
* **CI validates** schema conformance, ID uniqueness, link resolution and bidirectional relationships. It generates the
  indexes and an always-loaded rules digest.
* **The mechanism is separable from the content.** The schema, the validators and the skills arrive from upstream and
  describe nothing about this consortium.

[Knowledge as Code](../knowledge-as-code.md) describes the full approach.

## Alternatives Considered

* **Keep the wiki as unstructured Markdown pages** (the status quo). Rejected: without a schema no tool can validate or
  generate anything, so every index stays hand-maintained and every convention stays unenforced. The status quo also
  gives a session no reliable way to find or contribute knowledge. That gap is why this ADR exists.

* **Keep each repository's knowledge in its own README.** It's the cheapest option, it needs no tooling, and a
  developer already looks there. Rejected: most of what this consortium needs written down spans repositories. A
  decision about how services authenticate to each other is invisible from every repository but the one it was written
  in, and branch staff open none of them. Per-repository notes stay for what is genuinely local to one repository.

* **Move to a hosted knowledge platform**: Confluence, SharePoint, Notion or similar. Such platforms offer better
  discovery, better editing and broader reach across non-technical staff. Rejected because an agent can read those
  platforms but cannot write to them. The Microsoft 365 connector exposes search and read tools only. The documentation
  also moves away from the code it describes, so nobody can review a page in the same pull request as the change that
  invalidated it.

* **A vector-indexed knowledge store with semantic search** solves retrieval at scale and finds related material
  without curated links. Rejected as premature: at a few dozen documents the index goes stale and adds little to grep
  over a generated index. It's also risky here, because vector search returns *chunks*. An ADR's "Alternatives
  Considered" section contains confident descriptions of options we rejected, and a chunk retrieved out of context
  could read like a decision. Explicit `related` links, which we already write, are more precise, because they record
  *how* two documents relate. If a generated index stops being navigable, revisit this decision.

* **Adopt an off-the-shelf agent memory tool**: memcrate or something like it. It's a ready-made store with commands to
  save, load and pin. Rejected as a dependency: these tools are personal-scope, with no multi-writer model, no review
  and no conflict handling, and memcrate itself is pre-release. The underlying ideas are good and we adopt them:
  human-readable Markdown, and the explicit promotion of an insight. The dependency itself is not worth taking.

* **Keep knowledge in the work tracker.** Delivery detail already lives there, and the tracker has real search.
  Adopted for capture, rejected as the store. A work item is a delivery artefact whose lifecycle ends at "done".
  Durable knowledge outlives the work that produced it. A decision, a service description and a runbook have no natural
  home in a backlog. The corpus keeps the answer somebody settled. It links the work item and does not duplicate it.

## Consequences

* **Every document will acquire a schema obligation.** A contributor picks a type, allocates an ID and completes the
  required frontmatter. That adds friction, in exchange for validation and generation.
* **CI will become a gate on documentation.** A malformed document fails the build. Nothing blocks a documentation
  change today, so the first few failures could irritate.
* **Branch staff will meet a metadata table above every page.** The wiki renders frontmatter, so a reader who wants one
  procedure reads past a block written for automation.
* **Existing wiki content will have to migrate.** The pages worth keeping need frontmatter replacing their in-prose
  metadata, and somebody has to decide which pages those are.
* **The generator will own every index.** It marks each region it owns, and CI fails when a marked region is stale. A
  table can no longer disagree with its source unnoticed.
* **Agent-written content will enter the corpus**, through the tracker and with an explicit promotion path. The value
  of the whole exercise depends on the promotion gate working. If the gate fails, the corpus fills with plausible,
  unverified assertions, and trust in everything else falls with them.
* **The corpus will grow from a few dozen documents towards several hundred** as the taxonomy fills. A generated index
  matters at that size, so we automate the indexes before the growth.
* **Session logs will stay out of this corpus.** They routinely contain borrower data and credentials. We commit only
  distilled, reviewed records.
* **The mechanism will be reusable elsewhere.** Keeping the schema, the validators and the skills free of consortium
  specifics takes discipline, and makes the approach portable to another estate.

## References

* [Knowledge as Code](../knowledge-as-code.md) describes the approach.
* [Taxonomy](../knowledge-as-code/taxonomy.md) lists the types and how to choose between them.
* [Metadata](../knowledge-as-code/metadata.md) defines the frontmatter schema.
* [Contributing](../knowledge-as-code/contributing.md) covers the review model and the promotion path.
* [Automation](https://paul80nd.github.io/knowledge-as-code/framework/automation/) says what CI validates and generates.
