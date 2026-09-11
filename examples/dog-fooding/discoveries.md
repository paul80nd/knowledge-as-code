# Discoveries

Things we noticed and haven't verified yet.

**[→ Index](discoveries/_index.md)**

## What is a discovery?

A short, unreviewed note of something you saw while working here. *"A rule this type declares has never run against a
record."* *"The IDE ignores an `.editorconfig` edited from a shell."* It may be wrong, already fixed, or true only of
the branch you were on.

Deliberately low-ceremony: a title, what you saw, the context you were in, and why it might matter. Nothing more.

## Why we use them

Capture has to be nearly free or it doesn't happen. Nobody writes up a gotcha that costs them a template, an owner and
two reviewers, so a discovery asks for none of the three. A change here still arrives as a pull request, and **nobody
has to agree the observation is true** before it lands. It expires on its own unless someone promotes it.

A human adds the rigour at promotion. The corpus can then take in everything anyone notices while the documents that
carry authority stay few and checked.

Most of what lands here comes out of an agent session, because that is where most of the work in this repository
happens. A session ends and its findings would go with it, so `source` and `provenance` say which session saw what.

## Scope

Discoveries are **perishable and carry no authority**. They expire after 90 days by default, and the short life is
deliberate: an observation nobody has needed in three months was probably situational.

Boundaries:

* **[A fix](fixes.md)**: checked, general, current, and carries authority. That is what a discovery here is promoted
  *to* where the answer is a resolution. Where the answer is a rule people must follow, it becomes a standard instead.
* **Session state**: where a piece of work got to. That is personal handover and is **not stored in this repository**.
* **A bug.** Where something is broken and somebody should fix it, raise a GitHub issue. A discovery records something
  surprising, and claims nothing is owed.

## Metadata

<!-- BEGIN GENERATED: schema-discoveries -->

| Field          | Value                                   | Notes                                                                                                              |
|----------------|-----------------------------------------|--------------------------------------------------------------------------------------------------------------------|
| `id` *†        | string                                  | Stable, unique across the corpus, never reused, in the format the type sets.                                       |
| `type` *†      | string                                  | The singular name of the type, which CI checks against the folder.                                                 |
| `tier` *†      | `observed`                              | The record's trust level, fixed for the type and checked against the folder.                                       |
| `status` *†    | `open` `promoted` `expired` `rejected`  | Open until promoted, expired or rejected.                                                                          |
| `owner` *†     | string                                  | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                                             |
| `sources` †    | list                                    | Where the content came from, one entry per source.                                                                 |
| `tags` †       | list                                    | Free-form, lowercase and hyphenated. A reader searches on these across types.                                      |
| `source` *     | `human` `session` `dreamed`             | Who or what observed it. `dreamed` means an agent proposed it.                                                     |
| `confidence` * | `unverified` `corroborated` `confirmed` | Starts at `unverified`, and moves only when somebody proves it.                                                    |
| `expires` *    | date                                    | Quoted. The day the observation lapses, ninety days from capture.                                                  |
| `provenance`   | string                                  | A reference back to the session and passage, so a reviewer can check the claim. Required when `source == dreamed`. |
| `applies-to`   | list                                    | Service ids this observation concerns.                                                                             |
| `promoted-to`  | id                                      | The fix or standard this became. Required when `status == promoted`.                                               |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-discoveries -->

## Capturing a discovery

Low ceremony on purpose. Copy [`_template.md`](discoveries/_template.md) and fill in a title, what you saw, the context
you were in, and why it might matter. Leave `confidence: unverified` unless you've genuinely proven it. Where a session
wrote it, fill `provenance` too: the reader has to be able to check the observation without taking your word for it.
Don't tidy it up and don't verify it first.

Check the claim against the repository before you write it down. This corpus describes the repository it sits in, so an
observation here names a real file, a real run or a real command.

## Promoting a discovery

The one flow that crosses tiers. A resolution somebody has checked becomes a [fix](fixes.md). A rule people must follow
becomes a standard instead.

1. A human confirms the observation is real, general, and still current.
2. Write the record it becomes.
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
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""`, `—` or an unquoted `{{…}}`.                             |
| `empty-optional-key`        | warning | An optional field is filled in or left out, rather than written with no value.                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has: `YYYY-MM-DD`.                                          |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                         |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                    |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                      |
| `type-matches-folder`       | error   | `type` matches the singular type name the record's folder declares.                                             |
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
| `label-canonical`           | error   | A shortcut label is the id of the record it leads to, written as that record carries it.                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists, of the type the field names.           |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `low-ceremony`              | warning | A discovery stays within the length a capture needs.                                                            |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule           | What it would verify                                                                               |
|----------------|----------------------------------------------------------------------------------------------------|
| `expiry-sweep` | Scheduled. A discovery past `expires` with no promotion is set to `expired` with a note, and kept. |

<!-- END GENERATED: checks-discoveries -->
