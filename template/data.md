# Data

Where data lives, how long we keep it, and how sensitive it is.

**[→ Index](data/_index.md)**

## What is a data document?

One document per data domain. It names the entities in the domain, the [service](services.md) that owns them, and the
stores they live in. The rest of the record says how sensitive they are, how long we keep them, and where they flow.

## Why we use them

Two readers arrive with different questions. Someone building a feature wants to know where bookings actually live and
which service owns them. Ask around and the answers disagree. Someone answering an auditor wants to know what personal
data we hold, why we hold it and how long we keep it. Nobody should have to read a database schema to answer that. A
data document answers the first question the same way every time, and gives the [policy](policies.md) tier its
evidence for the second.

An author filling in `owned-by` also finds the entities that two services both believe they own. That disagreement is a
design problem worth finding on paper.

## Scope

A data document is **descriptive**: it mirrors what is actually stored. We organise data documents by data domain rather
than by store, because a domain often spans stores and that spread is the part worth seeing.

Not the place for:

* **Schema definitions.** Those live with the code that owns them.
* **How to query the data.** That is a [process](processes.md) or the service's own document.
* **Retention rules as commitments.** A [policy](policies.md) holds the commitment, and a data document records what the
  store actually does.

The folder is singular, `data/`, because English gives no plural. It and the [glossary](glossary.md) are the two
exceptions to the plural-folder rule.

## Level of detail and cadence

One record describes one data domain: the entities in it, the service that owns them, how sensitive they are, how long
they are kept and where they flow. The table definitions belong in the owning service's repository.

A record changes when the domain's retention, classification or flows change, which is rare. New records arrive with
each new domain.

## Metadata

<!-- BEGIN GENERATED: schema-data -->

| Field              | Value                                | Notes                                                                                                                                                  |
|--------------------|--------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `tier` *†          | `descriptive`                        | The record's trust level, fixed for the type and checked against the folder.                                                                           |
| `status` *†        | `active` `deprecated`                | Whether this data is current or deprecated. In force at `active`.                                                                                      |
| `owned-by` *       | id                                   | Id of the service that owns this data.                                                                                                                 |
| `classification` * | `public` `internal` `confidential`   | How widely the data may be shared, from published to need-to-know.                                                                                     |
| `personal-data` *  | `none` `personal` `special-category` | Whether the data identifies a living person, and whether GDPR Article 9 applies to it.                                                                 |
| `data-subjects`    | list                                 | Categories of people the data is about, such as `borrowers` or `staff`. Required when `personal-data != none`.                                         |
| `retention`        | string                               | How long the data is kept in practice. Where that differs from the policy, record both. Required when `personal-data in [personal, special-category]`. |
| `region` *         | string                               | Where the data is stored and processed, as a cloud region or a place.                                                                                  |
| `flows-to`         | list                                 | Ids of the services and integrations this data is sent to.                                                                                             |
| `review-by` *      | date                                 | Quoted. The date by which someone confirms the classification and the retention still hold.                                                            |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-data -->

## Adding a data document

Copy [`_template.md`](data/_template.md). Its guidance block states how to write the id and each field. The prompt under
each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-data -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                    | Level   | What it verifies                                                                                       |
|--------------------------|---------|--------------------------------------------------------------------------------------------------------|
| `ref-resolves`           | error   | An id in a field that references another document names one that exists, of the type the field names.  |
| `no-actual-data`         | error   | Fails on an email address outside `example.com`. Nothing catches an identifier or a connection string. |
| `classification-in-date` | warning | A document names a review date that has not passed.                                                    |

<!-- END GENERATED: checks-data -->
