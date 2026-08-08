# Principles

Why the framework is shaped the way it is. [Taxonomy](taxonomy.md) says what the types are; this page says what they are
for, and what the design will not trade away.

These are meant to outlast the implementation. Where a principle is not yet fully built, it is marked — see
[what is not built yet](authoring.md#what-is-not-built-yet) for what the markers mean.

## Behaviour before subject

The single most useful idea here, and the one everything else keys off: **what a document is about and how it behaves
are different things, and it is behaviour that determines the rules.**

An ADR and a postmortem have nothing in common as subject matter. As behaviour they are identical — both are accounts
that were true at a moment, both become misleading if edited afterwards, both are superseded rather than corrected. So
they share a tier, and the tier carries the rules: the review bar, the validation, the language, the expectation of
immutability.

This is what stops the taxonomy from needing new machinery every time it grows. A new kind of knowledge does not need a
new mechanism. It needs a tier.

## The mechanism is separable from the knowledge

A corpus has two halves. The **mechanism** — schema, validator, generator, agent skills — is generic and identical
everywhere. The **knowledge** is the organisation's, and is shared with nobody.

The split is load-bearing rather than tidy. It is what lets the mechanism improve without touching anyone's content,
what lets an organisation take a copy without inheriting someone else's opinions about accessibility, and what makes
"which of these files are mine?" a question with a checkable answer rather than a judgement call.

Which files fall on which side is declared in [`manifest.yaml`](manifest.yaml), not asserted in prose. Every file
resolves to exactly one layer, and each layer has a rule about what divergence means.

## Schema before prose

The authoritative form of a record is structured. Prose renders it.

An index is generated from frontmatter rather than maintained beside it. A cross-reference is validated rather than
trusted. A type's schema table is derived from the schema file rather than written to match it. Anything a machine can
derive, a human should not be maintaining — because the copy a human maintains is the copy that goes stale, and nothing
will say so.

This is also why **adding a knowledge type is adding a YAML file**, not editing the tool. A taxonomy that can only grow
by changing code is a taxonomy that stops growing.

## Knowledge is a graph

Repositories and folders are storage. The relationships are the knowledge.

A policy is implemented by a standard, verified by a control, applied to a service, contributing to a capability. That
chain is meaningful regardless of which folder each record sits in, and it is the part that carries the value — a
service document is a fact, but a service document that can tell you which standards bind it is an answer.

It is also the part that breaks silently, which is why reciprocal edges must agree in both directions and a one-sided
link fails the build. Documentation is a view over the graph. The graph is the thing.

## One authoritative owner

Every record has exactly one home. Where knowledge is needed in two places it is referenced, never copied.

Duplication does not stay duplicated. One copy gets updated, the other does not, and afterwards nobody can tell which is
current — so a reader believes whichever they found first. A corpus with two answers is worse than a corpus with none,
because the second failure is visible.

## Cheap capture, deliberate promotion

The most important tier is the one carrying the least authority.

Capture has to be nearly free or it does not happen. Nobody writes up a gotcha if doing so requires a template, an owner
and two reviewers — so observations are recorded with no review at all, marked unverified, and expire on their own if
nothing promotes them. The rigour lives at promotion instead: a [discovery](/discoveries) becomes an [FAQ](/faqs) when
a human confirms it, and the FAQ carries provenance back to the observation.

That gradient is what lets a corpus grow without its average trustworthiness falling. Cheap in, expensive up.

## Trust before coverage

The failure mode of a wiki is not too little content. It is content nobody believes.

Every mechanism here serves that: generated indexes cannot be stale, validated links cannot rot quietly, immutable
decisions cannot be quietly rewritten, and a rule with no control is recorded as unenforced rather than assumed. A
corpus that is half the size and entirely believed is worth more than one that is complete and doubted.

## Copied, not depended on

An organisation adopting this framework gets its own cut. No runtime dependency, no upstream to ask permission from,
nothing to remove if they later go their own way.

The cost is drift, and drift is met with a manifest and a lockfile rather than with a prohibition. A corpus records
which version of the shared layer it is on and which divergences it has deliberately accepted, so that a necessary
deviation does not have to masquerade as an accident.

## Readable and writable by agents

A wiki only humans write grows at the rate humans remember to write. A wiki that AI sessions can also write to grows at
the rate work happens.

That changes what the corpus has to be. Structure that a human infers from layout, an agent needs stated. Placement that
a human guesses correctly, an agent needs a decision table for. Authority that a human reads from tone, an agent needs
in a field. Almost every design choice here — typed documents, explicit tiers, a glossary treated as load-bearing,
validation that fails rather than warns — is at least partly this.

It does not replace human ownership. It means humans and agents work against one model rather than two.

## Deployment models

### A single corpus

One repository holding everything: policies, standards, services, runbooks and the rest, with the mechanism alongside.
This is the whole model for a single bounded context — one product, one estate, one governance conversation.

It is the first-class case and will stay so. Nothing below is required to use this framework, and the complexity of the
tiered model should never land on an organisation that does not need it.

### Tiered corpora

> **Planned.** The shape is agreed. None of the mechanism described here is built — tracked in
> [knowledge-as-code#93](https://github.com/paul80nd/knowledge-as-code/issues/93).

A larger organisation has several bounded contexts and one governance conversation. Without a way to share, each context
copies the standards and controls it is bound by, and the copies drift — which [one authoritative
owner](#one-authoritative-owner) says is the failure to avoid.

The intended shape is three layers. The **framework** holds schema, validator and skills. A **governance corpus** holds
the organisation-wide layer — policies, standards, controls, tools, glossary, cross-cutting decisions — changing slowly
and approved broadly. **Domain corpora** hold the bounded context's own knowledge — capabilities, services, data,
runbooks, local decisions — changing quickly and owned locally.

The relationship is consumption, not containment. A domain corpus does not belong to the governance corpus; it consumes
it, and remains free to hold decisions the governance layer has no opinion about.

This would be complete when a domain corpus can validate a reference to a governance record it does not hold, and a
coverage report can span the boundary.

## Where this is going

Each of these follows from a principle above and is not yet built.

**Explicit dependencies.**

> **Planned.** A corpus declares what it consumes, in its lockfile, by name and version. Dependencies are declared and
> never inferred from folder structure or naming convention — the same discipline `manifest.yaml` already applies to the
> shared layer, extended from files to corpora. Tracked in
> [knowledge-as-code#93](https://github.com/paul80nd/knowledge-as-code/issues/93).

**Versioned consumption.**

> **Planned.** A corpus validates against a published version of an upstream corpus, never against the current state of
> another repository's default branch. Validation that depends on someone else's uncommitted afternoon is not
> reproducible, and the argument is the one that already applies to software packages. Tracked in
> [knowledge-as-code#93](https://github.com/paul80nd/knowledge-as-code/issues/93).

**Published contracts.**

> **Aspirational.** A corpus publishes a machine-readable description of its graph — records, types, statuses, and the
> anchors within them — as a build artefact. Consumers import the description rather than cloning the repository, which
> makes the coupling explicit and the build cheap.

**Addressing below the document.**

> **Aspirational** across corpus boundaries. Within a corpus this partly exists: a policy aligns with an external
> framework through a single clause rather than in its entirety, so the edge leaves the clause and not the document.
> Extending that to standards, and then across a corpus boundary, is what would make coverage analysis and evidence
> mapping precise rather than approximate. The notation is not settled and should be settled once, not per type.

**Agents composing several corpora.**

> **Aspirational.** A session working in a domain context loads the domain corpus, the governance corpus above it and
> the framework's own rules, and sees one graph. This is the point of the tiered model rather than a consequence of it —
> everything else is machinery for making that composition trustworthy.
