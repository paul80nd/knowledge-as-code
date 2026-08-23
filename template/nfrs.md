# NFRs

Non-functional requirements: what the platform promises about availability, speed and recovery.

**[→ Index](nfrs/_index.md)**

## What is an NFR?

A stated, measurable target for a quality of service rather than a behaviour: availability, latency budgets, throughput,
recovery point and recovery time objectives (RPO and RTO), capacity assumptions. Each one names what it applies to, what
the target is, and how we measure it.

## Why we use them

An undocumented NFR is still real, and we discover it during an incident. Write it down and an assumption becomes a
commitment somebody has agreed to. A [postmortem](postmortems.md) then has something honest to measure against.

A target also constrains design. An RTO of four hours and an RTO of four minutes produce different architectures, and
settling that up front costs a fraction of rebuilding later.

## Scope

An NFR states a **target**, not a rule and not a mechanism.

* "Availability is 99.5% monthly, measured by the uptime probe" is an NFR.
* "Services **MUST** expose a `/health` endpoint" is a [standard](standards.md).
* "The uptime probe alerts at 99.5%" is a [control](controls.md).

**An NFR you cannot measure is a wish.** `measured-by` is required. Where nothing observes the target today, either
build the instrument or state the target you *can* observe. "We'd notice" is not a measurement method.

We cannot promise more than the dependencies we do not run. A third-party [integration](integrations.md) with a 99% SLA
caps everything built on it at 99%. Name that integration in `constrained-by`, and set the target at what the estate can
deliver.

## Metadata

<!-- BEGIN GENERATED: schema-nfrs -->

| Field            | Value                      | Notes                                                                                             |
|------------------|----------------------------|---------------------------------------------------------------------------------------------------|
| `id` *†          | string                     | Stable, unique across the corpus, never reused. Format set by the type.                           |
| `tier` *†        | `normative`                | Fixed for the type. A trust signal for the reader. CI checks it matches the folder.               |
| `status` *†      | `draft` `agreed` `retired` | `agreed` is a commitment someone accepted, not an aspiration.                                     |
| `owner` *†       | string                     | A named person, never a team alias.                                                               |
| `tags` †         | list                       | Free-form, lowercase, hyphenated. Used for cross-cutting search.                                  |
| `applies-to` *   | list                       | Estate-wide targets are almost always wrong. Scope them.                                          |
| `target` *       | string                     | Concrete and arguable (`99.5% monthly`, `p95 < 400ms`, `RTO 4h`). Include the measurement window. |
| `measured-by` *  | string                     | An NFR you cannot measure is a wish. "We'd notice" is not a measurement method.                   |
| `constrained-by` | list                       | Integrations whose own SLA caps this target.                                                      |
| `review-by` *    | date                       | Quoted. The date by which someone confirms this is still true.                                    |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-nfrs -->

## Adding an NFR

1. Copy [`_template.md`](nfrs/_template.md) to `NNNN-kebab-slug.md`.
2. State the target concretely. "Fast" is not a target; "p95 under 400ms" is.
3. Name the instrument that measures it, and say where a reader can find its reading.
4. Record what breaching it costs: degraded service, contractual exposure, or nothing much. An NFR with no consequence
   is documentation theatre.
5. Leave `status: draft` until someone has accepted the target, then set it to `agreed`.

**Conventions**

* **Scope each target to a capability or a service.** A default covering the whole estate holds a marketing page to the
  checkout flow's availability budget.
* **Record the current actual beside the target** where it is known. The gap between the two is the useful part.

## What CI checks

<!-- BEGIN GENERATED: checks-nfrs -->

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
| `unused-definition`         | warning | A link definition that nothing references.                                                                      |
| `target-is-measurable`      | warning | `measured-by` names an instrument, not a hedge: "monitored", "as needed", "where practical".                    |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                     | What it would verify                                                                                 |
|--------------------------|------------------------------------------------------------------------------------------------------|
| `constraint-consistency` | Where `constrained-by` names an integration whose `their-sla` is weaker than this target, report it. |

<!-- END GENERATED: checks-nfrs -->
