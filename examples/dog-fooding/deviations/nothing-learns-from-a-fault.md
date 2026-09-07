---
id: dev-nothing-learns-from-a-fault
tier: normative
status: active
departs-from:
  - eng:pol-INCR.ACTIONS
  - eng:pol-INCR.EVIDENC
  - eng:pol-INCR.LEARN
  - eng:pol-INCR.TOOSOON
  - eng:pol-SECD.ACTIONS
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
owner: paul.law
tags: [ incidents, learning, postmortems ]
---

# A fix lands and nothing asks what allowed the fault

`Deviation: dev-nothing-learns-from-a-fault` `ACTIVE`

This corpus adopted no type that holds an incident record, so nothing keeps evidence, nothing asks what let the fault
through, and nothing tracks what was agreed afterwards.

## What we are doing instead

A defect is fixed and the fix keeps the test that catches it, which `eng:std-GATES` asks for. The reasoning behind the
fix lands in the commit message. Nothing else survives.

There is no place to record what happened, what it cost, or what would stop it happening again. An action agreed in a
pull request review is agreed in a comment thread, and closes when somebody remembers it.

## Why we need it

The `postmortems` type exists in the framework and this corpus declined it, because nothing here has run an incident
worth one. Adopting a type to hold records nobody has written is scaffolding, and this repository has deleted invented
records before.

## What compensates

* A fixed defect keeps the test that catches it, so the same fault does not return unnoticed.
* Every fix is a pull request, and the commit message says why, so the reasoning is recoverable from git.
* The issue tracker holds agreed work, and an issue closes on the day it lands.
* [dev-decisions-live-in-commits] records the same weakness for a decision rather than for a fault.

## How it closes

Adopting `postmortems` gives an incident somewhere to go, and a standard says when one is written and what it holds. A
security action gets the same treatment: an issue on the tracker, labelled, with an owner.

The honest trigger is the first incident worth writing up. This record closes when that happens and a record is
written, or at the review date when the answer is still that nothing has.

## Scope

Every fault in `kac`, in the documentation site, in the workflows or in the plugin, and every action agreed after one.

## Related

* `eng:pol-INCR.ACTIONS`, `eng:pol-INCR.EVIDENC`, `eng:pol-INCR.LEARN`, `eng:pol-INCR.TOOSOON` and
  `eng:pol-SECD.ACTIONS` are the clauses this departs from.
* [dev-no-incident-process] carries what happens before the fix.
* [dev-decisions-live-in-commits] is the same weakness for a decision.

[dev-decisions-live-in-commits]: decisions-live-in-commits.md
[dev-no-incident-process]: no-incident-process.md
