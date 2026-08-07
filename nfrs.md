# NFRs

Non-functional requirements — what the platform promises about availability, speed and recovery.

**[→ Index](nfrs/INDEX.md)**

## What is an NFR?

A stated, measurable target for a quality of service rather than a behaviour: availability, latency budgets, throughput,
RPO, RTO, capacity assumptions. Each names what it applies to, what the target is, and — critically — how it is
measured.

## Why we use them

Undocumented NFRs are still real; they are just discovered during an incident. Writing them down converts an assumption
into a commitment somebody has agreed to, and gives [postmortems](/postmortems) something honest to measure against.

They also constrain design. An RTO of four hours and an RTO of four minutes produce different architectures, and the
decision is much cheaper before the fact.

## Scope

An NFR states a **target**, not a rule and not a mechanism.

* "Availability is 99.5% monthly, measured by the uptime probe" — an NFR.
* "Services **MUST** expose a `/health` endpoint" — a [standard](/standards).
* "The uptime probe alerts at 99.5%" — a [control](/controls).

**An NFR you cannot measure is a wish.** `measured-by` is required, and "we'd notice" is not a measurement method. If
there is no way to observe it, either build one or write down the target you *can* observe.

NFRs are also constrained by things outside our control — a third-party [integration](/integrations) with a 99% SLA caps
anything built on it. Record that in `constrained-by` rather than promising something the estate cannot deliver.

## Metadata

<!-- BEGIN GENERATED: schema-nfrs -->

| Field            | Req | Type   | Notes                                                                                             |
|------------------|-----|--------|---------------------------------------------------------------------------------------------------|
| `id` †           | ●   | string | Stable, unique across the wiki, never reused. Format set by the type.                             |
| `tier` †         | ●   | enum   | Fixed for the type — a trust signal for the reader. CI checks it matches the folder.              |
| `status` †       | ●   | enum   | `agreed` is a commitment someone accepted, not an aspiration.                                     |
| `owner` †        | ●   | string | A named person, never a team alias.                                                               |
| `tags` †         |     | list   | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                  |
| `applies-to`     | ●   | list   | Estate-wide targets are almost always wrong — scope them.                                         |
| `target`         | ●   | string | Concrete and arguable — `99.5% monthly`, `p95 < 400ms`, `RTO 4h`. Include the measurement window. |
| `measured-by`    | ●   | string | An NFR you cannot measure is a wish. "We'd notice" is not a measurement method.                   |
| `constrained-by` |     | list   | Integrations whose own SLA caps this target.                                                      |
| `review-by`      | ●   | date   | Quoted. Drives the staleness report.                                                              |

**Enum values**

| Field    | Values                                                              |
|----------|---------------------------------------------------------------------|
| `tier`   | `decided` · `normative` · `descriptive` · `procedural` · `observed` |
| `status` | `draft` · `agreed` · `retired`                                      |

† Carried by every document in the taxonomy — see [Metadata](/knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-nfrs -->

## Adding an NFR

1. Copy [`template.md`](nfrs/template.md) to `NNNN-kebab-slug.md`.
2. State the target concretely. "Fast" is not a target; "p95 under 400ms" is.
3. State how it is measured, and where that measurement can be seen.
4. Record what breaching it actually means — degraded service, contractual exposure, or nothing much. An NFR with no
   consequence is documentation theatre.
5. `status: draft` until someone has agreed it. `agreed` is a commitment, not an aspiration.

**Conventions**

* **Targets are per capability or per service**, never estate-wide by default — a marketing page and the checkout flow
  do not deserve the same availability budget.
* **Record the current actual alongside the target** where it is known. The gap is the useful part.

## What CI checks

<!-- BEGIN GENERATED: checks-nfrs -->

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
| `clause-ref`                | error   | A `pol-XXXX.CLAUSE` citation names a clause that exists.                                                 |
| `link-resolves`             | error   | Every internal link resolves (all link forms, `.md` optional).                                           |
| `undefined-label`           | error   | Every shortcut reference has a link definition.                                                          |
| `label-canonical`           | error   | A shortcut label that names a document is written as that document's id.                                 |
| `unused-definition`         | warning | A link definition that nothing references.                                                               |

<!-- END GENERATED: checks-nfrs -->
