---
id: dev-writing-skills-restate-prose
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-KNOW.COPY
risk: low
accepted-on: "2026-10-25"
review-by: "2027-10-25"
closed-on:
owner: human:paul.law
tags: [ agents, prose, skills ]
---

# The writing skills state std-PROSE's rules a second time

`Deviation: dev-writing-skills-restate-prose` `ACTIVE`

The writing skills under `.claude/skills/` keep their own copy of the prose rules [std-PROSE] states, for agents to
read. This stands while a corpus can receive the skills without the standard.

## What we are doing instead

[std-PROSE] is the record the estate cites for how prose is written here. The writing skills state the same rules for
an agent, and cite no record:

* `technical-writing`, the floor every surface answers to
* `writing-a-record`, `writing-a-report`, `writing-in-the-tool` and `writing-the-docs`, one voice each

`manifest.yaml` sends `technical-writing`, `writing-a-record` and `writing-a-report` to every corpus built from the
template. The other two stay in this repository.

## Why we need it

A corpus receiving the template may have no standards at all, so a skill linking to [std-PROSE] would send its agent to
a page that is not there. A skill stating the rules itself works in any checkout. The alternative is a skill that works
only here, and a corpus elsewhere that writes with no rules.

## What the risk is

The two copies drift. A rule changed in [std-PROSE] and left alone in a skill means an agent writes to the old rule,
and a reviewer reading the standard sends the work back. Nothing checks that the copies agree. The cost falls on
whoever reviews that agent's work.

## What compensates

* [std-PROSE] names the writing skills in its Rationale, so whoever changes a rule there sees that a skill states it as
  well.
* [dev-one-maintainer] is why the drift has cost little so far: the person changing the standard is the person who
  maintains the skills.

## How it closes

Every corpus that receives the skills also receives the standard, and each skill links its rules instead of stating
them. That needs a way for the template to send a standard, and the template sends none today. Where the owner
re-accepts this instead, the review weighs whether the skills and the standard have disagreed in practice.

## Scope

The writing skills under `.claude/skills/` and the copies `manifest.yaml` sends to each corpus. The skills a plugin
bundles are outside it, because [std-PLUGIN] has each of those quote the export and link the record for the rest.

## Related

* [std-PROSE] states the rules the skills copy.
* [std-PLUGIN] governs the skills a plugin bundles.
* [dev-one-maintainer] is why one person keeps both copies in step.

[dev-one-maintainer]: one-maintainer.md
[std-PLUGIN]: ../standards/plugin.md
[std-PROSE]: ../standards/prose.md
