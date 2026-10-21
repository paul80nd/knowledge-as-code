# Explanations

How the platform works, and why it is shaped the way it is.

**[→ Index](explanations/_index.md)**

## What is an explanation?

An explanation is prose whose job is comprehension. An architecture overview, a walkthrough of how a request flows
through the estate, an account of why the testing approach is shaped as it is. You read one to build a mental model, not
to consult it mid-task.

Every other type answers a narrow question: what was decided, what you must do, how you do it, what a component is. An
explanation answers how those pieces fit together. A service page or an ADR that took that question on would turn into
one.

## Why we use them

Narrative that has no home either goes unwritten or piles up at the top level of the corpus, with no owner and no review
date. That is how a page goes stale. The type gives every explanation an owner and a `review-by`, like every other
record.

## Scope

An explanation is **not**:

* **Normative.** If it says what you must do, it is a [standard](standards.md).
* **Procedural.** If it says how to perform a task, it is a [process](processes.md) or a [runbook](runbooks.md).
* **A catalogue entry.** If it describes one component, it is a [service](services.md).
* **A decision.** If it records what was chosen and why, it is an [ADR](adrs.md).

**Explanations link rather than restate.** An architecture overview points at the services, offerings, standards,
processes and ADRs with the detail. An explanation that states a fact those documents already own is the second copy of
it. That copy is the one that goes stale first.

This type is the residual, and a residual with a low bar fills up with whatever fits nowhere else. If a document could
plausibly be an explanation *or* something else, it is the something else.

## Level of detail and cadence

One explanation covers one subject, and each section explains one idea. It gives the reader the reasoning and the
trade-offs, and links to the records that state the detail. Steps to follow belong in a process, and rules in a
standard.

An explanation changes when the thing it explains changes. New explanations arrive rarely.

## Metadata

<!-- BEGIN GENERATED: schema-explanations -->

| Field         | Value                    | Notes                                                                                                             |
|---------------|--------------------------|-------------------------------------------------------------------------------------------------------------------|
| `tier` *†     | `descriptive`            | The record's trust level, fixed for the type and checked against the folder.                                      |
| `status` *†   | `draft` `active` `stale` | `draft` while it is written, `active` once it is right, `stale` where it has fallen behind. In force at `active`. |
| `explains` *  | list                     | Ids of the services, offerings, standards, processes and ADRs this explains.                                      |
| `review-by` * | date                     | Quoted. The date by which someone confirms this is still true.                                                    |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-explanations -->

## Adding an explanation

Check it is none of the exclusions above. Then copy [`_template.md`](explanations/_template.md). Its guidance block
states how to write the id and each field. The prompt under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-explanations -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                      |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `links-rather-than-restates` | warning | An explanation's prose stays proportionate to the links it makes.                                     |
| `not-normative`              | warning | No bold RFC 2119 keyword (MUST, MUST NOT, SHOULD, SHOULD NOT, MAY) binds from an explanation.         |
| `explanation-in-date`        | warning | An explanation names a review date that has not passed.                                               |

<!-- END GENERATED: checks-explanations -->
