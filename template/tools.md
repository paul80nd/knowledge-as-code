# Tools

The packages, services and utilities we have approved, rejected, or are trialling.

**[→ Index](tools/_index.md)**

## What is a tool entry?

A record of something we build with: a package, a framework, a linter, a hosted service, a CLI. It says what the tool is
for, whether it is approved, which versions we stand behind, its licence, and what we chose it over.

Together the entries are a lightweight software approval register. A larger organisation would run one as an approval
board. Three engineers run it as a folder of records.

## Why we use them

Two problems, one register. Nobody can answer *"are we allowed to use this?"* without asking around, so each project
answers it alone. The estate ends up with several ways of doing one job. Nobody can answer *"what are we actually
depending on?"* without opening every manifest we own. That second question arrives with a licence review, with a
security advisory, and on the day a dependency is abandoned.

**Declared.** `drift-against-manifests` is declared and does not run. `packages` states the package URL it would match
a manifest entry against. Once something implements it, the register can be compared against the real manifests in both
directions: packages in use that were never approved, and approved packages nothing uses any more.

## Scope

A tool is something we **build with**, not something we run. A running system we call is an
[integration](integrations.md). Something we deploy is a [service](services.md).

The register records **current state**. An [ADR](adrs.md) records the **decision**, where there was one worth recording.
A small, uncontroversial adoption needs only a register entry. A contested or expensive choice earns both, and the entry
cites the ADR in `decided-in`.

A `rejected` entry earns its place. Somebody proposes the same package two years later, and the entry hands them
the evaluation we already did.

## Metadata

<!-- BEGIN GENERATED: schema-tools -->

| Field              | Value                                      | Notes                                                                                  |
|--------------------|--------------------------------------------|----------------------------------------------------------------------------------------|
| `id` *†            | string                                     | Stable, unique across the corpus, never reused, in the format the type sets.           |
| `type` *†          | string                                     | The singular name of the type, which CI checks against the folder.                     |
| `tier` *†          | `descriptive`                              | The record's trust level, fixed for the type and checked against the folder.           |
| `status` *†        | `approved` `trial` `deprecated` `rejected` | `approved` applies to new work. Existing use of a tool with any other status is drift. |
| `owner` *†         | string                                     | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                 |
| `sources` †        | list                                       | Where this record's content came from, one entry per source.                           |
| `tags` †           | list                                       | Free-form, lowercase and hyphenated. A reader searches on these across types.          |
| `category`         | derived from the record's sub-path         | The folder the tool is filed under, below `tools/`.                                    |
| `packages`         | list                                       | Every package this entry approves, with the version range approved for each.           |
| `homepage` *       | string                                     | The project's own page.                                                                |
| `licence` *        | string                                     | The licence concluded to apply, as an SPDX expression, `NONE` or `NOASSERTION`.        |
| `licence-declared` | string                                     | The package's own statement of its licence, kept word for word.                        |
| `decided-on` *     | date                                       | Quoted. The day the current status was decided.                                        |
| `review-by` *      | date                                       | Quoted. The day by which somebody checks this entry is still right.                    |
| `decided-in`       | id                                         | The ADR id recording the decision to adopt this tool.                                  |
| `replaces`         | id                                         | The tool id this supersedes.                                                           |
| `successor`        | id                                         | The tool id that replaces this one.                                                    |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-tools -->

## Adding a tool

1. Copy [`_template.md`](tools/_template.md) to `<slug>.md`. Tools use slug ids: `tol-vitest`.
2. Set `status`. `trial` covers something being evaluated in one place. Promote or reject it once the evaluation ends.
3. Set `decided-on` to the day you decided, and `review-by` to the day somebody checks the entry again.
4. List each package in `packages`, as a package URL with the range approved for new work.
5. Record the `homepage`.
6. Read the package's own licence files. Write what the package states in `licence-declared`, and what you concluded
   in `licence`.
7. Where this tool takes over from an older one, name the older tool in `replaces`, so the deprecation path is visible.
8. Cite `decided-in` where an ADR exists. Where the choice was contested and no ADR exists, write one.

**Conventions**

* **Approved means approved for new work.** Something already in use but not approved is drift, and finding it is a
  manual job until something implements `drift-against-manifests`.
* **A deprecated entry names its successor.** Set `successor` to whatever took over, so somebody arriving from a
  manifest has somewhere to go next.
* **Give a package a range, not a pin.** The pin belongs in the manifest.
* **An entry expires.** `review-in-date` warns once `review-by` has passed, so an approval nobody revisits stops
  speaking for the estate.
* **A conclusion that departs from the declaration says why.** `conclusion-states-a-reason` warns where `licence` and
  `licence-declared` differ and no `## Licence and obligations` section says what you read.

## What CI checks

<!-- BEGIN GENERATED: checks-tools -->

| Check                        | Level   | What it verifies                                                                                                |
|------------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`         | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`                | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `derived-key`                | error   | A field derived from the record's folder is not written in frontmatter.                                         |
| `key-order`                  | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`             | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                   | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `empty-optional-key`         | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format`  | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                       | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`              | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `min-items`                  | error   | A list field carries at least as many entries as its schema asks for.                                           |
| `list-order`                 | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `entry-shape / entry-key`    | error   | An object field, and each entry of an object list, carries the keys the field declares and no others.           |
| `type-matches-folder`        | error   | `type` matches the singular type name the record's folder declares.                                             |
| `tier-matches-type`          | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                         | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                  | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`     | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                         | error   | The document has an H1.                                                                                         |
| `identity`                   | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                   | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`           | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`              | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`            | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`            | error   | A link label is the id of the record it leads to, written as that record carries it.                            |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                 | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`          | warning | A link definition that nothing references.                                                                      |
| `deprecated-has-successor`   | warning | A deprecated tool states what replaces it.                                                                      |
| `trial-has-criteria`         | warning | A tool in `trial` states what would end the trial.                                                              |
| `exit-states-a-reason`       | warning | A deprecated or rejected tool says what went wrong with it.                                                     |
| `conclusion-states-a-reason` | warning | A tool whose concluded licence differs from its declared one says how that was reached.                         |
| `review-in-date`             | warning | An entry names a review date that has not passed.                                                               |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                      | What it would verify                                                        |
|---------------------------|-----------------------------------------------------------------------------|
| `drift-against-manifests` | Every package in use is in the register, and every approved tool is in use. |

<!-- END GENERATED: checks-tools -->
