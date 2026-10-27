# Standards

Fixture scaffolding. A stood-up type needs a page beside its folder, so this stands in for the real one. It is checked
as any type page is: its links, both pairs of generated markers, and carrying no frontmatter of its own.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field           | Value                                      | Notes                                                                               |
|-----------------|--------------------------------------------|-------------------------------------------------------------------------------------|
| `tier` *†       | `normative`                                | The record's trust level, fixed for the type and checked against the folder.        |
| `status` *†     | `draft` `active` `deprecated` `superseded` | Plain values only. Enforcement notes belong in `verified-by`. In force at `active`. |
| `supersedes`    | id                                         | The standard this one replaces.                                                     |
| `superseded-by` | id                                         | The standard that replaces this one. Required when `status == superseded`.          |
| `category`      | derived from the record's sub-path         | The folder the standard is filed under, below `standards/`.                         |
| `derived-from`  | list                                       | The ADRs this standard distils. Provenance may come from `implements` instead.      |
| `implements`    | list                                       | Policy clause ids this standard puts into practice, as `pol-EVER.BRANCH`.           |
| `depends-on`    | list                                       | Standards whose rules this one's rules depend on.                                   |
| `verified-by`   | list                                       | Control ids that check it.                                                          |
| `applies-to` *  | list                                       | Service ids, or `all`.                                                              |
| `review-by` *   | date                                       | Quoted. The date by which someone confirms this is still true.                      |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-standards -->

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                       | Level   | What it verifies                                                                                            |
|-----------------------------|---------|-------------------------------------------------------------------------------------------------------------|
| `derived-key`               | error   | A field derived from the record's folder is not written in frontmatter.                                     |
| `part-none / part-empty`    | error   | The parts section holds at least one heading, and each has something under it.                              |
| `part-id-format`            | error   | Each part heading opens with its id, as `### PERMS: A job declares the permission it needs`.                |
| `part-modal / part-nested`  | error   | A part heading states its obligations as bullets, each with a declared modal in bold and no list inside it. |
| `modal-outside-rules`       | warning | Only the parts section binds: no bullet outside it names a declared modal.                                  |
| `part-id-unique / part-ref` | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.       |
| `mirrors-citations`         | error   | A field that mirrors a label reconciles with the citations the labelled lines gather.                       |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.       |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                            |
| `rule-id-kept`              | error   | With `--since`, a standard active at that revision keeps the id of every rule it had there.                 |
| `changelog-order`           | warning | A standard's changelog entries read newest first.                                                           |
| `provenance-required`       | error   | A standard cites an ADR in `derived-from`, a policy clause in `implements`, or both.                        |
| `standard-in-date`          | warning | A standard names a review date that has not passed.                                                         |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                  | What it would verify                                                                             |
|-----------------------|--------------------------------------------------------------------------------------------------|
| `rules-have-controls` | Every MUST and MUST NOT rule has a control that checks it, or the standard says which have none. |

<!-- END GENERATED: checks-standards -->
