# Offerings

What the product offers its customers, and why.

**[→ Index](offerings/_index.md)**

## What is an offering?

One document per customer-visible surface of the product. It records what the surface does, why it exists, which
services implement it, and where the detail lives.

An offering is a **hub**. It links to the work items that specify it, the feature files that test it, the services
that implement it and the NFRs that constrain it. It does not restate any of them.

## Why we use them

Functional detail lives in the work items your tracker keeps. A tracker has nothing above them: no account of what the
product offers a customer and why. It could not keep one without disturbing a hierarchy that exists to run delivery.

Without that account, nobody can answer "what does the product do?" from one place, and everyone starting a significant
piece of work rebuilds the same context.

## Scope

One document per **customer-visible surface**, not per work item and not per service. One offering typically spans
several services. One service often contributes to several offerings.

**An offering whose `implemented-by` names one service is a synonym for that service.** Write the service record
instead. This type was first tried over an estate of four services where every offering mapped onto one of them, and
each record restated the service beside it.

**Offerings link rather than restate.** An offering that specifies behaviour has begun to drift from the work items
it should point at. The next session to read it will trust it anyway, which makes a drifted offering worse than none.
Acceptance criteria go in the tracker.

Related but different:

* **Spec**: how standards apply to one concrete contract. It lives in the repository that owns the feature, beside its
  OpenAPI document and feature files. [ADRs](adrs.md) split the same way: cross-repo synthesis here, feature-level
  detail with the code.
* **[Service](services.md)**: a thing we deploy. An offering is a thing a customer gets.
* **[Explanation](explanations.md)**: how something works internally. An offering is what it does externally.

## Metadata

<!-- BEGIN GENERATED: schema-offerings -->

| Field              | Value                                    | Notes                                                                                       |
|--------------------|------------------------------------------|---------------------------------------------------------------------------------------------|
| `id` *†            | string                                   | Stable, unique across the corpus, never reused, in the format the type sets.                |
| `type` *†          | string                                   | The singular name of the type, which CI checks against the folder.                          |
| `tier` *†          | `descriptive`                            | The record's trust level, fixed for the type and checked against the folder.                |
| `status` *†        | `planned` `building` `live` `deprecated` | Lifecycle of the offering itself, which may differ from the lifecycle of its services.      |
| `owner` *†         | string                                   | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                      |
| `sources` †        | list                                     | Where this record's content came from, one entry per source.                                |
| `tags` †           | list                                     | Free-form, lowercase and hyphenated. A reader searches on these across types.               |
| `implemented-by` * | list                                     | Ids of the services that implement this offering.                                           |
| `feature-files`    | list                                     | Paths to the feature files that test this offering, each one beginning with its repository. |
| `nfrs`             | list                                     | Ids of the NFRs this offering must meet.                                                    |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-offerings -->

## Adding an offering

1. Copy [`_template.md`](offerings/_template.md) to `<slug>.md`. Offering ids are slugs: `cap-<name>`.
2. Write the *what* and the *why* in prose. Two or three paragraphs is usually enough.
3. Fill in `implemented-by` and `feature-files`, and link the work items from the list. Those links make it a hub.
4. Do not explain how it works. Link to the services and explanations that already do.

**Conventions**

* **Hub, not specification.** `hub-not-specification` weighs the whole document against its outbound links, at roughly
  forty words each, so an offering that grows a section of its own trips it. Where a section runs longer than the links
  around it, ask whether the detail belongs in a work item.
* **The list and the frontmatter say the same thing, and CI checks it.** `related-matches-section` reconciles
  `implemented-by` and `nfrs` against the ids the list names, in both directions. Write an id in one place and you
  write it in both.
* **Work items live in the list alone.** There is no field for them. A consumer reading an export sees the label and
  not the address, because a section travels and its link definitions do not.
* **Keep the rest of a feature file path honest yourself.** `feature-file-repo` checks the repository the path opens
  with. Past that first segment the field is a plain string, `ref-resolves` never sees it, and `feature-file-orphans`
  is declared and does not run. A path that goes stale there goes stale quietly.

## What CI checks

<!-- BEGIN GENERATED: checks-offerings -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `empty-optional-key`        | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
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
| `related-matches-section`   | error   | A field that mirrors a section reconciles with the ids in that section.                                         |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `feature-file-repo`         | warning | A feature file path begins with a repository one of the implementing services names.                            |
| `hub-not-specification`     | warning | An offering's prose stays proportionate to the links it makes.                                                  |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                   | What it would verify                                                                                               |
|------------------------|--------------------------------------------------------------------------------------------------------------------|
| `feature-file-orphans` | Scheduled. Reports feature files in the code repositories no offering claims, and paths here that no longer exist. |

<!-- END GENERATED: checks-offerings -->
