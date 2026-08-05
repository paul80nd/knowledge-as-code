# Runbooks

What to do when something is broken.

**[→ Index](runbooks/INDEX.md)**

## What is a runbook?

An incident-time procedure, read under pressure by someone who may not have seen this failure before. Symptoms first,
then immediate actions, then a diagnosis tree, then resolution and escalation.

Terse, imperative, and structured so the reader can find their branch without reading the whole document.

## Why we use them

At 2am nobody reconstructs a recovery sequence from first principles, and the person who knows it is asleep. A runbook
is the difference between a twenty-minute incident and a two-hour one.

They also make the untested assumption visible. `last-rehearsed: "never"` on a disaster-recovery runbook is a much more
useful thing to know before the disaster than after it.

## Scope

**Broken, not planned.** If you are doing it because you decided to, it is a [process](/processes).

Runbooks sit next to two other types and the boundaries are worth holding:

* **[FAQ](/faqs)** — a known problem with a known fix, usually one or two steps, no urgency. If it needs a diagnosis
  tree and an escalation path, it is a runbook.
* **[Postmortem](/postmortems)** — an account of an incident that happened. A runbook is instructions for one that
  might. A good postmortem frequently produces a runbook.

Disaster recovery and "ground-zero the estate" belong here, not in processes, however planned the rehearsal is — the
document is written for the day it isn't.

## Metadata

<!-- BEGIN GENERATED: schema-runbooks -->

| Field                 | Req | Type   | Notes                                                                                   |
|-----------------------|-----|--------|-----------------------------------------------------------------------------------------|
| `id` †                | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                   |
| `tier` †              | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.    |
| `status` †            | ●   | enum   | Whether the runbook is current, drafted, or stood down.                                 |
| `owner` †             | ●   | string | A named person, never a team alias.                                                     |
| `tags` †              |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                        |
| `applies-to`          |     | list   | Service ids this runbook covers.                                                        |
| `severity`            |     | enum   | The severity this runbook is written for.                                               |
| `last-rehearsed`      | ●   | date   | `"never"` is permitted, and is worth knowing before the incident rather than during it. |
| `rehearsal-frequency` |     | enum   | How often it should be exercised.                                                       |
| `requires-access`     |     | list   | Must be complete. Discovering you lack a permission mid-incident is its own outage.     |

**Enum values**

| Field                 | Values                                                              |
|-----------------------|---------------------------------------------------------------------|
| `tier`                | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`              | `active` · `draft` · `retired`                                      |
| `severity`            | `sev1` · `sev2` · `sev3`                                            |
| `rehearsal-frequency` | `per-release` · `quarterly` · `annual`                              |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-runbooks -->

## Adding a runbook

1. Copy [`template.md`](runbooks/template.md) to `<slug>.md`. Runbooks use slug ids — `rbk-estate-rebuild`.
2. Lead with **symptoms** — what the reader is seeing. That is how they will find this document.
3. Give immediate actions before diagnosis. Stop the bleeding, then work out why.
4. Structure the diagnosis as a tree, not prose. Each branch ends in a resolution or an escalation.
5. Put the escalation path where it can be found without scrolling.
6. Set `last-rehearsed` honestly, and `requires-access` completely — discovering you lack a permission mid-incident is
   its own outage.

**Conventions**

* **Short sentences, imperative mood.** No background, no rationale. Link to the [explanation](/explanations) if the
  reader needs the theory afterwards.
* **No prerequisites the reader can't satisfy at 2am.** If a step needs someone else's approval, say who and how to
  reach them.
* **Rehearse on a schedule.** An unrehearsed runbook is flagged by the staleness report, loudly, and that is deliberate.

## What CI checks

<!-- BEGIN GENERATED: checks-runbooks -->

| Check                       | Level   | What it verifies                                                                                   |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                     |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                  |
| `required-field`            | error   | Required and conditionally-required fields are present.                                            |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                     |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                               |
| `enum`                      | error   | Enum values are in range and lowercase.                                                            |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                       |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                         |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                         |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                      |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                              |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                    |
| `h1`                        | error   | The document has an H1 matching the title pattern, opening with its id where the type carries one. |
| `required-section`          | error   | Every required section heading is present.                                                         |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                     |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                    |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                           |
| `unused-definition`         | warning | A link definition that nothing references.                                                         |

<!-- END GENERATED: checks-runbooks -->
