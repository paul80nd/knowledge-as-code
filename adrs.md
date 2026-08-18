# Architecture Decision Records (ADRs)

A log of the architecturally significant decisions we have made about the platform.

**[→ ADRs index](adrs/_index.md)**

## What is an ADR?

An Architecture Decision Record (ADR) is a short Markdown document holding one architecturally significant decision. It
carries the context that led to the decision, the decision itself, the alternatives we weighed, and the consequences for
everyone working downstream of it.

ADRs are **immutable once accepted**. When a decision changes, we write a new ADR that supersedes the old one and leave
the old one where it is. A reader then has the reasoning behind the architecture we run today and the reasoning behind
the decisions it replaced.

## Why we use them

The platform spans many repositories, and many of our architectural decisions land in more than one of them: message
contracts, the service bus, auth, deployment ordering, infrastructure patterns. Without a durable record:

* The same questions resurface every few months ("why aren't we using Event Grid?", "why is the migration runner
  separate?") because nobody can point at the answer.
* A new contributor reads the code and learns the *shape* of the architecture, but not the *reasoning* behind it.
* An engineer working inside one repository drifts away from the decisions that cross several, because nothing in that
  repository carries the reasoning.

## Scope: central vs repo-local

An ADR here covers a decision spanning **more than one** repository. A decision entirely local to a single repository —
a library choice, an internal naming convention, a refactor approach — belongs in that repository, under `/docs/adrs/`.

Where a central ADR later supersedes a repo-local one, set the local ADR to superseded and reference the central ADR by
its id.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Value                                                       | Notes                                                                                            |
|-----------------|-------------------------------------------------------------|--------------------------------------------------------------------------------------------------|
| `id` *†         | string                                                      | Stable, unique across the corpus, never reused. Format set by the type.                          |
| `tier` *†       | `decided` `normative` `descriptive` `procedural` `observed` | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.             |
| `status` *†     | `proposed` `accepted` `deprecated` `superseded`             | Immutable once `accepted` — supersede rather than rewrite.                                       |
| `owner` *†      | string                                                      | A named person, never a team alias.                                                              |
| `tags` †        | list                                                        | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                 |
| `decided-on`    | date                                                        | The acceptance date. Bare key until accepted.                                                    |
| `supersedes`    | id                                                          | The ADR this replaces.                                                                           |
| `superseded-by` | id                                                          | CI enforces both directions; a one-sided supersession fails the build.                           |
| `deciders`      | list                                                        | The people who agreed it.                                                                        |
| `related`       | list                                                        | Must match the ids named in the `## Related` section. CI reconciles the two, case-insensitively. |

**Conditionally required**

| Field           | Required when          |
|-----------------|------------------------|
| `decided-on`    | `status == accepted`   |
| `superseded-by` | `status == superseded` |

\* Field is required  
† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-adrs -->

## Adding an ADR

1. Copy [`_template.md`](adrs/_template.md) to `NNNN-kebab-case-title.md`, where `NNNN` is the next unused four-digit
   number. The [index](adrs/_index.md) shows the highest one in use.
2. Fill in the frontmatter and the sections. Keep it short — narrative paragraphs, not form-filling.
3. Open a PR. The status starts at `proposed`.
4. On acceptance, set `status: accepted` and `decided-on`. The index rebuilds itself.

**Conventions**

* **Filename** — `NNNN-kebab-case-title.md`. Sequential, zero-padded, never reused. A withdrawn proposal retires its
  number.
* **Immutability** — once an ADR is accepted, change nothing in it beyond its status and its typos. To change a
  decision, write a new ADR that supersedes the old one.
* **Superseding** — set the old ADR's `status: superseded` and `superseded-by`, and the new one's `supersedes`. A
  supersession recorded on one side only fails the build.
* **Prescriptive language** — an ADR that establishes a default or a policy may use
  [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119) keywords. An ADR that records a decision uses plain
  declarative prose.
* **Format** — a lean Nygard-style format with an explicit Alternatives Considered section.
  [adr-0001](adrs/0001-knowledge-as-code.md) is the worked example.

See [Contributing](/knowledge-as-code/contributing.md) for the review model that applies to all Decided-tier documents.

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has — `YYYY-MM-DD`.                                         |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                        | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                 | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `related-matches-section`   | error   | A field that mirrors a section reconciles with the ids in that section.                                         |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `y-statement`               | warning | A Y-statement block-quote follows the H1, states all six moves, and is within its word ceiling.                 |
| `alternatives-verdict`      | warning | Each Alternatives Considered bullet states a verdict.                                                           |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                       | What it would verify                                                                             |
|----------------------------|--------------------------------------------------------------------------------------------------|
| `immutable-after-accepted` | Once status is `accepted`, only typo fixes, link corrections and status transitions are allowed. |

<!-- END GENERATED: checks-adrs -->
