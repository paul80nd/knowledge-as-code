# Principles

`kac` keeps an organisation's engineering knowledge as validated, identified and versioned Markdown, and packages it for
the agents that use it.

This page says why the framework is shaped the way it is, and what the design will not trade away. Each principle
describes how the framework works today. [Design intent](intent.md) states the direction the framework is being built
towards, and a principle moves here from that page once it is true.

## Behaviour before subject

What a document is about and how it behaves are different things. Behaviour sets the rules, and everything else on this
page keys off that.

An ADR and a postmortem have nothing in common as subject matter. As behaviour they are identical: both are accounts
that were true at a moment, both become misleading if their conclusion is edited afterwards, and both are superseded
rather than corrected. So they share a **tier**, meaning the group a type belongs to by how it behaves. The tier sets
the rules: the review bar, the validation, the language, and the expectation of immutability.

The taxonomy therefore grows without new machinery.

## The mechanism is separable from the knowledge

A corpus has two halves. The **mechanism** is the schema, the validator, the generator and the agent skills, and it is
identical everywhere. The **knowledge** is the organisation's, and it is shared with nobody.

That split lets the mechanism improve without touching anyone's content, and lets an organisation take a copy without
inheriting someone else's retention policy. The framework's own `manifest.yaml` declares which side each file falls
on, and [Layers](../use/layers.md) says what an update does with each.

## Schema before prose

The authoritative form of a record is structured. Prose renders it.

`kac` generates an index from frontmatter. It validates every cross-reference on every build, and derives a type's
schema table from the schema file that table documents. Where a machine could derive something and a person maintains it
instead, the person's copy is the one that goes stale, and nothing says so.

This is also why adding a knowledge type means adding a YAML file rather than editing the tool. A corpus grows its
taxonomy without waiting on a change to the code.

## Knowledge is a graph

Repositories and folders are storage. The relationships are the knowledge.

A policy is implemented by a standard, verified by a control, applied to a service, contributing to an offering. That
chain is meaningful whichever folder each record sits in, and it is where the value is. A service document is a fact. A
service document that can tell you which standards bind it is an answer.

The graph is also the part that breaks silently. So reciprocal edges must agree in both directions, and a one-sided link
fails the build.

## One authoritative owner

Every record has exactly one home. Where two places need the same knowledge, the second links to the first rather than
copying it.

A copy does not stay in step. Someone updates one and not the other, and afterwards nobody can tell which is current, so
a reader believes whichever they found first. A corpus with two answers is worse than a corpus with none, because a gap
is visible and a contradiction is not.

## In through the tracker, out through the export

Knowledge arrives as a work item and leaves as a record. Each route has its own bar.

Capture has to be nearly free or it does not happen. Nobody writes up a gotcha if doing so needs a template, an owner
and two reviewers. So **observed knowledge**, what somebody noticed and nobody has checked, goes to the tracker the
corpus already uses. It is not a record, it has no tier, and nothing in the corpus has to stand behind it.

Outbound is the **export**, meaning the machine-readable copy of a corpus that another repository installs, and the
agent skills that read it. Only a record travels that way. The rigour lives at promotion: an observation becomes a
fix once somebody verifies it, and the fix names where the content came from in `sources`. So capture stays free, and
what reaches the export stays worth believing.

## Trust is what a corpus is for

The failure mode of a wiki is not too little content. It is content nobody believes.

Every mechanism here serves that. A generated index cannot be stale, a dead link fails the build, an immutable decision
cannot be rewritten, and a rule with no control is recorded as unenforced rather than assumed. Each of those makes a
record harder to add, and that is the trade this framework takes.

## What you own, and what you install

An organisation adopting this framework owns what it writes and what says how to write it: the records, the schema, and
the framework's own documentation. Nobody upstream approves a change to any of that, and there is nothing to hand back
if they later go their own way.

What comes from upstream is `kac` itself, installed as a versioned tool and pinned the way any other dependency is. A
newer one is a version they choose to take.

The cost is **drift** in the half they own, meaning a file that no longer matches the upstream it came from. The
descriptor records which version of the shared layer the corpus is on, and which divergences it has accepted, so a
necessary deviation does not look like an accident.

## Adopt the types you need, and their rules with them

A corpus adopts only the types it has a use for. `kac new` takes a subset, and `kac update --add-type` adopts another
type later. A team that keeps only standards and a glossary runs a corpus of two types.

