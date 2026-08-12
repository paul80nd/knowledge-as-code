# Data

Where data lives, how long we keep it, and how sensitive it is.

**[→ Index](data/_index.md)**

## What is a data document?

One document per data domain. It names the entities in the domain, the [service](/services) that owns them, and the
stores they live in. The rest of the record says how sensitive they are, how long we keep them, and where they flow.

## Why we use them

Two readers arrive with different questions. Someone building a feature wants to know where bookings actually live and
which service owns them. Ask around and the answers disagree. Someone answering an auditor wants to know what personal
data we hold and how long we keep it. Nobody should have to read a database schema to answer that. A data document
answers the first question the same way every time, and gives the [policy](/policies) tier its evidence for the second.

An author filling in `owned-by` also finds the entities that two services both believe they own. That disagreement is a
design problem worth finding on paper.

## Scope

A data document is **descriptive**: it mirrors what is actually stored. We organise data documents by data domain rather
than by store, because a domain often spans stores and that spread is the part worth seeing.

Not the place for:

* **Schema definitions** — those live with the code that owns them.
* **How to query the data** — that is a [process](/processes) or the service's own document.
* **Retention rules as commitments** — a [policy](/policies) holds the commitment, and a data document records what the
  store actually does.

The folder is singular — `data/` — because English gives no plural. It and [`glossary/`](/glossary) are the two
exceptions to the plural-folder rule.

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

1. Copy [`_template.md`](data/_template.md) to `<slug>.md`. Data documents take slug ids, `dat-<name>`.
2. Name the entities the domain covers and the one service that owns them. If two services claim the same entity,
   resolve the claim before you write the document.
3. Classify honestly. Customer names, email addresses and payment histories are `personal`. Where the classification is
   `special-category`, record the lawful basis for holding it.
4. State `retention` concretely. "Indefinitely" is an answer, and a revealing one.
5. Record `flows-to`: the services and [integrations](/integrations) that receive this data.

**Conventions**

* **A `personal` or `special-category` classification requires a `retention`.** Leave it out and `required-field` fails
  the build.
* **Never put actual data here** — no sample records, no identifiers, no connection strings.

## What CI checks

<!-- BEGIN GENERATED: checks-data -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                            |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                           |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                        |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `no-actual-data`            | error   | Fails on an email address outside `example.com`. Nothing catches an identifier or a connection string.          |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                | What it would verify                                      |
|---------------------|-----------------------------------------------------------|
| `store-has-service` | Every store named resolves to a service in the catalogue. |

<!-- END GENERATED: checks-data -->
