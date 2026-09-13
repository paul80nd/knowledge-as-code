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
owner: human:paul.law
tags: [ derived-data, generate, verification ]
---

# No standard governs what `kac generate` computes

`Deviation: dev-generated-output-ungoverned` `ACTIVE`

`kac generate` and `kac export` both compute derived data, and nothing here implements the clauses about verifying it.

## What we are doing instead

`kac generate --check` regenerates every block and index and compares the result against what is committed. [ctl-0007]
runs it on each pull request, and a stale file fails the gate. That is an expectation stated and checked. It lives in
the tool, and no standard states it.

A generated block states which generator wrote it, and nothing more. It does not list the records it was computed
from, and no run keeps its inputs beside its output.

## Why we need it

The behaviour already exists and it is right. The missing piece is a record saying it has to stay right, and writing
that record only gains somebody a citation. Nothing has gone wrong here yet for want of that citation.

Lineage is the exception, and it is real work: a block would have to list the ids it read, and every generator would
have to emit them.

## What compensates

* `generate --check` fails the branch where any generated file is stale, so a stale file cannot land on `main`.
* The golden fixtures run `generate` over a corpus and compare the result, so the check has a test behind it.
* The generated block markers state the generator, so a reader can tell which command wrote the text.
* Every input is a committed file in the same repository, so the run's inputs are recoverable from the commit it ran
  on.

## How it closes

A standard states what a generated file is, and that every pull request regenerates it and compares the result. It
states that a stale file is unusable, not merely out of date. It cites [ctl-0007]. That closes four of the five
clauses.

Lineage closes separately, when a generated block lists the record ids it was computed from.

## Scope

Every block between `BEGIN GENERATED` and `END GENERATED`, every `_index.md`, and the export `kac export` writes.

## Related

* [ctl-0007] runs the check four of those clauses describe.

[ctl-0007]: ../controls/0007-corpus-validation.md
