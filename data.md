# Data

Where data lives, how long we keep it, and how sensitive it is.

**[→ Index](data/_index.md)**

## What is a data document?

One per data domain: which entities it covers, which [service](/services) owns them, what store they live in, how
sensitive they are, how long we keep them, and where they flow.

## Why we use them

Two audiences, one document. For anyone building something, it answers the question asked constantly and answered
inconsistently: *where do bookings actually live, and who owns them?* For the [policy](/policies) tier, it is the
evidence — an auditor's first question is what personal data exists and how long it is kept, and the answer should not
require an archaeology exercise.

Recording ownership also surfaces the cases where two services believe they own the same entity, which is a design
problem worth finding on paper.

## Scope

Data documents are **descriptive** — they mirror what is actually stored. They are organised by data domain rather than
by store, because a domain often spans stores and that is the interesting part.

Not the place for:

* **Schema definitions** — those live with the code that owns them.
* **How to query it** — that is a [process](/processes) or a service document.
* **Retention rules as commitments** — the *commitment* is a [policy](/policies); this records the *actual* retention.
  Where they differ, that gap is worth knowing about.

Note the folder is singular — `data/` — because English gives no plural. It is the one exception alongside
[`glossary.md`](/glossary).

## Metadata

<!-- BEGIN GENERATED: schema-data -->

| Field            | Req | Type   | Notes                                                                                          |
|------------------|-----|--------|------------------------------------------------------------------------------------------------|
| `id` †           | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                          |
| `tier` †         | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.           |
| `status` †       | ●   | enum   | Whether the store is current or on its way out.                                                |
| `owner` †        | ●   | string | A named person, never a team alias.                                                            |
| `tags` †         |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                               |
| `owned-by`       | ●   | id     | A single service. Shared ownership means nobody is answerable.                                 |
| `classification` | ●   | enum   | Drives handling. `personal` and `special-category` pull in retention.                          |
| `retention`      |     | string | The actual retention, not the policy's. Where they differ, record both — the gap is the point. |
| `flows-to`       |     | list   | Data leaving the estate is the part that matters most.                                         |

**Enum values**

| Field            | Values                                                                   |
|------------------|--------------------------------------------------------------------------|
| `tier`           | `decided` · `normative` · `descriptive` · `procedural` · `observed`      |
| `status`         | `active` · `deprecated`                                                  |
| `classification` | `public` · `internal` · `confidential` · `personal` · `special-category` |

**Conditionally required**

| Field       | Required when                                    |
|-------------|--------------------------------------------------|
| `retention` | `classification in [personal, special-category]` |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-data -->

## Adding a data document

1. Copy [`_template.md`](data/_template.md) to `<slug>.md`. Data documents use slug ids — `dat-<name>`.
2. Name the entities it covers and the single service that owns them. If two services claim ownership, resolve that
   before writing the document.
3. Classify honestly. Customer names, email addresses and payment histories are `personal`; anything special-category
   needs a lawful basis recorded.
4. State `retention` concretely — "indefinitely" is an answer, and a revealing one.
5. Record `flows-to`: which services and [integrations](/integrations) receive this data. Data leaving the estate is the
   part that matters most.

**Conventions**

* **One owning service per domain.** Shared ownership means nobody is answerable.
* **Never put actual data here** — no sample records, no identifiers, no connection strings.
* **Personal data without a stated retention** is reported by CI. It is the first thing anyone external will ask.

## What CI checks

<!-- BEGIN GENERATED: checks-data -->

| Check                       | Level   | What it verifies                                                                                         |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                      |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                           |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                        |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                  |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                           |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                     |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                  |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                             |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                               |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                               |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                            |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                    |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                          |
| `h1`                        | error   | The document has an H1.                                                                                  |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter. |
| `required-section`          | error   | Every required section heading is present.                                                               |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                 |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                 |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |
| `no-actual-data`            | error   | Fails on anything resembling a record, identifier or connection string. This wiki is broadly readable.   |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                | What it would verify                                      |
|---------------------|-----------------------------------------------------------|
| `store-has-service` | Every store named resolves to a service in the catalogue. |

<!-- END GENERATED: checks-data -->
