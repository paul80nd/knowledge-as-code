# Controls

How we know the standards are being followed.

**[→ Index](controls/_index.md)**

## What is a control?

A control binds a rule to its enforcement. It names the rules it verifies, the mechanism that checks them, how often
that check runs, and where the evidence lives.

## Why we use them

A standard states a rule and says nothing about whether anyone still follows it. A control answers that question for one
rule. Taken together, the controls make the gap between *rule* and *enforcement* countable: what share of our standards
does something check?

That share is worth knowing at three engineers and at three hundred. When someone asks whether we follow our own
standards, the controls answer honestly. Without them we can only point at the standards and hope.

**`not-enforced` matters more than any other `mechanism` value.** Using it turns an aspiration into a number someone can
see, and the number is worth reading only while it is honest. Do not invent a mechanism to avoid it.

## Scope

A control is not the rule it checks. The rule lives in a [standard](standards.md), and the control says how that rule is
checked.

| Standard                                                   | Control                                                                  |
|------------------------------------------------------------|--------------------------------------------------------------------------|
| "Secrets **MUST** come from Key Vault."                    | "CI runs secret scanning on every PR, and failures block merge."         |
| "Every public endpoint **MUST** carry a conformance test." | "Quarterly manual audit of the OpenAPI document against the test suite." |

Anything that can fail a build, block a merge, raise an alert or produce an audit artefact is a control. Anything that
tells you what to do is a standard.

A control names the [services](services.md) it applies to, because a control with no scope is one nobody owns. One control
may verify several rules, and one rule may need several controls.

## Metadata

<!-- BEGIN GENERATED: schema-controls -->

| Field         | Value                                                                    | Notes                                                                                    |
|---------------|--------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `id` *†       | string                                                                   | Stable, unique across the corpus, never reused. Format set by the type.                  |
| `tier` *†     | `normative`                                                              | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.     |
| `status` *†   | `active` `planned` `retired`                                             | Whether the control is running, intended, or stood down.                                 |
| `owner` *†    | string                                                                   | A named person, never a team alias.                                                      |
| `tags` †      | list                                                                     | Free-form, lowercase, hyphenated. Used for cross-cutting search.                         |
| `verifies` *  | list                                                                     | Standard ids, ideally rule-level anchors. A control that names no rule is not a control. |
| `mechanism` * | `ci` `review-checklist` `manual-periodic` `runtime-alert` `not-enforced` | How the check happens. `not-enforced` is first-class — an honest gap beats a fiction.    |
| `frequency`   | `per-pr` `per-deploy` `daily` `monthly` `quarterly` `annual`             | How often it runs. Required when `mechanism != not-enforced`.                            |
| `evidence`    | string                                                                   | Where the proof lives — the build log, the audit note, the dashboard.                    |
| `applies-to`  | list                                                                     | Service ids, or `all`.                                                                   |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-controls -->

## Adding a control

1. Copy [`_template.md`](controls/_template.md) to `NNNN-kebab-slug.md`.
2. Name the rules it verifies in `verifies`: rule-level anchors where the standard has them, the standard id otherwise.
3. Pick the mechanism that runs today. Where nothing checks the rule, that is `not-enforced`, and the control still gets
   written: an unenforced rule you know about is worth more than one you do not.
4. Record where the evidence lives: the pipeline step, the checklist, the alert rule, the audit note.

**Conventions**

* **A control names rules, not intentions.** "We review carefully" is not a control. "The PR template requires a tick
  against each conformance checklist item" is.
* **Controls follow the tooling.** When the way we check a rule changes, update the control. The rule has not changed,
  so the standard stays as it is.

## What CI checks

<!-- BEGIN GENERATED: checks-controls -->

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
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                |
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `mechanism-has-evidence`    | warning | A control whose mechanism is not `not-enforced` names where its evidence can be found.                          |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule              | What it would verify                                                                   |
|-------------------|----------------------------------------------------------------------------------------|
| `coverage-report` | The share of MUST / MUST NOT rules in active standards claimed by an enforced control. |

<!-- END GENERATED: checks-controls -->
