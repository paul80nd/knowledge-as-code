# Policies

Fixture scaffolding. A stood-up type needs a page beside its folder, so this stands in for the real one. It is checked
as any type page is: its links, both pairs of generated markers, and carrying no frontmatter of its own.

## Metadata

<!-- BEGIN GENERATED: schema-policies -->

| Field         | Value                              | Notes                                                                                             |
|---------------|------------------------------------|---------------------------------------------------------------------------------------------------|
| `id` *†       | string                             | Stable, unique across the corpus, never reused, in the format the type sets.                      |
| `type` *†     | string                             | The singular name of the type, which CI checks against the folder.                                |
| `tier` *†     | `normative`                        | The record's trust level, fixed for the type and checked against the folder.                      |
| `status` *†   | `draft` `active` `retired`         | Whether the policy is being agreed, in force, or retired. Retire a policy instead of deleting it. |
| `owner` *†    | string                             | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                            |
| `sources` †   | list                               | Where the content came from, one entry per source.                                                |
| `tags` †      | list                               | Free-form, lowercase and hyphenated. A reader searches on these across types.                     |
| `category`    | derived from the record's sub-path | The folder the policy is filed under, below `policies/`.                                          |
| `aligns-with` | list                               | The binding frameworks this policy's clauses map to, grouped with the references they cite.       |
| `review-by` * | date                               | The day the policy is looked at again, usually a year ahead.                                      |

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
| `bare-key`                             | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                                 |
| `empty-optional-key`                   | warning | An optional field is filled in or left out, rather than written with no value.                                      |
| `date-quoted / date-format`            | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                              |
| `enum`                                 | error   | Enum values are in range and lowercase.                                                                             |
| `field-pattern`                        | error   | Values match the pattern their field declares (e.g. `tags`).                                                        |
| `list-order`                           | warning | List entries read in alphabetical order, with numbers compared as numbers.                                          |
| `entry-shape / entry-key`              | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.               |
| `type-matches-folder`                  | error   | `type` matches the singular type name the record's folder declares.                                                 |
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
| `label-canonical`                      | error   | A shortcut label is the id of the record it leads to, written as that record carries it.                            |
| `unused-definition`                    | warning | A link definition that nothing references.                                                                          |
| `alignment-rollup / framework-posture` | error   | `aligns-with` carries every binding reference the `Alignment` column cites, and the register places each framework. |
| `framework-uncited`                    | error   | Every framework on the register is cited by at least one clause.                                                    |
| `posture-belongs-to-frameworks`        | warning | "compliant", "certified" or "registered" written near a framework reference.                                        |

<!-- END GENERATED: checks-policies -->
