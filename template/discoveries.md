# Discoveries

Things we noticed and haven't verified yet.

**[→ Index](discoveries/_index.md)**

## What is a discovery?

A short, unreviewed note of something you saw while working. *"The build fails silently if X."* *"The legacy API returns
200 with an empty body when Y."* It may be wrong, already fixed, or true only of the branch you were on.

Deliberately low-ceremony: a title, what you saw, the context you were in, and why it might matter. Nothing more.

## Why we use them

Capture has to be nearly free or it doesn't happen. Nobody writes up a gotcha that costs them a template, an owner and
two reviewers, so a discovery asks for none of the three. You write what you saw, mark it unverified, and get **no
review at all**. It expires on its own unless someone promotes it.

A human adds the rigour at promotion. The corpus can then take in everything anyone notices while the documents that
carry authority stay few and checked.

AI sessions contribute here too. What a session works out has somewhere to go, and the note outlives the session.

## Scope

Discoveries are **perishable and carry no authority**. They expire after 90 days by default, and the short life is
deliberate: an observation nobody has needed in three months was probably situational.

Boundaries:

* **[FAQ](faqs.md)**: confirmed, general, current, and carries authority. That is what a discovery is promoted *to*.
* **Session state**: where a piece of work got to. That is personal handover and is **not stored in this repository**.
* **A bug.** If it is broken and should be fixed, raise a work item. A discovery records something surprising, not
  something owed.

## Metadata

<!-- BEGIN GENERATED: schema-discoveries -->

| Field          | Value                                   | Notes                                                                                                                             |
|----------------|-----------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| `id` *†        | string                                  | Stable, unique across the corpus, never reused. Format set by the type.                                                           |
| `tier` *†      | `observed`                              | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.                                               |
| `status` *†    | `open` `promoted` `expired` `rejected`  | Open until promoted, expired or rejected.                                                                                         |
| `owner` *†     | string                                  | A named person, never a team alias.                                                                                               |
| `tags` †       | list                                    | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                                                  |
| `source` *     | `human` `session` `dreamed`             | Who or what observed it. `dreamed` means proposed by an agent.                                                                    |
| `confidence` * | `unverified` `corroborated` `confirmed` | Starts at `unverified`, and stays there unless genuinely proven.                                                                  |
| `expires` *    | date                                    | Quoted. Ninety days from capture, a convention the template carries.                                                              |
| `provenance`   | string                                  | A reference back to the session and passage, so review is a check rather than an act of faith. Required when `source == dreamed`. |
| `applies-to`   | list                                    | Service ids this observation concerns.                                                                                            |
| `promoted-to`  | id                                      | The FAQ or standard this became. Required when `status == promoted`.                                                              |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-discoveries -->

## Capturing a discovery

Low ceremony on purpose. Copy [`_template.md`](discoveries/_template.md) and fill in a title, what you saw, the context
you were in, and why it might matter. Leave `confidence: unverified` unless you've genuinely proven it. Don't tidy it
up, don't verify it first, and don't write it as an FAQ.

## Promoting a discovery to an FAQ

The one flow that crosses tiers. Where the observation turns out to be a rule people should follow, promote it to a
**standard** instead, and that needs an ADR first.

1. A human confirms the observation is real, general, and still current.
2. Create the FAQ with `promoted-from`, `confirmed-by` and `confirmed-on`.
3. Set the discovery's `status: promoted` and `promoted-to`.

Nothing proposes a promotion automatically
yet. [Automation](https://paul80nd.github.io/knowledge-as-code/framework/automation/) describes the distillation pass
that would. Such a proposal arrives as a pull request carrying `provenance` back to the passage that produced it. Read
that provenance before you accept anything: an unverifiable proposal is a rejected proposal. Checking it is the whole
reason the field exists.

## What CI checks

<!-- BEGIN GENERATED: checks-discoveries -->

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
| `filename / slug-length`    | error   | Filename matches the pattern. The slug is within 30 characters.                                                 |
| `h1`                        | error   | The document has an H1.                                                                                         |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.        |
| `sections`                  | error   | Every required section heading is present, and no declared section is left as a bare heading.                   |
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all forms, `.md` optional), and a `#fragment` names a heading there.              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `low-ceremony`              | warning | A discovery stays within the length its tier is for.                                                            |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule           | What it would verify                                                                                   |
|----------------|--------------------------------------------------------------------------------------------------------|
| `expiry-sweep` | Scheduled. Discoveries past `expires` with no promotion are set to `expired` with a note, not deleted. |

<!-- END GENERATED: checks-discoveries -->
