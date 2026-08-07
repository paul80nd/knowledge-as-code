# Postmortems

What actually happened, and why.

**[→ Index](postmortems/INDEX.md)**

## What is a postmortem?

A blameless account of an incident: the timeline, the impact, the root cause, the contributing factors, what went well,
and the actions that came out of it.

Immutable once published, like an [ADR](/adrs) — a postmortem is a record of what was understood at the time. New
understanding produces a new document, not a rewrite.

## Why we use them

The ADR log records what we intended. Postmortems record what the estate did about it — and the gap between the two is
where most real learning lives.

They are also the richest source of other knowledge in the corpus. A single incident routinely produces an
[FAQ](/faqs), a [runbook](/runbooks), a revised [NFR](/nfrs) and occasionally an [ADR](/adrs). And the pattern across
several postmortems — the recurring root cause nobody noticed was recurring — is the highest-signal thing this wiki can
tell you.

## Scope

**Blameless, always.** The output is a system that fails less, not a person who feels worse. Write about decisions and
conditions, not individuals — "the deploy ran before the migration completed", not "X deployed too early".

Boundaries:

* **[Runbook](/runbooks)** — instructions for an incident that might happen. A postmortem is an account of one that did.
* **[FAQ](/faqs)** — a reusable fix. A postmortem is a specific narrative, and often produces an FAQ as a by-product.
* **A work item** — actions belong in ADO. The postmortem links to them; it is not a tracker.

Not every incident needs one. Use severity as the trigger and be consistent about it, so the absence of a postmortem
means something.

## Metadata

<!-- BEGIN GENERATED: schema-postmortems -->

| Field         | Req | Type   | Notes                                                                                 |
|---------------|-----|--------|---------------------------------------------------------------------------------------|
| `id` †        | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                 |
| `tier` †      | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.  |
| `status` †    | ●   | enum   | `published` freezes the document; a new understanding is a new postmortem.            |
| `owner` †     | ●   | string | A named person, never a team alias.                                                   |
| `tags` †      |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                      |
| `occurred-on` | ●   | date   | Quoted. When the incident began, not when it was noticed.                             |
| `detected-on` | ●   | date   | Separate from `occurred-on` for a reason — the gap between them is often the finding. |
| `duration`    | ●   | string | How long it lasted, in whatever unit reads honestly.                                  |
| `severity`    | ●   | enum   | The severity it was handled at.                                                       |
| `affected`    | ●   | list   | Service and capability ids that suffered.                                             |
| `prompted`    |     | list   | What this incident caused to be written.                                              |

**Enum values**

| Field      | Values                                                              |
|------------|---------------------------------------------------------------------|
| `tier`     | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`   | `draft` · `published`                                               |
| `severity` | `sev1` · `sev2` · `sev3`                                            |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-postmortems -->

## Adding a postmortem

1. Copy [`template.md`](postmortems/template.md) to `NNNN-kebab-slug.md`, named for the symptom rather than the cause.
2. Write the timeline first, from the evidence, before anyone theorises. `occurred-on` and `detected-on` are separate
   fields for a reason — the gap between them is often the finding.
3. State the impact in customer terms, not system terms.
4. Separate root cause from contributing factors. There is usually one of the first and several of the second.
5. Include **what went well**. A postmortem that only lists failures teaches half the lesson.
6. Record actions as links to work items, and fill `prompted` with anything this incident caused to be written.
7. `status: draft` while it is being assembled; `published` freezes it.

**Conventions**

* **Immutable once published.** Corrections are limited to typos. A materially different understanding is a new
  postmortem that references this one.
* **No names in causal statements.** Roles and systems, not people.
* **Measure against the [NFRs](/nfrs)** where they exist. If the incident breached a target, say so; if there was no
  target, that is itself a finding.

## What CI checks

<!-- BEGIN GENERATED: checks-postmortems -->

| Check                          | Level   | What it verifies                                                                                         |
|--------------------------------|---------|----------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`           | error   | Frontmatter is present and is a valid YAML mapping.                                                      |
| `unknown-key`                  | error   | Every frontmatter key is a schema field or a reserved ADO key.                                           |
| `key-order`                    | error   | Key order is a topological extension of the schema's field order.                                        |
| `required-field`               | error   | Required and conditionally-required fields are present.                                                  |
| `bare-key`                     | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                           |
| `date-quoted / date-format`    | error   | Date fields are quoted `YYYY-MM-DD`.                                                                     |
| `enum`                         | error   | Enum values are in range and lowercase.                                                                  |
| `field-pattern`                | error   | Values match the pattern their field declares (e.g. `tags`).                                             |
| `list-order`                   | warning | List entries read in alphabetical order, with numbers compared as numbers.                               |
| `tier-matches-type`            | error   | `tier` matches the tier the type declares.                                                               |
| `id`                           | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                            |
| `id-unique`                    | error   | `id` is unique across the whole wiki.                                                                    |
| `filename / slug-length`       | error   | Filename matches the pattern; the slug is within 30 characters.                                          |
| `h1`                           | error   | The document has an H1.                                                                                  |
| `identity`                     | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter. |
| `required-section`             | error   | Every required section heading is present.                                                               |
| `link-resolves`                | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`              | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`              | error   | A shortcut label that names a document is written as that document's id.                                 |
| `unused-definition`            | warning | A link definition that nothing references.                                                               |
| `detected-not-before-occurred` | error   | `detected-on` is on or after `occurred-on`.                                                              |

<!-- END GENERATED: checks-postmortems -->
