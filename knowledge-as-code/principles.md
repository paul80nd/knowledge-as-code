# Principles

Why the framework is shaped the way it is. [Taxonomy](taxonomy.md) says what the types are; this page says what they are
for, and what the design will not trade away.

These are meant to outlast the implementation. Each describes how the framework works today; where one is only partly
built, the tracker holds the rest.

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
nothing promotes them. The rigour lives at promotion instead: a discovery becomes an FAQ when a human confirms it, and
the FAQ carries provenance back to the observation.

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

## One corpus, one bounded context

A corpus is one repository holding everything: policies, standards, services, runbooks and the rest, with the mechanism
alongside. One product, one estate, one governance conversation.

Most of this page rests on that boundary. The validator fails a one-sided link because it can see both ends of it. An
owner is authoritative because there is a single place the record could be. A session clones the repository and has the
whole graph, with nobody to ask for the rest of it.

Nothing here asks a corpus to reach outside itself. An organisation running one bounded context needs the mechanism in
this repository and nothing beyond it.
