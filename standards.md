# Engineering Standards

This wiki maintains the engineering standards for the platform — the normative patterns and practices that teams
follow when building and reviewing functionality.

## What is a standard?

A standard is a short, living Markdown document that states a rule we follow — what to do, expressed imperatively,
with concrete examples and a conformance checklist. Standards are the practical, day-to-day rulebook: when you design
a new endpoint, header, or contract, the relevant standard tells you the shape it must take.

## How standards relate to ADRs

[ADRs](/adrs) and standards are two halves of the same record:

* An **ADR** captures a *decision* — the context, the choice, the alternatives weighed, the consequences. It answers
  *why*, and it is immutable once accepted.
* A **standard** captures the *resulting practice* — the pattern or rule that the decision produces. It answers
  *what to do*, and it is maintained in place as the practice matures.

So a standard is the distillation of one or more accepted ADRs into something you can follow without re-reading the
reasoning each time. Each standard cites the ADR(s) it derives from; the ADR owns the "why", the standard owns the
"what". A feature **spec** then sits below both — it *applies* the standards to a concrete API and only documents what
is unique to that feature.

| Layer         | Answers                      | Lifecycle                    |
|---------------|------------------------------|------------------------------|
| [ADRs](/adrs) | *Why* did we decide this?    | Immutable, append-only log   |
| Standard      | *What* must I do, and how?   | Living, maintained catalogue |
| Spec          | What does *this* feature do? | Per-feature instance         |

## Why we use them

The ADR log preserves *reasoning*, but reasoning is the wrong thing to consult when you're mid-build and just need the
rule. Standards give a single, scannable source of truth for the patterns themselves — so a contributor (or an AI
session) designing new functionality can find the rule, check their design against a conformance checklist, and link
back to the ADR only when they need the deeper "why".

## Categories

Standards are stated at their true **altitude** along three axes and **composed** — the enforced rule-set for a piece of
work is the union of the layers that apply (`common ∪ platform ∪ framework ∪ domain`). The axes, and the heuristic for
placing a rule on the right one, are set out below.

* **common** — platform-agnostic principles (testing philosophy, code-quality-as-a-gate). _Active._
* **platform** — language / runtime / framework specifics: `node/`, `lit/` (future `dotnet/`). _Active._
* **public-api** — public HTTP APIs called directly from customer-embedded widgets and integrations. _Drafted._
* **widgets** — embedded widgets / web components: how they're built, delivered, and embedded (clients of the public
  API). _Drafted._
* **global-styles** — embed theming: the `--<prefix>-*` CSS custom-property contract, stable class hooks, and the
  authoring rules that keep every embedded widget restylable to match the host brand at render time. _Drafted._
* **webhooks** — public-facing webhooks for third-party integrations (API-key auth, delivery, signing). _Planned._
* **internal-api** — service-to-service APIs within the platform. _Planned._

Different consumer and trust models carry different rules for auth, caching, and versioning, so the interface/domain
categories stay distinct; the common and platform axes let a rule live once, at the layer where it is actually true.

## Where to find them

* **[→ Standards index](standards/_index.md)** — the generated catalogue of every standard, grouped by axis.
* **[`_template.md`](standards/_template.md)** — copy this to start a new standard; the categories above and the process
  below cover the rest.

## Metadata

<!-- BEGIN GENERATED: schema-standards -->

| Field          | Req | Type   | Notes                                                                                         |
|----------------|-----|--------|-----------------------------------------------------------------------------------------------|
| `id` †         | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                         |
| `tier` †       | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.          |
| `status` †     | ●   | enum   | Plain values only — enforcement notes belong in `verified-by`.                                |
| `owner` †      | ●   | string | A named person, never a team alias.                                                           |
| `tags` †       |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                              |
| `axis`         | ●   | enum   | The layer where the rule is actually true. TODO — four formulations exist; settle before use. |
| `derived-from` |     | list   | The ADRs this standard distils. Provenance may come from `implements` instead.                |
| `implements`   |     | list   | Policy ids this standard puts into practice.                                                  |
| `verified-by`  |     | list   | Control ids that check it.                                                                    |
| `applies-to`   | ●   | list   | Service ids, or `all`.                                                                        |
| `review-by`    | ●   | date   | Quoted. Drives the staleness report.                                                          |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `active` · `deprecated` · `superseded`                    |
| `axis`   | `common` · `platform` · `interface` · `domain`                      |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-standards -->

