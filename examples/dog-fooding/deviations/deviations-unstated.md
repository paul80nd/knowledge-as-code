---
id: dev-deviations-unstated
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-DEVI.CLOSE
  - eng:pol-DEVI.CONTENT
  - eng:pol-DEVI.CUSTOM
  - eng:pol-DEVI.DEBT
  - eng:pol-DEVI.PERM
  - eng:pol-DEVI.RECORD
  - eng:pol-DEVI.SURFACE
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
owner: human:paul.law
tags: [ deviations, governance, register ]
---

# No standard here says how a deviation is handled

`Deviation: dev-deviations-unstated` `ACTIVE`

This folder is the register, and nothing states what has to be in a record, who closes one, or what happens when a
review date passes.

## What we are doing instead

Every departure this corpus knows about is written here, with an owner and a review date. The schema requires each
record to have the sections the type declares. The type gives that much, and no standard here does.

Nothing here says a deviation is written before the departure. Nothing tests a long-standing habit against the policy
it breaks. That is why the whole register was written in one sitting.

## Why we need it

The type arrived first and the register was written against a coverage map, so the records exist before the rule that
governs them. Writing the standard at the same time would have stated a process nobody had run yet.

## What compensates

* The schema requires an owner, a review date and four sections, and `kac validate` fails a record missing any of them.
* `review-after-acceptance` fails a record that would expire as it was written.
* `expiry` warns on an active record whose `review-by` has gone by, so every run reports a review nobody has done.
* The register is a folder in a public repository, so anybody reading the corpus finds it.

## How it closes

A standard states what a deviation here has to say, who may accept the risk, and what closing means. It requires the
record to be written before the departure, where there was time. It cites `expiry` as the check behind the review
date.

## Scope

Every record in this folder, and every departure this repository takes from a clause `../engineering/` states.
