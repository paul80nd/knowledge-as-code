# Standards

Fixture scaffolding. A stood-up type needs a page beside its folder, so this stands in for the real one. It is checked
as any type page is: its links, both pairs of generated markers, and carrying no frontmatter of its own.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field          | Value                                      | Notes                                                                               |
|----------------|--------------------------------------------|-------------------------------------------------------------------------------------|
| `id` *†        | string                                     | Stable, unique across the corpus, never reused. Format set by the type.             |
| `tier` *†      | `normative`                                | Fixed for the type. A trust signal for the reader. CI checks it matches the folder. |
| `status` *†    | `draft` `active` `deprecated` `superseded` | Plain values only. Enforcement notes belong in `verified-by`.                       |
| `owner` *†     | string                                     | A named person, never a team alias.                                                 |
| `tags` †       | list                                       | Free-form, lowercase, hyphenated. Used for cross-cutting search.                    |
| `category`     | derived from the record's sub-path         | The folder the standard is filed under, below `standards/`.                         |
| `derived-from` | list                                       | The ADRs this standard distils. Provenance may come from `implements` instead.      |
| `implements`   | list                                       | Policy clause ids this standard puts into practice, as `pol-EVER.BRANCH`.           |
| `verified-by`  | list                                       | Control ids that check it.                                                          |
| `applies-to` * | list                                       | Service ids, or `all`.                                                              |
| `review-by` *  | date                                       | Quoted. The date by which someone confirms this is still true.                      |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-standards -->

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `derived-key`               | error   | A field derived from the record's folder is not written in frontmatter.                                         |
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
| `filename / slug-length`    | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `provenance-required`       | error   | A standard cites an ADR in `derived-from`, a policy clause in `implements`, or both.                            |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                           | What it would verify                                                                                       |
|--------------------------------|------------------------------------------------------------------------------------------------------------|
| `rules-have-controls`          | Every MUST / MUST NOT rule is claimed by a control, or the standard declares the gap explicitly.           |
| `changelog-begins-at-active`   | Changelog entries are material changes only, and begin when status becomes `active`.                       |
| `changelog-on-material-change` | If the Rules section changed and status is `active`, a new changelog entry is required in the same commit. |

<!-- END GENERATED: checks-standards -->
