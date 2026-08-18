# Postmortems

What actually happened, and why.

**[→ Index](postmortems/_index.md)**

## What is a postmortem?

A blameless account of one incident: when it began, what customers lost, what caused it, what worked, and the actions it
left us with.

Publishing one closes it, as with an [ADR](/adrs): a postmortem holds what we understood at the time. Where we later
understand the same incident differently, we write another one.

## Why we use them

The ADR log records what we intended. A postmortem records what the estate did instead, and the gap between the two is
what we did not know on the day we decided.

One incident routinely produces an [FAQ](/faqs), a [runbook](/runbooks), a revised [NFR](/nfrs) and sometimes an
[ADR](/adrs). A root cause that recurs shows up in no single account, so read several postmortems together when you want
to know what keeps breaking.

## Scope

**Blameless.** We write these to make the estate fail less often, and blame does not do that. Write the causal
statements about decisions, conditions, systems and roles — "the deploy ran before the migration completed", not
"X deployed too early".

Boundaries:

* **[Runbook](/runbooks)** — instructions for an incident that might happen. A postmortem is an account of one that did.
* **[FAQ](/faqs)** — a reusable fix, which an incident often produces as a by-product. A postmortem is the account of
  the incident itself.
* **A work item** — actions belong in ADO. The postmortem links to them and tracks nothing itself.

Not every incident needs one. Use severity as the trigger and apply it the same way each time, so that the absence of a
postmortem means something.

## Metadata

<!-- BEGIN GENERATED: schema-postmortems -->

| Field           | Type   | Notes                                                                                 |
|-----------------|--------|---------------------------------------------------------------------------------------|
| `id` *†         | string | Stable, unique across the corpus, never reused. Format set by the type.               |
| `tier` *†       | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.  |
| `status` *†     | enum   | `published` freezes the document; a new understanding is a new postmortem.            |
| `owner` *†      | string | A named person, never a team alias.                                                   |
| `tags` †        | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                      |
| `occurred-on` * | date   | Quoted. When the incident began, not when it was noticed.                             |
| `detected-on` * | date   | Separate from `occurred-on` for a reason — the gap between them is often the finding. |
| `duration` *    | string | How long it lasted, in whatever unit reads honestly.                                  |
| `severity` *    | enum   | The severity it was handled at.                                                       |
| `affected` *    | list   | Service and capability ids that suffered.                                             |
| `prompted`      | list   | What this incident caused to be written.                                              |

**Enum values**

| Field      | Values                                                              |
|------------|---------------------------------------------------------------------|
| `tier`     | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`   | `draft` · `published`                                               |
| `severity` | `sev1` · `sev2` · `sev3`                                            |

\* Field is required  
† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-postmortems -->

## Adding a postmortem

1. Copy [`_template.md`](postmortems/_template.md) to `NNNN-kebab-slug.md`, named for the symptom customers saw rather
   than for the cause.
2. Build the timeline from the evidence, before anyone starts theorising.
3. State the impact as a customer would describe it.
4. Separate the root cause from the contributing factors. There is usually one cause and several factors.
5. Fill in **What went well**. An account listing only failures teaches half the lesson.
6. Record each action as a link to its work item.
7. Fill `prompted` with whatever this incident caused someone to write.
8. Set `status: draft` while you assemble it, and `published` when it is finished.

**Conventions**

* **Immutable once published.** Corrections are limited to typos, and a materially different understanding is a new
  postmortem that references this one.
* **Measure the impact against the [NFRs](/nfrs)** where targets exist. Where the incident breached one, say which.
  Where no target existed, that absence is itself a finding.

## What CI checks

<!-- BEGIN GENERATED: checks-postmortems -->

| Check                          | Level   | What it verifies                                                                                                |
|--------------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`           | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`                  | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                    | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`               | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                     | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format`    | error   | Date fields are quoted, and name a day the calendar has — `YYYY-MM-DD`.                                         |
| `enum`                         | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`                | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                   | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `tier-matches-type`            | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                           | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                    | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`       | error   | Filename matches the pattern; the slug is within 30 characters.                                                 |
| `h1`                           | error   | The document has an H1.                                                                                         |
| `identity`                     | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                     | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`             | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`                | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`              | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`              | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`                 | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`            | warning | A link definition that nothing references.                                                                      |
| `detected-not-before-occurred` | error   | `detected-on` is on or after `occurred-on`.                                                                     |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                        | What it would verify                                                                                                   |
|-----------------------------|------------------------------------------------------------------------------------------------------------------------|
| `immutable-after-published` | Once status is `published`, content changes are limited to typo fixes. Same rule as ADRs.                              |
| `blameless`                 | Flags personal names inside the Timeline, Root cause and Contributing factors sections. Roles and systems, not people. |
| `recurring-root-causes`     | Scheduled. Reports root causes recurring across postmortems — the highest-signal output in the corpus.                 |

<!-- END GENERATED: checks-postmortems -->
