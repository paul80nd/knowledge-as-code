# Discoveries

Things we noticed and haven't verified yet.

**[→ Index](discoveries/INDEX.md)**

## What is a discovery?

A short, unreviewed note of something observed during work. *"The build fails silently if X."* *"The legacy API returns
200 with an empty body when Y."* Possibly wrong, possibly already fixed, possibly situational.

Deliberately low-ceremony: a title, what you saw, the context you were in, and why it might matter. Nothing more.

## Why we use them

Capture has to be nearly free or it doesn't happen. Nobody writes up a gotcha if doing so requires a template, an owner
and two reviewers — so observations are recorded with **no review at all**, marked unverified, and expire on their own
if nothing promotes them.

The rigour lives at promotion, not capture. That gradient — cheap in, deliberate out — is what lets the corpus grow
without the average trustworthiness falling.

This is also where AI sessions contribute. A session that discovers something useful has somewhere to put it, and the
discovery outlives the session.

## Scope

Discoveries are **perishable and carry no authority**. They expire after 90 days by default, and that is a feature: an
observation nobody has needed in three months was probably situational.

Boundaries:

* **[FAQ](/faqs)** — confirmed, general, current, and carries authority. That is what a discovery is promoted *to*.
* **Session state** — where a piece of work got to. That is personal handover and is **not stored in this repository**.
* **A bug** — if it is broken and should be fixed, raise a work item. A discovery records something surprising, not
  something owed.

## Metadata

<!-- BEGIN GENERATED: schema-discoveries -->

| Field         | Req | Type   | Notes                                                                                             |
|---------------|-----|--------|---------------------------------------------------------------------------------------------------|
| `id` †        | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                             |
| `tier` †      | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.              |
| `status` †    | ●   | enum   | Open until promoted, expired or rejected.                                                         |
| `owner` †     | ●   | string | A named person, never a team alias.                                                               |
| `tags` †      |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                  |
| `source`      | ●   | enum   | Who or what observed it. `dreamed` means proposed by an agent.                                    |
| `confidence`  | ●   | enum   | Stays `unverified` unless genuinely proven. An agent cannot confirm its own observation.          |
| `expires`     | ●   | date   | Perishable by default. An observation nobody has needed in three months was probably situational. |
| `provenance`  |     | string | A reference back to the session and passage, so review is a check rather than an act of faith.    |
| `applies-to`  |     | list   | Service ids this observation concerns.                                                            |
| `promoted-to` |     | id     | The FAQ or standard this became.                                                                  |

**Enum values**

| Field        | Values                                                              |
|--------------|---------------------------------------------------------------------|
| `tier`       | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`     | `open` · `promoted` · `expired` · `rejected`                        |
| `source`     | `human` · `session` · `dreamed`                                     |
| `confidence` | `unverified` · `corroborated` · `confirmed`                         |

**Conditionally required**

| Field         | Required when        |
|---------------|----------------------|
| `provenance`  | `source == dreamed`  |
| `promoted-to` | `status == promoted` |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-discoveries -->

## Capturing a discovery

Low ceremony on purpose. Copy [`template.md`](discoveries/template.md) and fill in a title, what you observed, why it
might matter, and the context you were in. Set `confidence: unverified` unless you've genuinely proven it. Don't tidy it
up; don't verify it first; don't write it as an FAQ.

Discoveries expire after 90 days by default. That's a feature — an observation nobody has needed in three months was
probably situational.

## Promoting a discovery to an FAQ

The one flow that crosses tiers, and the one worth getting right.

1. A human confirms the observation is real, general, and still current.
2. Create the FAQ with `promoted-from`, `confirmed-by` and `confirmed-on`.
3. Set the discovery's `status: promoted` and `promoted-to`.
4. If the underlying issue is actually a rule people should follow, the promotion target is a **standard**, not an FAQ —
   and that needs an ADR first.

Promotions proposed automatically arrive as PRs carrying `provenance` back to the passage that produced them. Review
that provenance; it's the whole reason the field exists. An unverifiable proposal is a rejected proposal.

_(The automatic half is not built yet — see [Automation](/knowledge-as-code/automation.md).)_

## What CI checks

<!-- BEGIN GENERATED: checks-discoveries -->

| Check                       | Level   | What it verifies                                                                       |
|-----------------------------|---------|----------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                    |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                         |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                      |
| `required-field`            | error   | Required and conditionally-required fields are present.                                |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                         |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                   |
| `enum`                      | error   | Enum values are in range and lowercase.                                                |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                           |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.             |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                             |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.          |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                  |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                        |
| `h1`                        | error   | The document has an H1 and, where the type declares one, it matches the title pattern. |
| `required-section`          | error   | Every required section heading is present.                                             |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                         |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                        |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.               |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                       |
| `unused-definition`         | warning | A link definition that nothing references.                                             |

<!-- END GENERATED: checks-discoveries -->
