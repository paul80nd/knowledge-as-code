# Architecture Decision Records

This is a fixture type page. `generate` regenerates the two blocks below from the schema, and rebuilds
`adrs/_index.md` from the frontmatter of the ADRs in this corpus. Everything outside the markers is
byte-preserved.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Value                                           | Notes                                                                                                        |
|-----------------|-------------------------------------------------|--------------------------------------------------------------------------------------------------------------|
| `id` *†         | string                                          | Stable, unique across the corpus, never reused. Format set by the type.                                      |
| `tier` *†       | `decided`                                       | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.                         |
| `status` *†     | `proposed` `accepted` `deprecated` `superseded` | Immutable once `accepted` — supersede rather than rewrite.                                                   |
| `owner` *†      | string                                          | A named person, never a team alias.                                                                          |
| `tags` †        | list                                            | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                             |
| `decided-on`    | date                                            | The acceptance date. Bare key until accepted. Required when `status == accepted`.                            |
| `supersedes`    | id                                              | The ADR this replaces.                                                                                       |
| `superseded-by` | id                                              | CI enforces both directions; a one-sided supersession fails the build. Required when `status == superseded`. |
| `deciders`      | list                                            | The people who agreed it.                                                                                    |
| `related`       | list                                            | Must match the ids named in the `## Related` section. CI reconciles the two, case-insensitively.             |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-adrs -->

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `related-matches-section`   | error   | A field that mirrors a section reconciles with the ids in that section.                                         |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `y-statement`               | warning | A Y-statement block-quote follows the H1, states all six moves, and is within its word ceiling.                 |
| `alternatives-verdict`      | warning | Each Alternatives Considered bullet states a verdict.                                                           |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                       | What it would verify                                                                             |
|----------------------------|--------------------------------------------------------------------------------------------------|
| `immutable-after-accepted` | Once status is `accepted`, only typo fixes, link corrections and status transitions are allowed. |

<!-- END GENERATED: checks-adrs -->
