# Postmortems

What actually happened, and why.

**[→ Index](postmortems/_index.md)**

## What is a postmortem?

A blameless account of one incident: when it began, what customers lost, what caused it, what worked, and the actions it
left us with.

Publishing one closes it. A postmortem states what we understood at the time. Where we later understand the same
incident differently, we write another one.

## Why we use them

A standard says how a payment is meant to behave. A postmortem records what it did instead, and the gap between them
is what we did not know when we wrote the rule.

One incident routinely produces a [fix](fixes.md), a revised [NFR](nfrs.md) and sometimes a new clause in a
[standard](standards.md). A root cause that recurs shows up in no single account, so read several postmortems together
when you want to know what keeps breaking.

## Scope

**Blameless.** We write these to make the estate fail less often, and blame does not do that. Write the causal
statements about decisions, conditions, systems and roles: "the deploy ran before the migration completed", not
"X deployed too early".

Boundaries:

* **[NFR](nfrs.md)**: the target a service is meant to meet. A postmortem says what happened on a day it was not
  met.
* **[Fix](fixes.md)**: a reusable resolution, which an incident often produces as a by-product. A postmortem is the
  account of the incident itself.
* **A work item.** Each action is a GitHub issue. The postmortem links to it and tracks nothing itself.

Not every incident needs one. Use severity as the trigger and apply it the same way each time, so that the absence of a
postmortem means something.

## Metadata

<!-- BEGIN GENERATED: schema-postmortems -->

| Field           | Value                | Notes                                                                                                         |
|-----------------|----------------------|---------------------------------------------------------------------------------------------------------------|
| `id` *†         | string               | Stable, unique across the corpus, never reused, in the format the type sets.                                  |
| `type` *†       | string               | The singular name of the type, which CI checks against the folder.                                            |
| `tier` *†       | `decided`            | The record's trust level, fixed for the type and checked against the folder.                                  |
| `status` *†     | `draft` `published`  | `published` freezes the document. A new understanding is a new postmortem citing this one.                    |
| `owner` *†      | string               | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                                        |
| `sources` †     | list                 | Where this record's content came from, one entry per source.                                                  |
| `tags` †        | list                 | Free-form, lowercase and hyphenated. A reader searches on these across types.                                 |
| `occurred-at` * | timestamp            | The moment the incident began, in UTC as `2026-09-07T20:18:00Z`.                                              |
| `detected-at` * | timestamp            | The moment somebody noticed the incident, in UTC as `2026-09-07T20:18:00Z`.                                   |
| `restored-at`   | timestamp            | The moment service was back for users, in UTC as `2026-09-07T20:18:00Z`. Required when `status == published`. |
| `duration`      | string               | How long the incident lasted, as an ISO 8601 duration such as `PT4H20M`. Required when `status == published`. |
| `severity` *    | `sev1` `sev2` `sev3` | The severity the incident was handled at.                                                                     |
| `affected` *    | list                 | Service and offering ids the incident affected.                                                               |
| `prompted`      | list                 | The ADRs, runbooks, NFRs, fixes and standards this incident caused.                                           |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-postmortems -->

## Adding a postmortem

1. Copy [`_template.md`](postmortems/_template.md) to `NNNN-kebab-slug.md`, named for the symptom customers saw rather
   than for the cause.
2. Build the timeline from the evidence, before anyone starts theorising.
3. State the impact as a customer would describe it.
4. Say in **Resolution** what ended the impairment, and say so where nothing you did ended it.
5. Separate the root cause from the contributing factors. Name more than one cause where more than one stands out.
6. Fill in all three lessons. **What went well** and **What went wrong** are halves of one account, and **Where we
   got lucky** names a control you do not have.
7. Record each action as a link to its work item. Repair you have already done belongs in **Resolution**.
8. Fill `prompted` with whatever this incident caused someone to write.
9. Set `status: draft` while you assemble it, and `published` when it is finished.

**Conventions**

* **Immutable once published.** A new understanding is a new postmortem that references this one. Edit everything else
  in place: a status transition, a typo, a broken link, or a sentence that no longer matches the timeline. The commit
  message says what you changed.
* **Measure the impact against the [NFRs](nfrs.md)** where targets exist. Where the incident breached one, say which.
  Where no target existed, that absence is itself a finding.

## What CI checks

<!-- BEGIN GENERATED: checks-postmortems -->

| Check                          | Level   | What it verifies                                                                                                |
|--------------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`           | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`                  | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                    | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`               | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                     | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `empty-optional-key`           | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format`    | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `timestamp-format`             | error   | Timestamp fields name a moment the calendar has, in UTC: `YYYY-MM-DDThh:mm:ssZ`.                                |
| `enum`                         | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`                | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                   | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `type-matches-folder`          | error   | `type` matches the singular type name the record's folder declares.                                             |
| `tier-matches-type`            | error   | `tier` matches the tier the type declares.                                                                      |
| `id`                           | error   | `id` carries the type's prefix, takes the shape the type declares, and names the same document as the filename. |
| `id-unique`                    | error   | `id` is unique across the whole corpus.                                                                         |
| `filename / slug-length`       | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                           | error   | The document has an H1.                                                                                         |
| `identity`                     | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                     | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`             | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`                | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`              | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`              | error   | A link label is the id of the record it leads to, written as that record carries it.                            |
| `ref-resolves`                 | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `unused-definition`            | warning | A link definition that nothing references.                                                                      |
| `duration-matches-the-moments` | error   | `duration` is the span from `occurred-at` to `restored-at`.                                                     |
| `detected-not-before-occurred` | error   | `detected-at` is at or after `occurred-at`.                                                                     |
| `restored-not-before-occurred` | error   | `restored-at` is at or after `occurred-at`.                                                                     |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                        | What it would verify                                                                                             |
|-----------------------------|------------------------------------------------------------------------------------------------------------------|
| `immutable-after-published` | Once `status` is `published`, the account changes only by a new postmortem, never by an edit in place.           |
| `blameless`                 | Flags a personal name in the Timeline, Root cause or Contributing factors sections. Name the role or the system. |
| `recurring-root-causes`     | Scheduled. Reports root causes recurring across postmortems.                                                     |

<!-- END GENERATED: checks-postmortems -->
