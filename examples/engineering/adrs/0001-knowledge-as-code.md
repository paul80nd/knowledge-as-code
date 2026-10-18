---
id: adr-0001
type: adr
tier: decided
status: accepted
decided-on: "2026-06-15"
owner: human:paul.law
deciders: [ human:paul.law ]
tags: [ documentation, knowledge-management, tooling ]
---

# Knowledge as code: a structured, validated wiki that AI sessions can read and contribute to

`ADR: adr-0001` `ACCEPTED`

> **In the context of** knowledge split across the wiki, ADO and people's memory, **facing** AI sessions that cannot
> find our standards or record findings, **we decided** to treat knowledge as code: Markdown with validated
> frontmatter and generated indexes, **rather than** unstructured pages or a hosted platform, **to achieve** knowledge
> that accumulates and stays current, **accepting** schema obligations and CI gating.

## Context

Our engineering knowledge sits in the wiki, in Azure DevOps (ADO) and in people's memory, and nothing connects them.
Metadata written in prose cannot be validated or indexed, so hand-kept indexes drift and conventions go unenforced. An
AI coding session that cannot find our standards breaks them with confidence. A session that discovers something has
nowhere to put it, so the discovery is lost when the session ends.

Three constraints shape our response.

* People who never open a terminal still read the wiki, so it keeps rendering as an ADO wiki.
* Agent-written content cannot be trusted as far as reviewed content, so the corpus separates the two.
* The corpus grows towards several hundred documents, where an index kept by hand fails.

## Decision

We treat knowledge as code:

* **Every document has a type and validated frontmatter.** The type comes from a
  [taxonomy](../knowledge-as-code/taxonomy.md), and the frontmatter follows a
  [schema](../knowledge-as-code/metadata.md). The type's tier sets its review bar and its lifecycle.
* **Cheap capture happens in the tracker.** A session files an observation as a work item. It becomes a record only
  when a person verifies it.
* **CI validates** the frontmatter, the ids and the links, and generates every index.

[Knowledge as Code](../knowledge-as-code.md) describes the full approach.

## Alternatives Considered

* **Keep the wiki as unstructured pages** (the status quo). Rejected: without a schema no tool can validate or generate
  anything, and an agent has no reliable way to find or contribute knowledge.
* **Move to a hosted knowledge platform (Confluence, SharePoint, Notion).** It offers better discovery and editing.
  Rejected because an agent can read those platforms but cannot write to them, and a page there cannot be reviewed in
  the pull request that changed the code it describes.
* **A vector-indexed store with semantic search.** Rejected as premature: at a few dozen documents it adds little to
  grep over a generated index, and a retrieved chunk of a rejected alternative can read like a decision.
* **Adopt an off-the-shelf agent memory tool**, such as memcrate. Rejected as a dependency: these tools are
  personal-scope, with no multi-writer model and no review. We adopt their ideas: Markdown, and the explicit promotion
  of an insight.
* **Keep knowledge in ADO work items.** Adopted for capture, rejected as the store. A work item's lifecycle ends at
  "done", and durable knowledge outlives the work that produced it.

## Consequences

* **Every document will acquire a schema obligation.** A contributor picks a type and completes the frontmatter, which
  adds friction in exchange for validation.
* **CI will gate documentation.** A malformed document fails the build, and the first few failures could irritate.
* **Existing ADRs and standards will have to migrate** into records with frontmatter.
* **The value depends on that verification.** If unverified observations reach the corpus, trust in everything else
  falls with them.
* **Session logs will stay out of this corpus**, because they contain credentials and customer data.

## References

* [Running it in CI](https://paul80nd.github.io/knowledge-as-code/use/ci/#what-the-checks-ask) says what CI validates
  and generates.
