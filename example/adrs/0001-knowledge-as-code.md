---
id: adr-0001
tier: decided
status: proposed
owner: paul.law
tags: [ documentation, knowledge-management, tooling ]
---

# Knowledge as code — a structured, validated wiki that AI sessions can read and contribute to

`ADR: adr-0001` `PROPOSED`

> **In the context of** knowledge split across the wiki, ADO and heads, **facing** AI sessions that cannot find our
> standards or record findings, **we decided** to treat knowledge as code: Markdown with validated frontmatter and
> generated indexes, **rather than** unstructured pages or a hosted platform, **to achieve** knowledge that compounds
> rather than decays, **accepting** schema obligations and CI gating.

## Context

Our engineering knowledge sits in three places that do not talk to each other. This wiki holds the architectural
reasoning in [ADRs](/adrs) and the rulebook in [standards](/standards). Azure DevOps (ADO) holds functional detail in
epics, features and stories. Everything else — operational know-how, the fixes for recurring problems, why a
particular tool was chosen — lives in people's memory or in a chat message nobody can find.

The wiki content that does exist is good. Its metadata sits in handwritten Markdown tables, so nothing can validate,
cross-check or index it. Contributors maintain the ADR and standards indexes by hand, so those indexes drift. The
conventions in those documents — sequential IDs, bidirectional supersession, standards citing the ADRs they derive
from — are real rules that nothing enforces.

AI coding sessions make both halves of the problem worse. A session that cannot find the relevant standard confidently
produces work that violates it. The standards already carry **MUST** and **MUST NOT** rules for a session to miss. In
the other direction, a session that discovers something worth knowing — a non-obvious failure mode, a fix that took
two hours to find — has nowhere to put it, so the discovery dies when the session ends.

Three constraints shape the response. The wiki must stay readable by people who will never open a terminal, so
whatever we build keeps rendering as an ADO wiki. We cannot trust agent-written content at the level we trust reviewed
content, so the corpus has to distinguish the two. The corpus will grow from a few dozen documents towards several
hundred, and anything relying on a person to maintain an index by hand fails at that size.

## Decision

We treat knowledge as code:

* **Every document has a type**, drawn from a [taxonomy](/knowledge-as-code/taxonomy.md), and lives in the folder for
  that type.
* **Every document carries YAML frontmatter** conforming to a [schema](/knowledge-as-code/metadata.md). ADO renders
  that frontmatter as a metadata table, so a human reader sees it. The rendered table replaces the handwritten metadata
  tables the ADRs carry.
* **We group types into tiers by behaviour**: decided, normative, descriptive, procedural and observed. The tier decides
  the review bar, the validation rules and the lifecycle, and the type does not.
* **Observed knowledge is cheap to capture and deliberate to promote.** A session records an unverified discovery with
  no review. The discovery expires by default. Promotion to an FAQ or a standard needs a human.
* **CI validates** schema conformance, ID uniqueness, link resolution and bidirectional relationships. It generates
  indexes, reports and an always-loaded rules digest.
* **The mechanism is separable from the content.** Schema, validators, generators and skills live under
  `knowledge-as-code/` and `tooling/`. They carry no organisation specifics, and the taxonomy does not govern them.

[Knowledge as Code](/knowledge-as-code.md) describes the full approach.

## Alternatives Considered

* **Keep the wiki as unstructured Markdown pages**: the status quo. Rejected: without a schema no tool can validate or
  generate anything, so every index stays hand-maintained and every convention stays unenforced. The status quo also
  gives an agent no reliable way to find or contribute knowledge. That gap is why this ADR exists.

* **Move to a hosted knowledge platform**: Confluence, SharePoint or Notion. Such a platform offers better discovery,
  better editing and broader reach across non-engineers. Rejected: an agent can read those platforms and cannot write
  to them, and the Microsoft 365 connector exposes search and read tools only. The accumulation half of the problem is
  the half we cannot solve today, so a platform with no write path cannot be the answer. Documentation also stops
  travelling with the code it describes. Nobody can then review a page in the same pull request as the change that
  invalidated it.

* **A vector-indexed knowledge store with semantic search** solves retrieval at scale and finds related material
  without curated links. Rejected as premature: at a few dozen documents the index goes stale and buys little over grep
  plus a generated index. It is also risky here, because vector search returns *chunks*. An ADR's "Alternatives
  Considered" section holds confident descriptions of options we rejected, and a chunk retrieved out of context reads
  exactly like a decision. Explicit `related` links, which we already write, are more precise because they record *how*
  two documents relate. If a generated index stops being navigable, revisit this decision.

* **Adopt an off-the-shelf agent memory tool**: memcrate or something like it. It is a ready-made vault with verbs to
  save, load and pin. Rejected as a dependency: these tools are personal-scope, with no multi-writer model, no review
  and no conflict handling, and memcrate itself is pre-release. The underlying ideas — human-readable Markdown,
  explicit promotion of an insight into durable knowledge — are good, and we adopt them. The dependency is not worth
  taking for a directory convention and three skills.

* **Keep knowledge in ADO work items.** Functional detail already lives there, and ADO has real search. Rejected: a
  work item is a delivery artefact whose lifecycle ends at "done". Durable knowledge outlives the work that produced
  it. A standard, a decision and a runbook have no natural home in a backlog. We link to ADO and do not duplicate it.

## Consequences

* **Every document will acquire a schema obligation.** A contributor picks a type, allocates an ID and completes the
  required frontmatter. That is friction, and we place it where it buys validation and generation.
* **CI will become a gate on documentation.** A malformed document fails the build. Nothing blocks a documentation
  change today, and the first few failures will irritate.
* **Existing content will have to migrate.** The ADRs and standards need frontmatter, and the rendered table replaces
  their handwritten metadata tables. The service catalogue moves out of the root README.
* **The generator will own every index.** It marks each region it owns, and CI fails when a marked region is stale. A
  table can no longer silently disagree with its source.
* **Agent-written content will enter the corpus**, at a lower authority tier and with an explicit promotion path. The
  value of the whole exercise depends on the promotion gate holding. If the gate fails, the corpus fills with
  plausible, unverified assertions, and trust in everything else falls with them.
* **The corpus will grow from a few dozen documents towards several hundred** as the taxonomy fills. A generated index
  becomes load-bearing at that size, so we build the indexes before the growth.
* **The generator will write a rules digest into the repository root.** An AI session loads it automatically. The
  digest carries a hard size budget. When the budget binds, we triage the rules and leave the budget alone.
* **Session logs will stay out of this repository.** They routinely contain credentials and customer data. We commit
  only distilled, reviewed discoveries.
* **The mechanism will be reusable elsewhere.** Keeping schema, validators and skills free of organisation specifics
  costs a little discipline. That discipline makes the approach portable to another estate.

## References

* [Knowledge as Code](/knowledge-as-code.md) describes the approach.
* [Taxonomy](/knowledge-as-code/taxonomy.md) lists the types and how to choose between them.
* [Metadata](/knowledge-as-code/metadata.md) defines the frontmatter schema.
* [Contributing](/knowledge-as-code/contributing.md) covers the review model and the promotion path.
* [Automation](/knowledge-as-code/automation.md) says what CI validates and generates.
