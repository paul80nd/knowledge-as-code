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
commitment somebody has agreed to. A [postmortem](postmortems.md) then has something honest to measure against.

A target also constrains design. An RTO of four hours and an RTO of four minutes produce different architectures, and
settling that up front costs a fraction of rebuilding later.

## Scope

An NFR states a **target**, not a rule and not a mechanism.

* "Availability is 99.5% monthly, measured by the uptime probe" is an NFR.
* "Services **MUST** expose a `/health` endpoint" is a [standard](standards.md).
* "CI fails a service that exposes no `/health` endpoint" is a [control](controls.md).

We cannot promise more than the dependencies we do not run. A third-party [integration](integrations.md) with a 99% SLA
caps everything built on it at 99%. Name that integration in `constrained-by`, and set the target at what the estate can
deliver.

## Level of detail and cadence

One NFR states one target and how it is measured. The dashboard query or the alert rule belongs in the service's own
repository, and the NFR links to it.

The target changes rarely, when somebody renegotiates it. Current actual changes with each measurement. New NFRs arrive
when a service or an offering takes on a target.

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

Copy [`_template.md`](nfrs/_template.md). Its guidance block states how to write the id and each field. The prompt under
each section states how to write that section.

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