Each type it adopts arrives with opinions: the fields a record carries, the sections it holds, and the checks that
fail it. Those opinions are what make records of one type read alike. They are also yours. The schema is a YAML file
in your corpus, and you change a rule by editing it.

## The Markdown outlives the tool

A record is a Markdown file with YAML frontmatter. It reads as a page in any viewer, with or without `kac`.

An organisation that stops running `kac` keeps a wiki. The generated indexes stop updating, and nothing else is lost.
So a corpus that only ever uses `kac` as a validator still keeps what it was worth: knowledge written down, and kept
correct while it ran.

## Where the design departs from prior art, it says why

Each type is built on established practice, and each one states where it differs. Every type's schema declares a
`lineage`: its `prior-art`, its `alignment` with that prior art, and a `divergence` saying where it departs and why.
[Lineage](lineage.md) explains how to read one.

A departure nobody can explain is a mistake. One that is stated is a choice a reader can weigh.

## MUST is the baseline, SHOULD is latitude

A standard grades each rule, and a policy each clause. A MUST states the outcome the owner is accountable for. A
control proves it wherever a tool can, and departing from it takes a deviation the owner accepts. A SHOULD states the
usual way to reach that outcome. An engineer or an agent may depart from it, and says why in the change. A standard's
MAY permits, and a policy's COULD states an aspiration.

A standard is the house style and a checklist to review work against. It is a safety net, and it is not there to catch
anyone out. An agent gets the same latitude as an engineer: the baseline is fixed, and inside it the agent uses its
judgement.

## Each type states its level of detail

Each type page states the level of detail a record of that type is written at. A step in a runbook is a command you can
run. A step in a process is an outcome. A rule in a standard states an outcome, with the usual mechanism as a SHOULD.

The writer, the agent and whoever triages a finding all read it. A finding refines a record at its own level, and never
adds detail below it. A step added for one case makes the record longer for every other reader. The level is guidance
for whether a record feels right, so nothing fails a record on it.

The same section states how often a record of the type changes, and how often new ones arrive. An accepted ADR
changes only its status. A fix arrives with each problem somebody solves.

## Readable and writable by agents

A wiki only humans write grows at the rate humans remember to write. A wiki that AI sessions can also write to grows at
the rate work happens.

That changes what the corpus has to be. A human infers structure from the layout, and an agent needs it stated. A human
guesses placement correctly, and an agent needs a decision table. A human reads authority from the tone, and an agent
needs it in a field. Almost every design choice here answers at least partly to that: typed documents, explicit tiers, a
glossary treated as load-bearing, and validation that fails rather than warns.

Structure does not make a record easier for an agent to understand. An agent reads prose as a person does. Structure
makes a record addressable and selectable. A rule's id lets an agent cite the exact rule it followed, and lets a
reviewer point back at it. A standard's `applies-to` lets a lookup pick the standards that bind the service being
changed.

None of it replaces human ownership.

## Skills that state intent

A skill states what it is for, the intent of the type it reads, and the reason for each rule it gives. It leaves the
means to the model's judgement, and sets out steps only where the order is the content: a record of a type an agent
follows, or a write-back or review whose order a rule fixes. [`std-PLUGIN`][std-plugin] states the rule.

Every piece of text the plugin puts in front of an agent is a pointer: a skill's description, a hook's message, a line
of the build index. Its wording decides whether the agent reads what it points at. When a pointer misses, sharpen its
wording first. [`writing-for-agents`][wfa] is the reference for that text.

A skill that did not cover a case gains a reason the agent can apply to the next case, or loses a step. It never gains
a step.

## One corpus, one bounded context

A corpus is one repository holding everything: policies, standards, services, runbooks and the rest, with the mechanism
alongside.

The validator fails a one-sided link because it can see both ends of it, and an owner is authoritative because there
is a single place the record could be. A session clones the repository and has the whole graph, with nobody to ask for
the rest of it. [One corpus or several](../use/one-corpus-or-several.md) says when to split.

[std-plugin]: https://github.com/paul80nd/knowledge-as-code/blob/main/examples/dog-fooding/standards/plugin.md#reason-a-skill-states-what-it-is-for-and-the-reason-for-each-rule-it-gives
[wfa]: https://github.com/mattpocock/skills/tree/v1.3.1/skills/productivity/writing-for-agents
