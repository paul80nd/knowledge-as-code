# Offerings

What the product offers its customers, and why.

**[→ Index](offerings/_index.md)**

## What is an offering?

One document per customer-visible surface of the product. It records what the surface does, why it exists, which
services implement it, and where the detail lives.

An offering is a **hub**. It links to the work items that specify it, the feature files that test it, the services
that implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in this estate's GitHub issues. An issue tracker has nothing above them: no account of what
the product offers a customer and why. It could not keep one without disturbing a backlog that exists to run delivery.

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
Acceptance criteria go in the issue.

Related but different:

* **Spec**: how standards apply to one concrete contract. It lives in the repository that owns the feature, beside its
  OpenAPI document and feature files. ADRs split the same way: cross-repo synthesis here, feature-level
  detail with the code.
* **[Service](services.md)**: a thing we deploy. An offering is a thing a customer gets.
* **Explanation**: how something works internally. An offering is what it does externally.

## Level of detail and cadence

One record describes one thing you offer a customer: who it is for, why it exists, and where they reach it. It links
to what implements, tests and constrains it, and the detail of each belongs in the record it links to.

An offering changes when the product strategy changes, which is rare. New offerings arrive rarely.

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

1. Copy [`_template.md`](offerings/_template.md) to `<slug>.md`. Offering ids are slugs: `ofr-<name>`.
2. Write the *what*, the *who* and the *why* in prose. Two or three paragraphs is usually enough.
3. Fill in `implemented-by` and `feature-files`, and link the work items from the list. Those links make it a hub.
4. Do not explain how it works. Link to the services and explanations that already do.

**Conventions**

* **The consumer group is what makes this one offering.** `Who it is for` names that group. Two groups served in two
  different ways are two offerings, however much they share underneath.
* **Hub, not specification.** `hub-not-specification` weighs the whole document against its outbound links, at roughly
  forty words each, so an offering that grows a section of its own trips it. Where a section runs longer than the links
  around it, ask whether the detail belongs in a work item.
* **The list and the frontmatter say the same thing, and CI checks it.** `related-matches-section` reconciles
  `implemented-by` and `nfrs` against the ids the list names, in both directions. Write an id in one place and you
  write it in both.
* **Work items live in the list alone.** There is no field for them. Label an issue `gh#2101`. A consumer reading an
  export sees that label and not the address, because a section travels and its link definitions do not.
* **Keep the rest of a feature file path honest yourself.** `feature-file-repo` checks the repository the path opens
  with. Past that first segment the field is a plain string, `ref-resolves` never sees it, and `feature-file-orphans`
  is declared and does not run. A path that goes stale there goes stale quietly.

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
