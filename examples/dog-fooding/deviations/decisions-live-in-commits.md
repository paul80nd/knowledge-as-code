---
id: dev-decisions-live-in-commits
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-KNOW.DECIDE
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
owner: human:paul.law
tags: [ decisions, knowledge, provenance ]
---

# The reasoning behind a decision lives in the commit that made it

`Deviation: dev-decisions-live-in-commits` `ACTIVE`

This corpus declined the `adrs` type, so a decision about `kac` is recorded in a commit message and in
`tooling/CLAUDE.md`.

## What we are doing instead

A commit message here explains at length why the change was made, and says little about what changed. The pull request
body gives the evidence. `tooling/CLAUDE.md` records the decisions a contributor has to know before they change the
tool.

Neither has an id. Nothing cites either from a record, and nothing supersedes one when the decision is reversed.
Finding the reasoning behind a design means reading `git log` and knowing what to search for.

## Why we need it

`adrs/` in the corpora beside this one records the invented estate's decisions, and a decision about the tool would not
belong among them. Adopting the type here means moving several years of reasoning out of the commit messages that
already contain it. Nobody has needed it badly enough to do that.

## What compensates

* A commit message here says why, which [std-PROSE] requires, and the history is public.
* `tooling/CLAUDE.md` records the decisions a contributor meets, so the ones that matter daily are on a page.
* A pull request body gives the evidence behind the change, and the branch rule means every change has one.
* [dev-one-maintainer] is why this has cost nothing so far. It is also why it would cost a lot on the day a second
  person arrives.

## How it closes

This corpus adopts `adrs`, and the decisions worth citing move into it: only the ones a record needs to point at. This
record closes on that.

## Scope

Every decision about `kac`, the schema, the documentation site or the workflows.

## Related

* [dev-one-maintainer] is why nobody has needed this yet.
* [std-PROSE] requires a commit message to say why.

[dev-one-maintainer]: one-maintainer.md
[std-PROSE]: ../standards/prose.md
