# Controls

How we know the standards are actually being followed.

**[→ Index](controls/INDEX.md)**

## What is a control?

A control binds a rule to its enforcement. It names the standards rules it verifies, the mechanism that does the
verifying, how often it runs, and where the evidence lives.

## Why we use them

A rulebook nobody checks becomes fiction slowly and without anyone noticing. Controls make the gap between *rule* and
*enforcement* visible and countable: what proportion of our standards are actually real?

That number is useful at three engineers and at three hundred. It is also the honest answer when someone asks whether we
follow our own standards — rather than pointing at the standards and hoping.

The most valuable value in the `mechanism` enum is **`not-enforced`**. It converts an aspiration into a visible number
instead of letting it hide. Do not invent a mechanism to avoid using it.

## Scope

A control is **not** the rule. The rule lives in a [standard](/standards); the control says how it is checked.

| Standard                                                   | Control                                                                  |
|------------------------------------------------------------|--------------------------------------------------------------------------|
| "Secrets **MUST** come from Key Vault."                    | "CI runs secret scanning on every PR; failures block merge."             |
| "Every public endpoint **MUST** carry a conformance test." | "Quarterly manual audit of the OpenAPI document against the test suite." |

If it can fail a build, block a merge, raise an alert or produce an audit artefact, it is a control. If it tells you
what to do, it is a standard.

One control may verify several rules, and one rule may need several controls. Controls apply to
[services](/services) — a control with no scope is a control nobody owns.

## Metadata

<!-- BEGIN GENERATED: schema-controls -->

| Field        | Req | Type   | Notes                                                                                    |
|--------------|-----|--------|------------------------------------------------------------------------------------------|
| `id` †       | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                    |
| `tier` †     | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.     |
| `status` †   | ●   | enum   | Whether the control is running, intended, or stood down.                                 |
| `owner` †    | ●   | string | A named person, never a team alias.                                                      |
| `tags` †     |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                         |
| `verifies`   | ●   | list   | Standard ids, ideally rule-level anchors. A control that names no rule is not a control. |
| `mechanism`  | ●   | enum   | How the check happens. `not-enforced` is first-class — an honest gap beats a fiction.    |
| `frequency`  |     | enum   | How often it runs.                                                                       |
| `evidence`   |     | string | Where the proof lives — the build log, the audit note, the dashboard.                    |
| `applies-to` |     | list   | Service ids, or `all`.                                                                   |

**Enum values**

| Field       | Values                                                                           |
|-------------|----------------------------------------------------------------------------------|
| `tier`      | `decided` · `normative` · `descriptive` · `procedural` · `observed`              |
| `status`    | `active` · `planned` · `retired`                                                 |
| `mechanism` | `ci` · `review-checklist` · `manual-periodic` · `runtime-alert` · `not-enforced` |
| `frequency` | `per-pr` · `per-deploy` · `daily` · `monthly` · `quarterly` · `annual`           |

**Conditionally required**

| Field       | Required when               |
|-------------|-----------------------------|
| `frequency` | `mechanism != not-enforced` |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-controls -->

## Adding a control

1. Copy [`template.md`](controls/template.md) to `NNNN-kebab-slug.md`.
2. Name the rules it verifies in `verifies` — rule-level anchors where the standard has them, the standard id otherwise.
3. Pick the real `mechanism`. If nothing currently checks the rule, that is `not-enforced` and the control still gets
   written — an unenforced rule you know about is worth more than one you don't.
4. Record where the evidence lives: the pipeline step, the checklist, the alert rule, the audit note.

**Conventions**

* **A control names rules, not intentions.** "We review carefully" is not a control; "the PR template requires a tick
  against each conformance checklist item" is.
* **`not-enforced` is a first-class value**, not a failure state. The coverage report is only useful if it is honest.
* **Controls follow the tooling.** When enforcement changes, update the control rather than the standard — the rule
  didn't change, the way we check it did.

## What CI checks

<!-- BEGIN GENERATED: checks-controls -->

| Check                       | Level   | What it verifies                                                                                         |
|-----------------------------|---------|----------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                      |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                           |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                        |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                  |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                           |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                     |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                  |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                             |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                               |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                               |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                            |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                    |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                          |
| `h1`                        | error   | The document has an H1.                                                                                  |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter. |
| `required-section`          | error   | Every required section heading is present.                                                               |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                 |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                         |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |

<!-- END GENERATED: checks-controls -->
