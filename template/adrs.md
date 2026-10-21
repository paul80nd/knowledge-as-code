# Architecture Decision Records (ADRs)

A log of the architecturally significant decisions we have made about the platform.

**[→ ADRs index](adrs/_index.md)**

## What is an ADR?

An Architecture Decision Record (ADR) is a short Markdown document holding one architecturally significant decision. It
carries the context that led to the decision, the decision itself, the alternatives we weighed, and the consequences for
everyone working downstream of it.

ADRs are **immutable once accepted**. When a decision changes, we write a new ADR that supersedes the old one and leave
the old one where it is. A reader then has the reasoning behind the architecture we run today and the reasoning behind
the decisions it replaced.

## Why we use them

The platform spans many repositories, and many of our architectural decisions land in more than one of them: message
contracts, the service bus, auth, deployment ordering, infrastructure patterns. Without a durable record:

* The same questions resurface every few months ("why aren't we using Event Grid?", "why is the migration runner
  separate?") because nobody can point at the answer.
* A new contributor reads the code and learns the *shape* of the architecture, but not the *reasoning* behind it.
* An engineer working inside one repository drifts away from the decisions that cross several, because nothing in that
  repository carries the reasoning.

## Scope: central vs repo-local

An ADR here covers a decision spanning **more than one** repository. A decision entirely local to a single repository (a
library choice, an internal naming convention, a refactor approach) belongs in that repository, under `/docs/adrs/`.

Where a central ADR later supersedes a repo-local one, set the local ADR to superseded and reference the central ADR by
its id.

## Level of detail and cadence

One ADR records one decision, in about a page. Decision states the choice in a sentence or two, and Consequences say
what it commits you to. How to carry the decision out belongs in a standard or a process that cites the ADR.

An accepted ADR changes only its status, when a later ADR supersedes it. New ADRs arrive a few times a year, when a
decision affects more than one repository.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Value                                                      | Notes                                                                             |
|-----------------|------------------------------------------------------------|-----------------------------------------------------------------------------------|
| `tier` *†       | `decided`                                                  | The record's trust level, fixed for the type and checked against the folder.      |
| `status` *†     | `proposed` `rejected` `accepted` `deprecated` `superseded` | Where the decision stands. In force at `accepted`.                                |
| `decided-on`    | date                                                       | The day the decision was settled. Required when `status in [accepted, rejected]`. |
| `supersedes`    | id                                                         | The ADR this one replaces.                                                        |
| `superseded-by` | id                                                         | The ADR that replaces this one. Required when `status == superseded`.             |
| `deciders`      | list                                                       | The people who agreed it, each as `human:alex.doe`.                               |
| `related`       | list                                                       | Other ADRs worth reading beside this one, matching the `## Related` section.      |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-adrs -->

## Adding an ADR

Copy [`_template.md`](adrs/_template.md). Its guidance block states how to write the id, the filename, each field and
the form. It also says when you may edit a decided ADR. The prompt under each section states how to write that section.
Open a pull request with the status at `proposed`.

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                      |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `related-matches-section`    | error   | A field that mirrors a section reconciles with the ids in that section.                               |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`                 | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `y-statement`                | warning | A Y-statement block-quote follows the H1, states all six moves, and is within its word ceiling.       |
| `alternatives-verdict`       | warning | Each Alternatives Considered bullet states a verdict.                                                 |
| `decision-cites-descriptive` | warning | No link in Decision or Consequences lands on a record of a descriptive type.                          |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                       | What it would verify                                                                                                |
|----------------------------|---------------------------------------------------------------------------------------------------------------------|
| `immutable-after-accepted` | Once status is `accepted` or `rejected`, the decision changes only by a superseding ADR, never by an edit in place. |

<!-- END GENERATED: checks-adrs -->
