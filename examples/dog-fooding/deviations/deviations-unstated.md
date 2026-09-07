---
id: dev-deviations-unstated
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
applies-to:
owner: paul.law
tags: [ deviations, governance, register ]
---

# No standard here says how a deviation is handled

`Deviation: dev-deviations-unstated` `ACTIVE`

This folder is the register, and nothing states what has to be in a record, who closes one, or what happens when a
review date passes.

## What we are doing instead

Every departure this corpus knows about is written here, with an owner and a review date, and the schema holds each
record to the sections the type declares. That much is the type's doing rather than a standard's.

Nothing here says a deviation is written before the departure. Nothing sweeps a review date that has passed. Nothing
tests a long-standing habit against the policy it breaks, which is exactly how this register came to be written in one
sitting rather than one departure at a time.

## Why we need it

The type arrived first and the register was written against a coverage map, so the records exist before the rule that
governs them. Writing the standard at the same time would have stated a process nobody had run yet.

## What compensates

* The schema requires an owner, a review date and four sections, and `kac validate` fails a record missing any of them.
* `review-after-acceptance` fails a record that would expire as it was written.
* The register is a folder in a public repository, so anybody reading the corpus meets it.
* The `expiry` rule is declared and does not run. A passed review date is caught by somebody reading the index, and the
  index sorts on `review-by`.

## How it closes

A standard states what this repository owes a deviation: written before the departure where there was time, what the
record has to say, who may accept the risk, and what closing means. It cites the `expiry` rule once that runs.

## Scope

Every record in this folder, and every departure this repository takes from a clause `../engineering/` states.

## Related

* `eng:pol-DEVI.CLOSE`, `eng:pol-DEVI.CONTENT`, `eng:pol-DEVI.CUSTOM`, `eng:pol-DEVI.DEBT`, `eng:pol-DEVI.PERM`,
  `eng:pol-DEVI.RECORD` and `eng:pol-DEVI.SURFACE` are the clauses this departs from.
