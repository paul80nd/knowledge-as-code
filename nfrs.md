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

| Field            | Req | Type   | Notes                              |
|------------------|-----|--------|------------------------------------|
| `status`         | ●   | enum   | `draft` · `agreed` · `retired`     |
| `applies-to`     | ●   | list   | Service or capability ids          |
| `target`         | ●   | string | e.g. `99.5% monthly`, `RTO 4h`     |
| `measured-by`    | ●   | string | An NFR you can't measure is a wish |
| `constrained-by` |     | list   | Integration ids                    |
| `review-by`      | ●   | date   | Quoted                             |

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

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-nfrs -->
