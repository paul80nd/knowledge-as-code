# Runbooks

What to do when something is broken.

**[→ Index](runbooks/_index.md)**

## What is a runbook?

An incident-time procedure, read under pressure by someone who may not have seen this failure before. Symptoms first,
then the immediate actions, then a diagnosis tree, then resolution and escalation.

Keep it terse and imperative, and structure it so the reader finds their branch without reading the whole document.

## Why we use them

At 2am nobody reconstructs a recovery sequence from first principles, and the person who knows it is asleep. A runbook
is the difference between a twenty-minute incident and a two-hour one.

Writing one also exposes the step nobody has tried. A disaster-recovery runbook carrying `last-rehearsed: "never"` says
the recovery has never been run, while there is still time to run it.

## Scope

**Broken, not planned.** If you are doing this because you decided to, it is a [process](processes.md).

Two other types sit close enough to confuse:

* **[FAQ](faqs.md)**: a known problem with a known fix, usually one or two steps, no urgency. If it needs a diagnosis
  tree and an escalation path, it is a runbook.
* **[Postmortem](postmortems.md)**: an account of an incident that happened. A runbook gives instructions for an
  incident that has not happened yet. A good postmortem frequently produces a runbook.

Disaster recovery and rebuilding the estate from nothing belong here. You rehearse them deliberately, which makes them
look like processes. You open the document on a day when the estate is already down.

## Metadata

<!-- BEGIN GENERATED: schema-runbooks -->

| Field                 | Value                              | Notes                                                                                   |
|-----------------------|------------------------------------|-----------------------------------------------------------------------------------------|
| `id` *†               | string                             | Stable, unique across the corpus, never reused. Format set by the type.                 |
| `tier` *†             | `procedural`                       | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.     |
| `status` *†           | `active` `draft` `retired`         | Whether the runbook is current, drafted, or stood down.                                 |
| `owner` *†            | string                             | A named person, never a team alias.                                                     |
| `tags` †              | list                               | Free-form, lowercase, hyphenated. Used for cross-cutting search.                        |
| `applies-to`          | list                               | Service ids this runbook covers.                                                        |
| `severity`            | `sev1` `sev2` `sev3`               | The severity this runbook is written for.                                               |
| `last-rehearsed` *    | date                               | `"never"` is permitted, and is worth knowing before the incident rather than during it. |
| `rehearsal-frequency` | `per-release` `quarterly` `annual` | How often it should be exercised.                                                       |
| `requires-access`     | list                               | Must be complete. Discovering you lack a permission mid-incident is its own outage.     |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-runbooks -->

## Adding a runbook

1. Copy [`_template.md`](runbooks/_template.md) to `<slug>.md`. Runbooks use slug ids: `rbk-estate-rebuild`.
2. Lead with **symptoms**: what the reader is seeing. That is how they find this document.
3. Give the immediate actions before the diagnosis. Stop the bleeding, then work out why.
4. Structure the diagnosis as a tree, not prose. Each branch ends in a resolution or an escalation.
5. Put the escalation path where the reader finds it without scrolling.
6. Set `last-rehearsed` honestly, and name every permission the runbook needs in `requires-access`.

**Conventions**

* **Short sentences, imperative mood.** No background, no rationale. Link to an [explanation](explanations.md) where the
  reader wants the theory afterwards.
* **No prerequisite the reader cannot satisfy at 2am.** Where a step needs someone else's approval, name who and how to
  reach them.
* **Rehearse on a schedule.** `rehearsal-frequency` says how often, and `last-rehearsed` records the last time someone
  did.

## What CI checks

<!-- BEGIN GENERATED: checks-runbooks -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
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
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `symptoms-first`            | error   | Symptoms is the first section after the H1. That is how the reader finds the document.                          |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                  | What it would verify                                                            |
|-----------------------|---------------------------------------------------------------------------------|
| `escalation-required` | Every diagnosis branch ends in a resolution or an escalation, never a dead end. |
| `staleness-loud`      | Rehearsal staleness, reported more prominently than a process's.                |

<!-- END GENERATED: checks-runbooks -->
