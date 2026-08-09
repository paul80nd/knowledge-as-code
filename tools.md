# Tools

The packages, services and utilities we have approved, rejected, or are trialling.

**[→ Index](tools/_index.md)**

## What is a tool entry?

A record of something we build with: a package, a framework, a linter, a hosted service, a CLI. What it is for, whether
it is approved, which versions we stand behind, its licence, and what we chose it over.

Collectively this is a lightweight software approval register — the thing a larger organisation would call an approval
board, at a size that fits three engineers.

## Why we use them

Two problems, one register. Nobody can currently answer *"are we allowed to use this?"* without asking, so the answer
gets invented per project and the estate accretes four ways of doing the same thing. And nobody can answer *"what are we
actually depending on?"* without reading seventeen manifests, which matters for licences, for security advisories and
for the day a dependency is abandoned.

Once the register exists, CI can compare it against the real manifests and report drift in both directions — packages in
use that were never approved, and approved tools nothing uses any more.

## Scope

A tool is something we **build with**, not something we run. A running system we call is an
[integration](/integrations); something we deploy is a [service](/services).

The register records **current state**; an [ADR](/adrs) records the **decision** where there was one worth recording.
Small, uncontroversial adoptions need only a register entry — not every package choice deserves a decision record. A
contested or expensive choice deserves both, and the entry cites the ADR in `decided-in`.

`rejected` entries earn their place. Recording what we considered and turned down is what stops the same evaluation
being repeated in eighteen months.

## Metadata

<!-- BEGIN GENERATED: schema-tools -->

| Field        | Req | Type   | Notes                                                                                                |
|--------------|-----|--------|------------------------------------------------------------------------------------------------------|
| `id` †       | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                                |
| `tier` †     | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.                 |
| `status` †   | ●   | enum   | `approved` means approved for new work; existing use that is not approved is drift.                  |
| `owner` †    | ●   | string | A named person, never a team alias.                                                                  |
| `tags` †     |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                     |
| `category`   | ●   | string | e.g. `testing`, `build`, `observability`, `runtime`.                                                 |
| `versions`   |     | string | A range, not a pin. The register states what we stand behind; the manifests state what is installed. |
| `licence`    |     | string | SPDX identifier. The field nobody wants until they urgently do.                                      |
| `decided-in` |     | id     | Where a decision was worth recording. Small, uncontroversial adoptions need only a register entry.   |
| `replaces`   |     | id     | The tool id this supersedes.                                                                         |
| `successor`  |     | id     | The tool id that replaces this one.                                                                  |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `approved` · `trial` · `deprecated` · `rejected`                    |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-tools -->

## Adding a tool

1. Copy [`_template.md`](tools/_template.md) to `<slug>.md`. Tools use slug ids — `tol-vitest`.
2. Set `status`. `trial` is for something being evaluated in one place; promote or reject it rather than leaving it
   there indefinitely.
3. Record the `licence` as an SPDX identifier. This is the field nobody wants until they urgently do.
4. Note what it replaces, if anything, so the deprecation path is visible.
5. Cite `decided-in` where an ADR exists. If the choice was contested and no ADR exists, that is a prompt to write one.

**Conventions**

* **Approved means approved for new work.** Something already in use but not approved is drift, and the drift report
  will say so.
* **Deprecated needs a successor** — mark what replaces it, or the entry is just a complaint.
* **Version ranges, not pinned versions.** The register states what we stand behind; the manifests state what is
  installed.

## What CI checks

<!-- BEGIN GENERATED: checks-tools -->

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
| `required-section`          | error   | Every required section heading is present.                                                                      |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                                  |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                        |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `deprecated-has-successor`  | warning | A deprecated tool names what replaces it, or the entry is just a complaint.                                     |
| `trial-has-criteria`        | warning | A tool in `trial` states what would decide it. A trial with no decision criteria stays a trial forever.         |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                      | What it would verify                                                                                                                                                                                       |
|---------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `drift-against-manifests` | Scheduled. Compares the register against package manifests across the organisation's repositories in both directions — packages in use that were never approved, and approved tools nothing uses any more. |

<!-- END GENERATED: checks-tools -->
