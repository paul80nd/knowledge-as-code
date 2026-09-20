# NFRs

Non-functional requirements: what the platform promises about availability, speed and recovery.

**[→ Index](nfrs/_index.md)**

## What is an NFR?

A stated, measurable target for a quality of service rather than a behaviour. `characteristic` names which quality:
availability, latency, throughput, capacity, recovery, scalability or accuracy. Recovery covers the recovery point and
recovery time objectives (RPO and RTO). Each record states one quality, what it applies to, the target, the period the
target is read over, and how we measure it.

## Why we use them

An undocumented NFR is still real, and we discover it during an incident. Write it down and an assumption becomes a
commitment somebody has agreed to. A postmortem then has something honest to measure against.

A target also constrains design. An RTO of four hours and an RTO of four minutes produce different architectures, and
settling that up front costs a fraction of rebuilding later.

## Scope

An NFR states a **target**, not a rule and not a mechanism.

* "Availability is 99.5% monthly, measured by the uptime probe" is an NFR.
* "Services **MUST** expose a `/health` endpoint" is a [standard](standards.md).
* "The uptime probe alerts at 99.5%" is a control.

**An NFR you cannot measure is a wish.** `measured-by` is required. Where nothing observes the target today, either
build the instrument or state the target you *can* observe. "We'd notice" is not a measurement method.

**A figure with no period behind it is not a target.** `window` is required for an availability, capacity, latency or
throughput target, because a rate or a percentile only means something over a stated period. A recovery, scalability or
accuracy target binds each event or each value, so it needs no window.

**A target with no reason is a number somebody picked.** `Why this number` is required. State what fixes the figure:
what a customer will tolerate, what a dependency already limits you to, or a measurement you have taken. Say what the
target leaves out, so a reader can see the gap is deliberate.

**A breach has a cost and a response, and they are two sections.** `What a breach costs` states what a miss costs.
`What we do about a breach` states the response: the alert that fires, who it pages, and the repair. Both are
required. A target whose miss costs nothing much answers both, and says that nobody is paged.

**An NFR says what it is achieving now.** `Current actual` is required. A reader sees the reading beside the target,
so the gap between them is never something to work out elsewhere.

**A target nobody is held to yet says so in `status`.** `aspirational` means somebody settled the figure and the
estate is not measured against it. `What we do about a breach` then says that nobody is paged. `agreed` means the
estate is held to the number.

We cannot promise more than the dependencies we do not run. A third-party integration with a 99% SLA caps everything
built on it at 99%. Name that integration in `constrained-by`, and set the target at what the estate can deliver.

## Metadata

<!-- BEGIN GENERATED: schema-nfrs -->

| Field              | Value                                                                                | Notes                                                                                                                                                                |
|--------------------|--------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `id` *†            | string                                                                               | Stable, unique across the corpus, never reused, in the format the type sets.                                                                                         |
| `type` *†          | string                                                                               | The singular name of the type, which CI checks against the folder.                                                                                                   |
| `tier` *†          | `normative`                                                                          | The record's trust level, fixed for the type and checked against the folder.                                                                                         |
| `status` *†        | `draft` `aspirational` `agreed` `retired`                                            | `agreed` binds the estate. `aspirational` is a target set but not yet binding.                                                                                       |
| `owner` *†         | string                                                                               | A person as `human:alex.doe`, or a post as `role:head-of-engineering`.                                                                                               |
| `sources` †        | list                                                                                 | Where this record's content came from, one entry per source.                                                                                                         |
| `tags` †           | list                                                                                 | Free-form, lowercase and hyphenated. A reader searches on these across types.                                                                                        |
| `characteristic` * | `accuracy` `availability` `capacity` `latency` `recovery` `scalability` `throughput` | The quality of service this target commits to.                                                                                                                       |
| `applies-to` *     | list                                                                                 | The service or offering ids this target binds.                                                                                                                       |
| `target` *         | string                                                                               | The number this commits to: `99.5%`, `p95 under 400ms`, `RPO 5 minutes`.                                                                                             |
| `window`           | string                                                                               | The period the number is read over: `rolling 1 hour`, `monthly`, `rolling 4 weeks`. Required when `characteristic in [availability, capacity, latency, throughput]`. |
| `measured-by` *    | string                                                                               | The instrument that reports the number, and where to read it.                                                                                                        |
| `constrained-by`   | list                                                                                 | Integrations whose own SLA caps this target.                                                                                                                         |
| `agreed-on`        | date                                                                                 | The day the named owner accepted the commitment. Required when `status == agreed`.                                                                                   |
| `review-by` *      | date                                                                                 | Quoted. The date by which someone confirms this is still true.                                                                                                       |

\* Field is required  
† Carried by every document in the taxonomy. See [Metadata](knowledge-as-code/metadata.md).

<!-- END GENERATED: schema-nfrs -->

## Adding an NFR

1. Copy [`_template.md`](nfrs/_template.md) to `NNNN-kebab-slug.md`.
2. Set `characteristic` to the quality this target commits to. One record states one quality.
3. State the target concretely. "Fast" is not a target; "p95 under 400ms" is.
4. Set `window` to the period the figure is read over, where the characteristic requires one.
5. Say under `Why this number` what fixes the figure, and what it leaves out.
6. Name the instrument that measures it, and say where a reader can find its reading.
7. Record the current actual, and the day it was read. The gap between it and the target is the useful part.
8. Record what a breach costs: degraded service, contractual exposure, or nothing much. An NFR with no consequence is
   documentation theatre.
9. Record the response to a breach: the alert that fires, who it pages, and the repair.
10. Leave `status: draft` until someone has accepted the target. Then set it to `agreed` and write `agreed-on`.

**Conventions**

* **Scope each target to an offering or a service.** A default covering the whole estate holds a marketing page to the
  checkout flow's availability budget.
* **Open an aspirational target at `aspirational`, not `draft`.** `draft` says nobody has settled the figure.
  `aspirational` says the figure is settled and nobody is held to it.

## What CI checks

<!-- BEGIN GENERATED: checks-nfrs -->

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
| `target-is-measurable`      | warning | `measured-by` states an instrument. A hedge such as "monitored" or "where practical" fails.                     |

**Declared, not yet enforced**: carried by the schema, run by nothing.

| Rule                     | What it would verify                                                                          |
|--------------------------|-----------------------------------------------------------------------------------------------|
| `constraint-consistency` | Every integration in `constrained-by` states a `their-sla` at least as strong as this target. |

<!-- END GENERATED: checks-nfrs -->
