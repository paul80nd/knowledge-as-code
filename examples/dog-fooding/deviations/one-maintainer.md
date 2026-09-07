---
id: dev-one-maintainer
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-KNOW.HEADS
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
owner: paul.law
tags: [ knowledge, maintainer, resilience ]
---

# One person knows how this repository works

`Deviation: dev-one-maintainer` `ACTIVE`

This repository has one maintainer, and nothing tests what only they know.

## What we are doing instead

A great deal is written down. Four `CLAUDE.md` files route the work, the playbooks under `.claude/skills/i-want-to/`
carry the steps, and this corpus records the rules a change answers to. Any of it can be read by somebody arriving
cold, and agents do exactly that every day.

None of it has been read by a second person who then had to act on it. The proof that the writing is enough is that an
agent can follow it, which is a weaker test than a human taking over.

## Why we need it

There is no second person to spread the knowledge to. Writing more of it down is the only move available, and this
repository does that continuously: a session that finds guidance wanting is asked to say so in the reply that closes
it.

## What compensates

* The repository is public, under the MIT licence, so nothing is lost if the maintainer stops.
* The guidance is written for a reader with no context, and agents test that reading daily.
* Every published version of `kac` stays on nuget.org whatever happens to the repository.
* A session here reports where the guidance failed it, and that feedback is what keeps the writing honest.

## How it closes

A second maintainer takes a change through from issue to release without asking the first anything. What they had to
ask becomes an edit to the guidance. This record closes when that has happened once.

## Scope

Everything this repository holds: the tool, the schema, the four corpora, the site and the workflows.

## Related

* [dev-standing-publish-rights] and [dev-decisions-live-in-commits] both rest on this condition.

[dev-decisions-live-in-commits]: decisions-live-in-commits.md
[dev-standing-publish-rights]: standing-publish-rights.md
