# Policies

Fixture scaffolding. A stood-up type needs a page beside its folder, so this stands in for the real one. It is checked
as any type page is: its links, both pairs of generated markers, and carrying no frontmatter of its own.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field         | Value                              | Notes                                                                                |
|---------------|------------------------------------|--------------------------------------------------------------------------------------|
| `id` *†       | string                             | Stable, unique across the corpus, never reused. Format set by the type.              |
| `tier` *†     | `normative`                        | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.  |
| `status` *†   | `draft` `active` `retired`         | `draft` until agreed. `retired` rather than deleted.                                 |
| `owner` *†    | string                             | A named person, never a team alias.                                                  |
| `tags` †      | list                               | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `category`    | derived from the record's sub-path | The folder the policy is filed under, below `policies/`.                             |
| `aligns-with` | list                               | The binding frameworks this policy's clauses map to, with the references they reach. |
| `review-by` * | date                               | Quoted. Annual is usually right for a policy.                                        |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-policies -->

## What CI checks

<!-- BEGIN GENERATED: checks-policies -->

| Check                                  | Level   | What it verifies                                                                                                    |
|----------------------------------------|---------|---------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`                   | error   | Frontmatter is present and is a valid YAML mapping.                                                                 |
| `unknown-key`                          | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                      |
| `derived-key`                          | error   | A field derived from the record's folder is not written in frontmatter.                                             |
| `key-order`                            | error   | Key order is a topological extension of the schema's field order.                                                   |
| `required-field`                       | error   | Required and conditionally-required fields are present.                                                             |
| `bare-key`                             | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                      |
| `date-quoted / date-format`            | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                              |
| `enum`                                 | error   | Enum values are in range and lowercase.                                                                             |
| `field-pattern`                        | error   | Values match the pattern their field declares (e.g. `tags`).                                                        |
| `list-order`                           | warning | List entries read in alphabetical order, with numbers compared as numbers.                                          |
| `entry-shape / entry-key`              | error   | Each entry of an object list is a mapping, carrying the keys the field declares and no others.                      |
| `tier-matches-type`                    | error   | `tier` matches the tier the type declares.                                                                          |
| `id`                                   | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename.     |
| `id-unique`                            | error   | `id` is unique across the whole corpus.                                                                             |
| `filename / slug-length`               | error   | Filename matches the pattern. The slug is within 30 characters.                                                     |
| `h1`                                   | error   | The document has an H1.                                                                                             |
| `identity`                             | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.            |
| `sections`                             | error   | Every required section heading is present, and no declared section is left as a bare heading.                       |
| `placeholder-left`                     | error   | No `{{…}}` from the template is left unfilled, outside code.                                                        |
| `clauses`                              | error   | The clause section is a table of `Id \| Clause` rows, each id a code span and each clause opening with its modal.   |
| `clause-order / clause-compound`       | warning | Clause rows are grouped by binding level, and each carries a single obligation.                                     |
| `part-id-unique / part-ref`            | error   | No two parts of a record share an address, and a `record-id.part` citation reaches the part it names.               |
| `link-resolves`                        | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.                  |
| `undefined-label`                      | error   | Every shortcut reference has a link definition.                                                                     |
| `label-canonical`                      | error   | A shortcut label that names a document is written as that document's id.                                            |
| `unused-definition`                    | warning | A link definition that nothing references.                                                                          |
| `alignment-rollup / framework-posture` | error   | `aligns-with` carries every binding reference the `Alignment` column cites, and the register places each framework. |
| `posture-belongs-to-frameworks`        | warning | "compliant", "certified" or "registered" near a framework reference. Standing belongs in `frameworks.md`.           |

<!-- END GENERATED: checks-policies -->
