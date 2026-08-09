# Architecture Decision Records (ADRs)

This wiki maintains a log of architecturally significant decisions made for the platform.

**[→ ADRs index](adrs/_index.md)**

## What is an ADR?

An Architecture Decision Record (ADR) is a short Markdown document capturing a single architecturally significant
decision — the context that led to it, the decision itself, the alternatives weighed, and the consequences for everyone
working downstream of it.

ADRs are **immutable once accepted**. When a decision later changes, a new ADR supersedes the old one rather than
rewriting history. The wiki therefore preserves the reasoning behind both the current architecture *and* how we got
there.

## Why we use them

The platform spans many repositories, and a significant share of architectural decisions affect more than one of them —
message contracts, the service bus, auth, deployment ordering, infrastructure patterns. Without a durable record:

* The same questions resurface every few months ("why aren't we using Event Grid?", "why is the migration runner
  separate?") because there is no canonical answer.
* New contributors can learn the current *shape* of the architecture from code but not the *reasoning* behind it.
* Engineers working inside individual repos drift away from cross-cutting decisions, because the central rationale is
  invisible from inside those repos.

## Scope: central vs repo-local

ADRs here cover decisions spanning **more than one** repository. Decisions entirely local to a single repository — a
library choice, an internal naming convention, a refactor approach — belong in that repository, under `/docs/adrs/`.

If a repo-local decision is later superseded by a central one, mark the local ADR superseded and reference the central
ADR by ID.

## Metadata

<!-- BEGIN GENERATED: schema-adrs -->

| Field           | Req | Type   | Notes                                                                                            |
|-----------------|-----|--------|--------------------------------------------------------------------------------------------------|
| `id` †          | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                            |
| `tier` †        | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.             |
| `status` †      | ●   | enum   | Immutable once `accepted` — supersede rather than rewrite.                                       |
| `owner` †       | ●   | string | A named person, never a team alias.                                                              |
| `tags` †        |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                 |
| `decided-on`    |     | date   | The acceptance date. Bare key until accepted.                                                    |
| `supersedes`    |     | id     | The ADR this replaces.                                                                           |
| `superseded-by` |     | id     | CI enforces both directions; a one-sided supersession fails the build.                           |
| `deciders`      |     | list   | The people who agreed it.                                                                        |
| `related`       |     | list   | Must match the ids named in the `## Related` section. CI reconciles the two, case-insensitively. |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `proposed` · `accepted` · `deprecated` · `superseded`               |

**Conditionally required**

| Field           | Required when          |
|-----------------|------------------------|
| `decided-on`    | `status == accepted`   |
| `superseded-by` | `status == superseded` |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-adrs -->

## Adding an ADR

1. Copy [`_template.md`](adrs/_template.md) to `NNNN-kebab-case-title.md`, where `NNNN` is the next unused four-digit
   number — check the [index](adrs/_index.md) for the highest in use.
2. Fill in the frontmatter and sections. Keep it short — narrative paragraphs, not form-filling.
3. Open a PR. Status starts `proposed`.
4. On acceptance, set `status: accepted` and `decided-on`. The index rebuilds itself.

**Conventions**

* **Filename** — `NNNN-kebab-case-title.md`. Sequential, zero-padded, never reused. A withdrawn proposal retires its
  number.
* **Immutability** — once accepted, content is not rewritten beyond status changes and typo fixes. To change a decision,
  write a new ADR that supersedes the old one.
* **Superseding** — set the old ADR's `status: superseded` and `superseded-by`, and the new one's
  `supersedes`. CI checks both directions; a one-sided supersession fails the build.
* **Prescriptive language** — ADRs establishing defaults or policies may use
  [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119) keywords. Decision-style ADRs use plain declarative prose.
* **Format** — a lean Nygard-style format with an explicit Alternatives Considered section.
  [adr-0001](adrs/0001-knowledge-as-code.md) is the worked example.

See [Contributing](/knowledge-as-code/contributing.md) for the review model that applies to all Decided-tier documents.

## What CI checks

<!-- BEGIN GENERATED: checks-adrs -->

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
| `related-matches-section`   | error   | A field that mirrors a section reconciles with the ids in that section.                                  |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                 |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                         |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |
| `y-statement`               | warning | A Y-statement block-quote follows the H1, states all six moves, and is within 60 words.                  |
| `alternatives-verdict`      | warning | Each Alternatives Considered bullet states a verdict.                                                    |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                       | What it would verify                                                                                                                                                                |
|----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `immutable-after-accepted` | Once status is `accepted`, content changes are limited to typo fixes, link corrections and status transitions. A substantive edit fails with a pointer to the supersession process. |

<!-- END GENERATED: checks-adrs -->
