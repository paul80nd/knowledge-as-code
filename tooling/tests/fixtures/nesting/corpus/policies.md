# Policies

Fixture scaffolding. A stood-up type needs a page beside its folder, so this stands in for the real one. It is checked
as any type page is: its links, both pairs of generated markers, and carrying no frontmatter of its own.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field         | Value                              | Notes                                                                                                                            |
|---------------|------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| `tier` *†     | `normative`                        | The record's trust level, fixed for the type and checked against the folder.                                                     |
| `status` *†   | `draft` `active` `retired`         | `draft` while it is being agreed, `retired` once it is stood down. Retire a policy instead of deleting it. In force at `active`. |
| `category`    | derived from the record's sub-path | The folder the policy is filed under, below `policies/`.                                                                         |
| `aligns-with` | list                               | The binding frameworks this policy's clauses map to, grouped with the references they cite.                                      |
| `active-from` | date                               | The day the policy came into force. Required when `status == active`.                                                            |
| `review-by` * | date                               | The day the policy is looked at again, usually a year ahead.                                                                     |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-policies -->

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                                  | Level   | What it verifies                                                                                                    |
|----------------------------------------|---------|---------------------------------------------------------------------------------------------------------------------|
| `derived-key`                          | error   | A field derived from the record's folder is not written in frontmatter.                                             |
| `entry-shape / entry-key`              | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.               |
| `clauses`                              | error   | The clause section is a table of `Id \| Clause` rows, each id a code span and each clause opening with its modal.   |
| `clause-order / clause-compound`       | warning | Clause rows are grouped by binding level, and no row states a second modal.                                         |
| `part-id-unique / part-ref`            | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.               |
| `pair-one-way`                         | error   | A clause signposted by a clause of the same id in another policy signposts that clause back.                        |
| `shared-id-unsignposted`               | warning | A clause id used in two policies is signposted from at least one of them.                                           |
| `alignment-rollup / framework-posture` | error   | `aligns-with` carries every binding reference the `Alignment` column cites, and the register places each framework. |
| `alignment-unlinked`                   | error   | Every framework an `Alignment` cell cites is written as a link the corpus resolves.                                 |
| `framework-uncited`                    | error   | Every framework on the register is cited by at least one clause.                                                    |
| `clause-id-kept`                       | error   | With `--since`, a policy active at that revision keeps the id of every clause it had there.                         |
| `review-not-before-active`             | error   | `review-by` is at or after `active-from`.                                                                           |
| `posture-belongs-to-frameworks`        | warning | "compliant", "certified" or "registered" written near a framework reference.                                        |
| `policy-in-date`                       | warning | A policy names a review date that has not passed.                                                                   |

<!-- END GENERATED: checks-policies -->
