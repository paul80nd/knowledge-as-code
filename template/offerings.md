# Offerings

What the product offers its customers, and why.

**[→ Index](offerings/_index.md)**

## What is an offering?

One document per customer-visible surface of the product. It records what the surface does, why it exists, which
services implement it, and where the detail lives.

An offering is a **hub**. It links to the work items that specify it, the feature files that test it, the services
that implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in the work items your tracker keeps. A tracker has nothing above them: no account of what the
product offers a customer and why. It could not keep one without disturbing a hierarchy that exists to run delivery.

Without that account, nobody can answer "what does the product do?" from one place, and everyone starting a significant
piece of work rebuilds the same context.

## Scope

One document per **customer-visible surface**, not per work item and not per service. One offering typically spans
several services. One service often contributes to several offerings.

**An offering whose `implemented-by` names one service is a synonym for that service.** Write the service record
instead, and `spans-more-than-one-service` warns where you do not. This type was first tried over an estate of four
services where every offering mapped onto one of them, and each record restated the service beside it.

**Offerings link rather than restate.** An offering that specifies behaviour has begun to drift from the work items
it should point at. The next session to read it will trust it anyway, which makes a drifted offering worse than none.
Acceptance criteria go in the tracker.

Related but different:

* **Spec**: how standards apply to one concrete contract. It lives in the repository that owns the feature, beside its
  OpenAPI document and feature files. [ADRs](adrs.md) split the same way: cross-repo synthesis here, feature-level
  detail with the code.
* **[Service](services.md)**: a thing we deploy. An offering is a thing a customer gets.
* **[Explanation](explanations.md)**: how something works internally. An offering is what it does externally.

## Level of detail and cadence

One record describes one thing you offer a customer: who it is for, why it exists, and where they reach it. It links to
what implements, tests and constrains it, and the detail of each belongs in the record it links to.

An offering changes when the product strategy changes. New offerings arrive rarely.

## Metadata

<!-- BEGIN GENERATED: schema-offerings -->

| Field              | Value                                    | Notes                                                                                                      |
|--------------------|------------------------------------------|------------------------------------------------------------------------------------------------------------|
| `tier` *†          | `descriptive`                            | The record's trust level, fixed for the type and checked against the folder.                               |
| `status` *†        | `planned` `building` `live` `deprecated` | Lifecycle of the offering itself, which may differ from the lifecycle of its services. In force at `live`. |
| `implemented-by` * | list                                     | Ids of the services that implement this offering.                                                          |
| `feature-files`    | list                                     | Paths to the feature files that test this offering, each one beginning with its repository.                |
| `nfrs`             | list                                     | Ids of the NFRs this offering must meet. Required when `status == live`.                                   |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-offerings -->

## Adding an offering

Copy [`_template.md`](offerings/_template.md). Its guidance block states how to write the id, each field, the work items
and the NFRs. The prompt under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-offerings -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                         | Level   | What it verifies                                                                                      |
|-------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `related-matches-section`     | error   | A field that mirrors a section reconciles with the ids in that section.                               |
| `ref-resolves`                | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`                  | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `feature-file-repo`           | warning | A feature file path begins with a repository one of the implementing services names.                  |
| `hub-not-specification`       | warning | An offering's prose stays proportionate to the links it makes.                                        |
| `spans-more-than-one-service` | warning | An offering is delivered by more than one service.                                                    |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                   | What it would verify                                                                                               |
|------------------------|--------------------------------------------------------------------------------------------------------------------|
| `feature-file-orphans` | Scheduled. Reports feature files in the code repositories no offering claims, and paths here that no longer exist. |

<!-- END GENERATED: checks-offerings -->
