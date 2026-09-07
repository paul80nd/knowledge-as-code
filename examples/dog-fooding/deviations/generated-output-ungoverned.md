---
id: dev-generated-output-ungoverned
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-DERV.CHECK
  - eng:pol-DERV.EXPECT
  - eng:pol-DERV.FAILED
  - eng:pol-DERV.LINEAGE
  - eng:pol-DERV.RUNLOG
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-kac
owner: paul.law
tags: [ derived-data, generate, verification ]
---

# No standard governs what `kac generate` computes

`Deviation: dev-generated-output-ungoverned` `ACTIVE`

`kac generate` and `kac export` both compute derived data, and every clause about verifying derived data lands on
nothing here.

## What we are doing instead

`kac generate --check` regenerates every block and index and compares the result against what is committed. ctl-0007
runs it on each pull request, and a stale file fails the gate. That is an expectation stated and checked, and it is
implemented in the tool rather than written in a standard.

A generated block names the generator that wrote it and nothing else. It does not name the records it was computed
from, and no run keeps its inputs beside its output.

## Why we need it

The behaviour already exists and it is right. What is missing is the record saying it must stay right, which is work
whose only payoff is that somebody could cite it. Nothing has gone wrong here yet for want of that citation.

Lineage is the exception, and it is real work: a block would have to carry the ids it read, and every generator would
have to emit them.

## What compensates

* `generate --check` fails the branch where any generated file is stale, so drift never reaches `main`.
* The golden fixtures run `generate` over a corpus and compare the result, so the check has a test behind it.
* The generated block markers name the generator, so a reader can tell which command owns the text.
* Every input is a committed file in the same repository, so the run's inputs are recoverable from the commit it ran
  on.

## How it closes

A standard states what a generated file is, that it is regenerated and compared on every pull request, and that a stale
one is unusable rather than merely out of date. It cites ctl-0007. That closes four of the five clauses.

Lineage closes separately, when a generated block names the record ids it was computed from.

## Scope

Every block between `BEGIN GENERATED` and `END GENERATED`, every `_index.md`, and the export `kac export` writes.

## Related

* [ctl-0007] runs the check that four of them describe.

[ctl-0007]: ../controls/0007-corpus-validation.md
