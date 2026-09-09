---
id: dev-branch-habits-unstated
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-AUTV.BROKEN
  - eng:pol-AUTV.OFTEN
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
owner: human:paul.law
tags: [ branches, habits, verification ]
---

# Branch size and a red `main` are habits here

`Deviation: dev-branch-habits-unstated` `ACTIVE`

Nothing states that a red `main` comes before other work, and nothing states how big a branch is allowed to get.

## What we are doing instead

Branches here are small and short-lived because one person works one thing at a time. A red `main` gets fixed on
sight, for the same reason. Both are what the maintainer does rather than what a record asks for.

[std-CI] holds the gate: every job runs on a pull request into `main`, and the branch rule names `validate` as the
check a merge waits for. That stops a broken change landing. It says nothing about what happens once one has.

## Why we need it

A rule about branch size needs a number, and nothing here has measured one. A rule about a red `main` needs somebody
other than its author to notice, and there is nobody else. Writing either without the measurement or the second person
puts a clause in a standard that no reviewer could fail a change against.

## What compensates

* The merge gate refuses a change that breaks a check, so `main` goes red from a merge conflict or a flake rather than
  from an untested change.
* ctl-0001 names the branch rule, and says plainly that nothing in CI reads it.
* One maintainer means one branch in flight, so a long-running branch has nothing to diverge from.

## How it closes

Somebody measures how long a branch here actually lives and how large it gets, and writes the rule that matches. The
red-`main` clause closes when a standard says what stops when the gate is red, and ctl-0001 or a new control checks it.

## Scope

Every branch of this repository, and `main` itself.

## Related

* [dev-one-maintainer] is why there is nobody else to notice a red `main`.
* [std-CI] holds the gate those two clauses sit beside.

[dev-one-maintainer]: one-maintainer.md
[std-CI]: ../standards/workflows.md
