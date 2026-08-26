# Principles

Why the framework is shaped the way it is. A corpus's own taxonomy page says what its types are. This page says what
they are for, and what the design will not trade away.

These are meant to outlast the implementation. Each describes how the framework works today. The
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds what's to come.

## Behaviour before subject

What a document is about and how it behaves are different things. Behaviour sets the rules, and everything else on this
page keys off that.

An ADR and a postmortem have nothing in common as subject matter. As behaviour they are identical: both are accounts
that were true at a moment, both become misleading if edited afterwards, and both are superseded rather than corrected.
So they share a **tier**, meaning the group a type belongs to by how it behaves, and the tier carries the rules: the
review bar, the validation, the language, and the expectation of immutability.

The taxonomy therefore grows without new machinery.

## The mechanism is separable from the knowledge

A corpus has two halves. The **mechanism** is the schema, the validator, the generator and the agent skills, and it is
identical everywhere. The **knowledge** is the organisation's, and it is shared with nobody.

That split lets the mechanism improve without touching anyone's content, and lets an organisation take a copy without
inheriting someone else's retention policy. It also turns *which of these files are mine?* into a question
[`kac update --check`](../cli/update.md) answers.

The framework's own `manifest.yaml` declares which files fall on which side, so no prose has to assert it. Every file
resolves to exactly one **layer**, meaning who owns a file and what happens when it differs from upstream, and each
layer has a rule about what divergence means.

## Schema before prose

The authoritative form of a record is structured. Prose renders it.

`kac` generates an index from frontmatter. It validates every cross-reference on every build, and derives a type's
schema table from the schema file that table documents. Where a machine could derive something and a person maintains it
instead, the person's copy is the one that goes stale, and nothing will say so.

This is also why adding a knowledge type means adding a YAML file rather than editing the tool. A corpus grows its
taxonomy without waiting on a change to the code.

## Knowledge is a graph

Repositories and folders are storage. The relationships are the knowledge.

A policy is implemented by a standard, verified by a control, applied to a service, contributing to a capability. That
chain is meaningful whichever folder each record sits in, and it carries the value: a service document is a fact, but a
service document that can tell you which standards bind it is an answer.

The graph is also the part that breaks silently. So reciprocal edges must agree in both directions, and a one-sided link
fails the build.

## One authoritative owner

Every record has exactly one home. Where two places need the same knowledge, the second links to the first rather than
copying it.

 Someone updates one copy and not the other, and afterwards nobody can tell which
is current, so a reader believes whichever they found first. A corpus with two answers is worse than a corpus with none,
because a gap is visible and a contradiction is not.

## Cheap capture, deliberate promotion

Capture has to be nearly free or it does not happen. Nobody writes up a gotcha if doing so needs a template, an owner
and two reviewers. So you record an observation with no review at all and mark it unverified, and it expires on its own
if nothing promotes it.

The rigour lives at promotion. A discovery becomes an FAQ when a human confirms it, and the FAQ carries provenance back
to the observation. That gradient lets a corpus grow without its average trustworthiness falling.

## Trust is what a corpus is for

The failure mode of a wiki is not too little content. It is content nobody believes.

Every mechanism here serves that. A generated index cannot be stale, a validated link cannot rot quietly, an immutable
decision cannot be rewritten, and a rule with no control is recorded as unenforced rather than assumed. Each of those
makes a record harder to add, and that is the trade this framework takes.

## What you own, and what you install

An organisation adopting this framework owns what it writes and what says how to write it: the records, the schema, and
the framework's own documentation. Nobody upstream approves a change to any of that, and there is nothing to hand back
if they later go their own way.

What comes from upstream is `kac` itself, installed as a versioned tool and pinned the way any other dependency is. A
newer one is a version they choose to take.

The cost is **drift** in the half they own, meaning a file that no longer matches the upstream it came from. A manifest
and a descriptor answer it. A corpus records which version of the shared layer it is on, and which divergences it has
deliberately accepted, so a necessary deviation does not look like an accident.

## Readable and writable by agents

A wiki only humans write grows at the rate humans remember to write. A wiki that AI sessions can also write to grows at
the rate work happens.

That changes what the corpus has to be. A human infers structure from the layout, and an agent needs it stated. A human
guesses placement correctly, and an agent needs a decision table. A human reads authority from the tone, and an agent
needs it in a field. Almost every design choice here answers at least partly to that: typed documents, explicit tiers, a
glossary treated as load-bearing, and validation that fails rather than warns.

None of it replaces human ownership.

## One corpus, one bounded context

A corpus is one repository holding everything: policies, standards, services, runbooks and the rest, with the mechanism
alongside.

Most of this page rests on that boundary. The validator fails a one-sided link because it can see both ends of it. An
owner is authoritative because there is a single place the record could be. A session clones the repository and has the
whole graph, with nobody to ask for the rest of it.


