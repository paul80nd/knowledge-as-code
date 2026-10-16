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
| `tier` *†          | `normative`                                                                          | The record's trust level, fixed for the type and checked against the folder.                                                                                         |
| `status` *†        | `draft` `aspirational` `agreed` `retired`                                            | `agreed` binds the estate. `aspirational` is a target set but not yet binding. In force at `agreed`.                                                                 |
| `characteristic` * | `accuracy` `availability` `capacity` `latency` `recovery` `scalability` `throughput` | The quality of service this target commits to.                                                                                                                       |
| `applies-to` *     | list                                                                                 | The service or offering ids this target binds.                                                                                                                       |
| `target` *         | string                                                                               | The number this commits to: `99.5%`, `p95 under 400ms`, `RPO 5 minutes`.                                                                                             |
| `window`           | string                                                                               | The period the number is read over: `rolling 1 hour`, `monthly`, `rolling 4 weeks`. Required when `characteristic in [availability, capacity, latency, throughput]`. |
| `alert-after`      | string                                                                               | How long the condition holds before the alert or ticket is raised: `15 minutes`, `2 hours`.                                                                          |
| `measured-by` *    | string                                                                               | The instrument that reports the number, and where to read it.                                                                                                        |
| `constrained-by`   | list                                                                                 | Integrations whose own SLA caps this target.                                                                                                                         |
| `agreed-on`        | date                                                                                 | The day the named owner accepted the commitment. Required when `status == agreed`.                                                                                   |
| `review-by` *      | date                                                                                 | Quoted. The date by which someone confirms this is still true.                                                                                                       |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

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

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                        | Level   | What it verifies                                                                                               |
|------------------------------|---------|----------------------------------------------------------------------------------------------------------------|
| `ref-resolves`               | error   | An id in a field that references another document names one that exists, of the type the field names.          |
| `reciprocal`                 | error   | A reciprocal field and its counterpart agree in both directions.                                               |
| `alert-after-needs-an-alert` | warning | A target stating `alert-after` binds a service that raises an alert or a ticket, through any offering between. |
| `target-is-measurable`       | warning | `measured-by` states an instrument. A hedge such as "monitored" or "where practical" fails.                    |
| `nfr-in-date`                | warning | A target names a review date that has not passed.                                                              |

<!-- END GENERATED: checks-nfrs -->
