---
id: dev-generated-output-ungoverned
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-DERV.RUNLOG
risk: low
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ derived-data, generate, verification ]
---

# A generated file keeps no record of the run that wrote it

`Deviation: dev-generated-output-ungoverned` `ACTIVE`

`kac generate` and `kac export` compute derived data, and a run's check result lives only in a log that expires.

## What we are doing instead

`kac generate --check` regenerates every block and index and compares the result against what is committed.
[ctl-corpus-validation] runs it on each pull request, and the workflow log records the run and its result. That log
lives in GitHub Actions and expires there, and nothing keeps the result for longer.

## Why we need it

Nothing here has needed a run's result after its log expired. Keeping each result with the release it produced needs
a step in the publish workflows and somewhere to put the output, and neither exists.

## What the risk is

A reader doubts a generated block or a published export after the run's log has expired, and nothing shows the run
that produced it. Answering the doubt means checking out the commit and running the generator again.

## What compensates

* `generate --check` fails the branch where any generated file is stale, so a stale file cannot land on `main`.
* The golden fixtures run `generate` over a corpus and compare the result, so the check has a test behind it.
* The generated block markers state the generator, so a reader can tell which command wrote the text.
* Every input is a committed file in the same repository, so the run's inputs are recoverable from the commit it ran
  on.

## How it closes

A run that publishes keeps its inputs, its output and its check result with the release it produced, and a standard
states that it has to. This record closes on that standard.

## Scope

Every block between `BEGIN GENERATED` and `END GENERATED`, every `_index.md`, and the export `kac export` writes.

## Related

* [ctl-corpus-validation] runs the check that fails a stale file.

[ctl-corpus-validation]: ../controls/corpus-validation.md
