# Architecture Decision Records

This is a fixture type page. `index` regenerates the two blocks below from the schema, and rebuilds
`adrs/INDEX.md` from the frontmatter of the ADRs in this corpus. Everything outside the markers is
byte-preserved.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Req | Type   | Notes                                                                                            |
|-----------------|-----|--------|--------------------------------------------------------------------------------------------------|
| `id` †          | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                            |
| `tier` †        | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.             |
| `status` †      | ●   | enum   | Immutable once `accepted` — supersede rather than rewrite.                                       |
| `owner` †       | ●   | string | A named person, never a team alias.                                                              |
| `tags` †        |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                 |
| `decided-on`    |     | date   | The acceptance date. Bare key until accepted.                                                    |
| `supersedes`    |     | id     | The ADR this replaces.                                                                           |
| `superseded-by` |     | id     | CI enforces both directions; a one-sided supersession fails the build.                           |
| `deciders`      |     | list   | The people who agreed it.                                                                        |
| `related`       |     | list   | Must match the ids named in the `## Related` section. CI reconciles the two, case-insensitively. |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `proposed` · `accepted` · `deprecated` · `superseded`               |

**Conditionally required**

| Field           | Required when          |
|-----------------|------------------------|
| `decided-on`    | `status == accepted`   |
| `superseded-by` | `status == superseded` |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-adrs -->

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

| Check                       | Level   | What it verifies                                                                       |
|-----------------------------|---------|----------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                    |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                         |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                      |
| `required-field`            | error   | Required and conditionally-required fields are present.                                |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                         |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                   |
| `enum`                      | error   | Enum values are in range and lowercase.                                                |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                           |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.             |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                             |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.          |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                  |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                        |
| `h1`                        | error   | The document has an H1 and, where the type declares one, it matches the title pattern. |
| `required-section`          | error   | Every required section heading is present.                                             |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                         |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                        |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.               |
| `related-matches-section`   | error   | A field that mirrors a section reconciles with the ids in that section.                |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                       |
| `unused-definition`         | warning | A link definition that nothing references.                                             |
| `y-statement`               | warning | A Y-statement block-quote follows the H1 and is within 60 words.                       |
| `alternatives-verdict`      | warning | Each Alternatives Considered bullet states a verdict.                                  |

<!-- END GENERATED: checks-adrs -->
