# Explanations

How the platform works, and why it is shaped the way it is.

**[→ Index](explanations/_index.md)**

## What is an explanation?

An explanation is prose whose job is comprehension. An architecture overview, a walkthrough of how a request flows
through the estate, an account of why the testing approach is shaped as it is. You read one to build a mental model, not
to consult it mid-task.

Every other type answers a narrow question: what was decided, what you must do, how you do it, what a component is. An
explanation answers how those pieces fit together. A service page or an ADR that took that question on would turn into
one.

## Why we use them

Narrative that has no home either goes unwritten or piles up at the top level of the corpus, with no owner and no review
date. That is how a page goes stale. The type gives every explanation an owner and a `review-by`, like every other
record.

## Scope

An explanation is **not**:

* **Normative.** If it says what you must do, it is a [standard](standards.md).
* **Procedural.** If it says how to perform a task, it is a [process](processes.md) or a [runbook](runbooks.md).
* **A catalogue entry.** If it describes one component, it is a [service](services.md).
* **A decision.** If it records what was chosen and why, it is an [ADR](adrs.md).

**Explanations link rather than restate.** An architecture overview points at the services, capabilities and ADRs that
hold the detail. An explanation that states a fact those documents already own holds the second copy of it. That copy is
the one that goes stale first.

This type is the residual, and a residual with a low bar fills up with whatever fits nowhere else. If a document could
plausibly be an explanation *or* something else, it is the something else.

## Metadata

<!-- BEGIN GENERATED: schema-explanations -->

| Field         | Value                    | Notes                                                                                        |
|---------------|--------------------------|----------------------------------------------------------------------------------------------|
| `id` *†       | string                   | Stable, unique across the corpus, never reused. Format set by the type.                      |
| `tier` *†     | `descriptive`            | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.          |
| `status` *†   | `draft` `active` `stale` | `stale` is an honest state: say so rather than let the page quietly rot.                     |
| `owner` *†    | string                   | A named person, never a team alias.                                                          |
| `tags` †      | list                     | Free-form, lowercase, hyphenated. Used for cross-cutting search.                             |
| `explains` *  | list                     | Service or capability ids this explains.                                                     |
| `review-by` * | date                     | The field that stops this type rotting. Explanations need the tightest staleness discipline. |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-explanations -->

## Adding an explanation

1. Check it is none of the exclusions above.
2. Copy [`_template.md`](explanations/_template.md) to a kebab-case filename, with no number prefix. Explanations are
   named, not sequenced.
3. Set `explains` to the services or capabilities it covers, and `review-by`.
4. Write it as prose. Link out for every concrete fact you are tempted to state.

## What CI checks

<!-- BEGIN GENERATED: checks-explanations -->

| Check                        | Level   | What it verifies                                                                                                |
|------------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`         | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`                | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                  | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`             | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                   | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format`  | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                       | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`              | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                 | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`          | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                         | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                  | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`     | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                         | error   | The document has an H1.                                                                                         |
| `identity`                   | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                   | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`           | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`              | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`            | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`            | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`          | warning | A link definition that nothing references.                                                                      |
| `links-rather-than-restates` | warning | An explanation's prose stays proportionate to its links, rather than restating their facts.                     |
| `not-normative`              | warning | No bold RFC 2119 keyword (MUST, MUST NOT, SHOULD, SHOULD NOT, MAY) binds from an explanation.                   |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule               | What it would verify                                                             |
|--------------------|----------------------------------------------------------------------------------|
| `not-load-bearing` | No decided-tier document cites an explanation as the authority for a constraint. |

<!-- END GENERATED: checks-explanations -->
