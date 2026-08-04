# Architecture Decision Records

This is a fixture type page. `index` regenerates the two blocks below from the schema, and rebuilds
`adrs/INDEX.md` from the frontmatter of the ADRs in this corpus. Everything outside the markers is
byte-preserved.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Req | Type | Notes                                                                                              |
| --------------- | --- | ---- | -------------------------------------------------------------------------------------------------- |
| `status`        | ●   | enum | `proposed` · `accepted` · `deprecated` · `superseded`                                              |
| `decided-on`    |     | date | The acceptance date. Bare key until accepted. Required once `accepted`.                            |
| `supersedes`    |     | id   | The ADR this replaces.                                                                             |
| `superseded-by` |     | id   | CI enforces both directions; a one-sided supersession fails the build. Required once `superseded`. |
| `deciders`      |     | list | The people who agreed it.                                                                          |
| `related`       |     | list | Must match the ids named in the `## Related` section. CI reconciles the two, case-insensitively.   |

<!-- END GENERATED: schema-adrs -->

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

| Check                       | Level   | What it verifies                                                                  |
| --------------------------- | ------- | --------------------------------------------------------------------------------- |
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                               |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                    |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                 |
| `required-field`            | error   | Required and conditionally-required fields are present.                           |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                    |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                              |
| `enum`                      | error   | Enum values are in range and lowercase.                                           |
| `field-pattern`             | error   | Values match the pattern their field declares (`tags`, `aligns-with`, `licence`). |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                        |
| `id`                        | error   | `id` has the type's prefix and width and matches the filename number.             |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                             |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                   |
| `h1`                        | error   | The H1 matches the title pattern and its number matches the `id`.                 |
| `required-section`          | error   | Every required section heading is present.                                        |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                    |
| `undefined-label`           | error   | Every `[ADR-NNNN]` shortcut reference has a link definition.                      |
| `related-matches-section`   | error   | `related` reconciles with the ids in the `## Related` section.                    |
| `reciprocal`                | error   | `supersedes` / `superseded-by` agree in both directions.                          |
| `unused-definition`         | warning | A link definition that nothing references.                                        |
| `y-statement`               | warning | A Y-statement block-quote follows the H1 and is within 60 words.                  |
| `alternatives-verdict`      | warning | Each Alternatives Considered bullet states a verdict.                             |

<!-- END GENERATED: checks-adrs -->
