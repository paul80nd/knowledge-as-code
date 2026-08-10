# Processes

How to perform the planned tasks that keep the platform running.

**[→ Index](processes/_index.md)**

## What is a process?

A procedure you follow **deliberately**, because you decided to: onboarding a new developer, cutting a release,
provisioning an environment, rotating a secret. Numbered steps, prerequisites, a verification step and a way back.

## Why we use them

Procedural knowledge is the kind most reliably held in one person's head, and the kind most expensive to lose. Writing
it down is also the only way to find out that a step everyone "just knows" stopped working three months ago.

Processes carry `last-rehearsed` for exactly that reason. A procedure nobody has walked through is a hypothesis.

## Scope

The distinction that matters: **are you doing this because you planned to, or because something is broken?**

* Planned — a process.
* Broken — a [runbook](/runbooks).

Different audiences, different tone, different consequences when they go stale. A process that is slightly out of date
is annoying; a runbook that is slightly out of date is dangerous.

A process is also not:

* **A rule** — "deployments happen in dependency order" is a [standard](/standards); the release process cites it.
* **A reference list** — system requirements and port tables are [service](/services) catalogue content, not steps. A
  document with no steps in it is not a process.
* **An explanation** — how the pipeline works is an [explanation](/explanations); how to use it is a process.

## Metadata

<!-- BEGIN GENERATED: schema-processes -->

| Field                 | Req | Type   | Notes                                                                                          |
|-----------------------|-----|--------|------------------------------------------------------------------------------------------------|
| `id` †                | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                          |
| `tier` †              | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.           |
| `status` †            | ●   | enum   | Whether the process is current, drafted, or stood down.                                        |
| `owner` †             | ●   | string | A named person, never a team alias.                                                            |
| `tags` †              |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                               |
| `applies-to`          |     | list   | Service ids this process concerns.                                                             |
| `last-rehearsed`      | ●   | date   | Quoted date or `"never"`. Set when someone follows it end to end, not when the page is edited. |
| `rehearsal-frequency` |     | enum   | How often it should be exercised.                                                              |
| `requires-access`     |     | list   | Systems or roles needed before step 1.                                                         |

**Enum values**

| Field                 | Values                                                              |
|-----------------------|---------------------------------------------------------------------|
| `tier`                | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status`              | `active` · `draft` · `retired`                                      |
| `rehearsal-frequency` | `per-release` · `quarterly` · `annual`                              |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-processes -->

## Adding a process

1. Copy [`_template.md`](processes/_template.md) to `<slug>.md`. Processes use slug ids — `prc-releasing`.
2. Write the steps imperatively and in order. Assume the reader has not done it before.
3. Include prerequisites, a verification step ("you know it worked when…") and a rollback.
4. Set `last-rehearsed`. `"never"` is permitted and preferable to a guess.
5. Name what access is needed in `requires-access`, and who to ask. "Obtain the file from the repository owner" is not
   useful to someone who doesn't know who that is.

**Conventions**

* **No hedging.** "Typically the order would be…" is not followable. If the order varies, say what it depends on.
* **Verification is not optional.** A process that ends at the last action leaves the reader guessing whether it worked.
* **Rehearse before you trust.** Update `last-rehearsed` when someone actually follows it end to end — not when someone
  edits the document.

## What CI checks

<!-- BEGIN GENERATED: checks-processes -->

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
| `placeholder-left`          | error   | No `{{…}}` from the template is left unfilled, outside code.                                                    |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                                  |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                 |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                        |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                        |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `no-hedged-ordering`        | warning | No step hedged with "typically", "usually" or "normally".                                                       |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule        | What it would verify                                                                                    |
|-------------|---------------------------------------------------------------------------------------------------------|
| `staleness` | Scheduled. Reports processes past their rehearsal frequency, and any whose `last-rehearsed` is `never`. |

<!-- END GENERATED: checks-processes -->
