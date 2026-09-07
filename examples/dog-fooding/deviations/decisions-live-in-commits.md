---
id: dev-decisions-live-in-commits
tier: normative
status: active
departs-from:
  - eng:pol-KNOW.DECIDE
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
owner: paul.law
tags: [ decisions, knowledge, provenance ]
---

# The reasoning behind a decision lives in the commit that made it

`Deviation: dev-decisions-live-in-commits` `ACTIVE`

This corpus declined the `adrs` type, so a decision about `kac` is recorded in a commit message and in
`tooling/CLAUDE.md`.

## What we are doing instead

A commit message here carries the why rather than the what, at length, and a pull request body carries the evidence.
`tooling/CLAUDE.md` holds the decisions a contributor has to know before they change the tool.

Neither has an id. Nothing cites either from a record, and nothing supersedes one when the decision is reversed.
Finding the reasoning behind a design means reading `git log` and knowing what to search for.

## Why we need it

`adrs/` in the corpora beside this one records the invented estate's decisions, and a decision about the tool would sit
oddly among them. Adopting the type here means moving several years of reasoning out of commit
messages that already carry it. Nobody has needed it badly enough to do that.

## What compensates

* A commit message here says why, which [std-PROSE] holds it to, and the history is public.
* `tooling/CLAUDE.md` carries the decisions a contributor meets, so the ones that matter daily are on a page.
* A pull request body carries the evidence behind the change, and the branch rule means every change has one.
* [dev-one-maintainer] is the reason this has cost nothing so far, and the reason it would cost a lot on the day a
  second person arrives.

## How it closes

This corpus adopts `adrs`, and the decisions worth citing move into it: which ones a record needs to point at, rather
than all of them. This record closes on that.

## Scope

Every decision about `kac`, the schema, the documentation site or the workflows.

## Related

* [dev-one-maintainer] is why nobody has needed this yet.
* [std-PROSE] holds a commit message to saying why.

[dev-one-maintainer]: one-maintainer.md
[std-PROSE]: ../standards/prose.md
