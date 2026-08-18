# Engineering Standards

Teams follow these standards when they build and review functionality: the normative patterns and practices of the
platform.

## What is a standard?

A standard is a short, living Markdown document that states a rule we follow. It says what to do, imperatively, with
concrete examples and a conformance checklist you tick a design against. Standards are the day-to-day rulebook. Design a
new endpoint, header or contract, and the standard for that area tells you the shape it must take.

## How standards relate to ADRs

[ADRs](/adrs) and standards are two halves of one record:

* An **ADR** captures a decision: the context, the choice, the alternatives weighed, the consequences. It answers *why*,
  and it is immutable once accepted.
* A **standard** captures the practice that decision produces. It answers *what to do*, and we edit it in place as the
  practice matures.

A standard distils accepted ADRs into something you can follow without reading the reasoning again, and cites what it
came from: the ADR owns the *why* and the standard owns the *what*. A standard descending from a policy rather than a
decision cites the policy instead. Below both sits a feature **spec**, which applies the standards to a concrete API and
documents only what is unique to that feature.

| Layer         | Answers                      | Lifecycle                    |
|---------------|------------------------------|------------------------------|
| [ADRs](/adrs) | *Why* did we decide this?    | Immutable, append-only log   |
| Standard      | *What* must I do, and how?   | Living, maintained catalogue |
| Spec          | What does *this* feature do? | Per-feature instance         |

## Why we use them

The ADR log preserves the reasoning, and reasoning is the wrong thing to read when you are mid-build and want the rule.
A standard states the pattern itself, in one place a reader can scan. Someone designing new functionality — a
contributor or an AI session — finds the rule and checks the design against a conformance checklist. They open the ADR
only when they want the deeper *why*.

## Categories

We state each standard at its true **altitude** on one of four axes — `common`, `platform`, `interface`, `domain` — and
we **compose** them. The rule-set enforced for a piece of work is the union of the axes that apply to it.

The folders below group the standards a reader goes looking for together. A folder sits on an axis rather than being
one: `public-api`, `widgets` and `webhooks` all carry `axis: interface`.

* **common** — platform-agnostic principles (testing philosophy, code-quality-as-a-gate). _Active._
* **platform** — language, runtime and framework specifics: `node/`, `lit/` (future `dotnet/`). _Active._
* **public-api** — public HTTP APIs called directly from customer-embedded widgets and integrations. _Drafted._
* **widgets** — embedded widgets and web components, the clients of the public API: how we build, deliver and embed
  them. _Drafted._
* **global-styles** — embed theming: the `--<prefix>-*` CSS custom-property contract, stable class hooks, and the
  authoring rules that keep every embedded widget restylable to match the host brand at render time. _Drafted._
* **messaging** — the message bus contract: topic naming, payload shape, delivery guarantees. _Drafted._

Auth, caching and versioning change with the consumer and the trust model, so the interface and domain categories stay
distinct. The common and platform axes let a rule live once, at the layer where it is actually true.

## Where to find them

* **[→ Standards index](standards/_index.md)** — the generated catalogue of every standard, grouped by axis.
* **[`_template.md`](standards/_template.md)** — copy it to start a new standard. The categories above and the steps
  below cover the rest.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field          | Type   | Notes                                                                                |
|----------------|--------|--------------------------------------------------------------------------------------|
| `id` *†        | string | Stable, unique across the corpus, never reused. Format set by the type.              |
| `tier` *†      | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder. |
| `status` *†    | enum   | Plain values only — enforcement notes belong in `verified-by`.                       |
| `owner` *†     | string | A named person, never a team alias.                                                  |
| `tags` †       | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                     |
| `axis` *       | enum   | The layer where the rule is actually true.                                           |
| `derived-from` | list   | The ADRs this standard distils. Provenance may come from `implements` instead.       |
| `implements`   | list   | Policy ids this standard puts into practice.                                         |
| `verified-by`  | list   | Control ids that check it.                                                           |
| `applies-to` * | list   | Service ids, or `all`.                                                               |
| `review-by` *  | date   | Quoted. The date by which someone confirms this is still true.                       |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `active` · `deprecated` · `superseded`                    |
| `axis`   | `common` · `platform` · `interface` · `domain`                      |

\* Field is required  
† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-standards -->

## Adding or changing a Standard

Name where the standard comes from: an ADR in `derived-from`, a policy in `implements`, or both. `provenance-required`
fails a standard carrying neither. Where you can name neither, either the decision has not been made — make it — or what
you are writing is guidance rather than a standard.

Write the rules with RFC 2119 keywords, and make each one **testable**. Where a rule cannot be checked against a
concrete artefact, sharpen it or move it to the rationale section. Every **MUST** and **MUST NOT** should have a
corresponding control, even where that control's mechanism is `not-enforced`. An honest gap is more useful than a silent
one.

Standards are living documents, and we edit them in place. Record every material change in the changelog.

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

| Check                       | Level   | What it verifies                                                                                                |
|-----------------------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                             |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                  |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                               |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                         |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                  |
| `date-quoted / date-format` | error   | Date fields are quoted, and name a day the calendar has — `YYYY-MM-DD`.                                         |
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
| `provenance-required`       | error   | A standard cites an ADR in `derived-from`, a policy in `implements`, or both.                                   |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                           | What it would verify                                                                                       |
|--------------------------------|------------------------------------------------------------------------------------------------------------|
| `rules-have-controls`          | Every MUST / MUST NOT rule is claimed by a control, or the standard declares the gap explicitly.           |
| `changelog-begins-at-active`   | Changelog entries are material changes only, and begin when status becomes `active`.                       |
| `changelog-on-material-change` | If the Rules section changed and status is `active`, a new changelog entry is required in the same commit. |

<!-- END GENERATED: checks-standards -->