## Adding or changing a Standard

Every standard cites at least one ADR in `derived-from`. If there is no ADR, either the decision hasn't been made yet —
make it — or what you're writing is guidance rather than a standard.

Rules use RFC 2119 keywords and must be **testable**. If a rule can't be checked against a concrete artefact, sharpen it
or move it to the rationale section. Every **MUST** and **MUST NOT** should have a corresponding control, even if that
control's mechanism is `not-enforced` — an honest gap is more useful than a silent one.

Standards are living documents. Material changes are recorded in the changelog rather than made silently.

## What CI checks

<!-- BEGIN GENERATED: checks-standards -->

| Check                       | Level   | What it verifies                                                                                                                            |
|-----------------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------|
| `frontmatter-parses`        | error   | Frontmatter is present and is a valid YAML mapping.                                                                                         |
| `unknown-key`               | error   | Every frontmatter key is a schema field or a reserved ADO key.                                                                              |
| `key-order`                 | error   | Key order is a topological extension of the schema's field order.                                                                           |
| `required-field`            | error   | Required and conditionally-required fields are present.                                                                                     |
| `bare-key`                  | error   | An absent value is a bare key, never `null`, `~`, `""` or `—`.                                                                              |
| `date-quoted / date-format` | error   | Date fields are quoted `YYYY-MM-DD`.                                                                                                        |
| `enum`                      | error   | Enum values are in range and lowercase.                                                                                                     |
| `field-pattern`             | error   | Values match the pattern their field declares (e.g. `tags`).                                                                                |
| `list-order`                | warning | List entries read in alphabetical order, with numbers compared as numbers.                                                                  |
| `tier-matches-type`         | error   | `tier` matches the tier the type declares.                                                                                                  |
| `id`                        | error   | `id` carries the type's prefix and matches the filename's number or mnemonic.                                                               |
| `id-unique`                 | error   | `id` is unique across the whole wiki.                                                                                                       |
| `filename / slug-length`    | error   | Filename matches the pattern; the slug is within 30 characters.                                                                             |
| `h1`                        | error   | The document has an H1.                                                                                                                     |
| `identity`                  | error   | An identity line beneath the H1 names the type, id and status, and all three agree with the frontmatter.                                    |
| `required-section`          | error   | Every required section heading is present.                                                                                                  |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                                                              |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                                                             |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                                                    |
| `ref-resolves`              | error   | An id in a field that references another document names one that exists.                                                                    |
| `reciprocal`                | error   | A reciprocal field and its counterpart agree in both directions.                                                                            |
| `unused-definition`         | warning | A link definition that nothing references.                                                                                                  |
| `provenance-required`       | error   | Every standard cites at least one ADR in `derived-from` or one policy in `implements`. A standard with neither is guidance, not a standard. |

**Declared, not yet enforced** — carried by the schema, run by nothing.

| Rule                           | What it would verify                                                                                                                                                                                         |
|--------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `rules-have-controls`          | Every MUST / MUST NOT rule is claimed by a control, or the standard declares the gap explicitly.                                                                                                             |
| `changelog-begins-at-active`   | Changelog entries are material changes only — a rule added, removed, or changed in effect — and begin when status becomes `active`. Wording, examples, link fixes and typos are not material; git has those. |
| `changelog-on-material-change` | If the Rules section changed and status is `active`, a new changelog entry is required in the same commit.                                                                                                   |

<!-- END GENERATED: checks-standards -->
