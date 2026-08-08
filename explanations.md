# Explanations

Narrative documents that help you *understand* how the platform works and why it is shaped the way it is.

**[→ Index](explanations/_index.md)**

## What is an explanation?

An explanation is prose whose job is comprehension rather than instruction. An architecture overview, a walkthrough of
how a request flows through the estate, an account of why the testing approach is structured as it is. It is the
document you read to build a mental model, not the one you consult mid-task.

Every other type in this wiki answers a narrow question — what was decided, what must I do, how do I do it, what is this
component. Explanations answer "how does this hang together", which none of the others can without becoming something
they shouldn't.

## Why we use them

Without a home for narrative, this kind of content either doesn't get written or accumulates at the top level with no
owner and no review date — which is how it goes stale. Typing it means an explanation has an owner and a `review-by`
like anything else.

## Scope

An explanation is **not**:

* **Normative** — if it says what you must do, it is a [standard](/standards).
* **Procedural** — if it says how to perform a task, it is a [process](/processes) or a
  [runbook](/runbooks).
* **A catalogue entry** — if it describes one component, it is a [service](/services).
* **A decision** — if it records what was chosen and why, it is an [ADR](/adrs).

**Explanations link rather than restate.** An architecture overview points at the services, capabilities and ADRs that
hold the detail; it does not duplicate them. An explanation that starts accumulating facts of its own has become a
maintenance liability, and its facts will be the ones that go stale first.

If a document could plausibly be an explanation *or* something else, it is the something else. This type is the
residual, and residual categories become dumping grounds unless the bar for entry is kept high.

## Metadata

<!-- BEGIN GENERATED: schema-explanations -->

| Field       | Req | Type   | Notes                                                                                         |
|-------------|-----|--------|-----------------------------------------------------------------------------------------------|
| `id` †      | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                         |
| `tier` †    | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.          |
| `status` †  | ●   | enum   | `stale` is an honest state — say so rather than let the page quietly rot.                     |
| `owner` †   | ●   | string | A named person, never a team alias.                                                           |
| `tags` †    |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                              |
| `explains`  | ●   | list   | Service or capability ids this explains.                                                      |
| `review-by` | ●   | date   | The field that stops this type rotting — explanations need the tightest staleness discipline. |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `active` · `stale`                                        |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-explanations -->

## Adding an explanation

1. Check it isn't one of the four things above.
2. Copy [`_template.md`](explanations/_template.md) to a kebab-case filename — no number prefix; explanations are named,
   not sequenced.
3. Set `explains` to the services or capabilities it covers, and `review-by`.
4. Write it as prose. Link out for every concrete fact you're tempted to state.

## What CI checks

<!-- BEGIN GENERATED: checks-explanations -->

| Check                        | Level   | What it verifies                                                                                                                                                                                               |
|------------------------------|---------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`         | error   | Frontmatter is present and is a valid YAML mapping.                                                                                                                                                            |
| `unknown-key`                | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                                                                                                                 |
| `key-order`                  | error   | Key order is a topological extension of the schema's field order.                                                                                                                                              |
| `required-field`             | error   | Required and conditionally-required fields are present.                                                                                                                                                        |
| `bare-key`                   | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                                                                                                                 |
| `date-quoted / date-format`  | error   | Date fields are quoted `YYYY-MM-DD`.                                                                                                                                                                           |
| `enum`                       | error   | Enum values are in range and lowercase.                                                                                                                                                                        |
| `field-pattern`              | error   | Values match the pattern their field declares (e.g. `tags`).                                                                                                                                                   |
| `list-order`                 | warning | List entries read in alphabetical order, with numbers compared as numbers.                                                                                                                                     |
| `tier-matches-type`          | error   | `tier` matches the tier the type declares.                                                                                                                                                                     |
| `id`                         | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                                                                                                                                  |
| `id-unique`                  | error   | `id` is unique across the whole wiki.                                                                                                                                                                          |
| `filename / slug-length`     | error   | Filename matches the pattern; the slug is within 30 characters.                                                                                                                                                |
| `h1`                         | error   | The document has an H1.                                                                                                                                                                                        |
| `identity`                   | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.                                                                                                       |
| `required-section`           | error   | Every required section heading is present.                                                                                                                                                                     |
| `link-resolves`              | error   | Every internal link resolves (all link forms, `.md` optional).                                                                                                                                                 |
| `undefined-label`            | error   | Every shortcut reference has a link definition.                                                                                                                                                                |
| `label-canonical`            | error   | A shortcut label that names a document is written as that document's id.                                                                                                                                       |
| `unused-definition`          | warning | A link definition that nothing references.                                                                                                                                                                     |
| `links-rather-than-restates` | warning | Reports an explanation whose prose-to-link ratio exceeds a threshold. An explanation that accumulates facts of its own has become a maintenance liability, and its facts will be the ones that go stale first. |
| `not-normative`              | warning | Flags RFC 2119 keywords (MUST, MUST NOT, SHOULD, SHOULD NOT, MAY) in bold. Normative content belongs in a standard; the explanation should link to it.                                                         |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule               | What it would verify                                                                                                                                                     |
|--------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `not-load-bearing` | Reports where a decided-tier document (ADR or postmortem) cites an explanation as the authority for a constraint. Immutable records should not derive from mutable ones. |

<!-- END GENERATED: checks-explanations -->
