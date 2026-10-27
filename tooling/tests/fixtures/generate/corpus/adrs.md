# Architecture Decision Records

This is a fixture type page. `generate` regenerates the two blocks below from the schema, and rebuilds
`adrs/_index.md` from the frontmatter of the ADRs in this corpus. Everything outside the markers is
byte-preserved.

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

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                                 |
|------------------------------|---------|------------------------------------------------------------------------------------------------------------------|
| `related-matches-section`    | error   | A field that mirrors a section reconciles with the ids in that section.                                          |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names.            |
| `reciprocal`                 | error   | A reciprocal field and its counterpart agree in both directions.                                                 |
| `y-statement`                | warning | A Y-statement block-quote follows the H1, states all six moves, and is within its word ceiling.                  |
| `alternatives-verdict`       | warning | Each Alternatives Considered bullet states a verdict.                                                            |
| `decision-cites-descriptive` | warning | No link in Decision or Consequences lands on a record of a descriptive type.                                     |
| `immutable-after-accepted`   | error   | With `--since`, an accepted or rejected ADR has changed by no more than a typo, a link, its status or `accepts`. |

<!-- END GENERATED: checks-adrs -->
