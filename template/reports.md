# Reports

What the corpus says about itself, once somebody has read it.

**[→ Index](reports/_index.md)**

## What is a report?

A report answers a question no single record holds. Which policy clauses nothing implements. Which framework references
hang on one citation. The answer is a walk over every record at once, so nowhere else in the corpus can carry it.

`kac report` does the walking. It fills every cell the corpus states and leaves the judgement cells open, because it
cannot tell a gap worth closing from an obligation about something this organisation does not have. A report is that
output with those cells answered.

## Why we use them

An edge nobody reads back rots. A clause loses its last covering standard, a framework reference loses its last citing
clause, and the corpus goes on validating. A report is where somebody looks.

Keeping it as a record rather than as a file generated on demand puts an owner on the answer and a date on it. The
argument written on top of the numbers is written once, and the next reader starts from it.

## Scope

A report is **not**:

* **An explanation.** If it would still be true with every record deleted, it is an [explanation](explanations.md).
* **A discovery.** If nothing would reproduce it, it is a [discovery](discoveries.md).
* **Raw output.** If nobody has answered the judgement cells, it is a command's output and belongs in a pipe.

**A report is stale the moment the corpus moves.** That is the price of keeping it, and the frontmatter is what keeps
the price visible: `sources` names the version of each corpus the report answers for, and `confirmed` names who last
checked that it still holds.

## Metadata

<!-- BEGIN GENERATED: schema-reports -->

| Field         | Value                    | Notes                                                                               |
|---------------|--------------------------|-------------------------------------------------------------------------------------|
| `id` *†       | string                   | Stable, unique across the corpus, never reused. Format set by the type.             |
| `type` *†     | string                   | The type's singular name. Fixed for the type. CI checks it matches the folder.      |
| `tier` *†     | `descriptive`            | Fixed for the type. A trust signal for the reader. CI checks it matches the folder. |
| `status` *†   | `draft` `active` `stale` | `stale` is an honest state: a report nobody has run since the corpus moved says so. |
| `owner` *†    | string                   | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.              |
| `tags` †      | list                     | Free-form, lowercase, hyphenated. Used for cross-cutting search.                    |
| `generated` * | object                   | What produced the content and when.                                                 |
| `sources` *   | list                     | Every corpus this report answers for, and the version of each it is true of.        |
| `confirmed` * | list                     | Every confirmation this report has had, oldest first, one line each.                |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-reports -->

## Adding a report

1. Run `kac report <name>` and read what it printed. The limits it states are part of the document.
2. Copy [`_template.md`](reports/_template.md) to a kebab-case filename naming the question, and paste the output under
   the frontmatter.
3. Answer every judgement cell. A cell you cannot answer is a question for whoever owns the area, not a blank.
4. Set `sources` to the `content-version` of each corpus the run read, and add a `confirmed` entry naming yourself.

**Regenerating replaces the content.** Carry forward every verdict whose row is unchanged, and answer the rows that
moved. Where nothing in the corpus touched the report, raise the `sources` version by hand and leave `confirmed` alone.
A version the corpus moved for something else is no reason to claim a fresh read.

## What CI checks

<!-- BEGIN GENERATED: checks-reports -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `timestamp-format`          | error   | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                                |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `min-items`                 | error   | A list field carries at least as many entries as its schema asks for.                                           |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `entry-shape / entry-key`   | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.           |
| `type-matches-folder`       | error   | `type` matches the singular type name the record's folder declares.                                             |
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
| `label-canonical`           | error   | A shortcut label is the id of the record it leads to, written as that record carries it.                        |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `report-stale`              | warning | Each corpus a report answers for is at the version the report names.                                            |
| `confirmed-by-a-person`     | error   | A confirmation names the person who made it.                                                                    |
| `generated-by-a-producer`   | error   | `generated.by` names what produced the content, in OKF's `<producer>/<version>` form.                           |

<!-- END GENERATED: checks-reports -->
