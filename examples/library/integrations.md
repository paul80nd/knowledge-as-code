# Integrations

The external systems the platform depends on.

**[→ Index](integrations/_index.md)**

## What is an integration?

One document per third-party or external system we depend on but do not deploy: payment gateways, tax-calculation
services, email delivery, accounting systems, and the Azure platform services themselves. The record covers what the
system does for us, the contract we call it under, how it authenticates and how it fails. It also states what the vendor
promises us, what we do when they break that promise, who to contact, and how we would move to somebody else.

## Why we use them

Some incidents start outside our estate. Whoever picks one up needs to know which vendor runs the system, what the
contract says, what SLA they signed and who answers the phone when we call. None of that sits in our code or our logs.

An integration also caps what we can promise. Where an NFR targets more availability than the vendor's SLA supports, the
vendor's bad day breaks our target. Whoever sets that target can read the vendor's number in `their-sla` first.
Otherwise the gap turns up during an incident.

An integration is also the hardest thing here to replace. On the day a vendor raises the price or stops answering,
somebody has to say what a move would cost. `Exit` answers that before anyone is under pressure.

## Scope

**The line is whether we deploy it.** If we deploy it, it is a [service](services.md). If someone else runs it and we
call it, it is an integration.

A library or package we depend on is a tool, because it reaches us as code in our own build and not as a system somebody
else runs.

Not the place for:

* **How to configure it.** That is a [process](processes.md).
* **What to do when it's down.** That is a [runbook](runbooks.md), which this document links to.
* **Why we chose it.** That is an [ADR](adrs.md).

## Metadata

<!-- BEGIN GENERATED: schema-integrations -->

| Field           | Value                               | Notes                                                                          |
|-----------------|-------------------------------------|--------------------------------------------------------------------------------|
| `tier` *†       | `descriptive`                       | The record's trust level, fixed for the type and checked against the folder.   |
| `status` *†     | `active` `trial` `retired`          | Whether the integration is active, on trial, or retired. In force at `active`. |
| `vendor` *      | string                              | Who supplies the system.                                                       |
| `used-by` *     | list                                | Ids of the services that call this integration.                                |
| `criticality` * | `critical` `important` `supporting` | The impact on a customer when the integration is unavailable.                  |
| `their-sla`     | string                              | What the vendor's contract promises, in the contract's own words.              |
| `replaces`      | id                                  | The integration id this one took over from.                                    |
| `successor`     | id                                  | The integration id that took over from this one.                               |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-integrations -->

## Adding an integration

1. Copy [`_template.md`](integrations/_template.md) to `<slug>.md`. Integrations use slug ids: `itg-sendgrid`.
2. Record the contract and how the system authenticates us. Name where the credential is held. Nothing secret goes in
   this corpus.
3. Give each way the system fails a row of the Failure modes table, and say in its fallback cell what we do instead. "It
   goes down sometimes" is not a failure mode; "returns 503 during their maintenance window, we queue and retry" is.
4. Copy `their-sla` from the contract, word for word.
5. Name who to contact when the vendor misses that SLA.
6. Say how we would leave. Name what would have to be rebuilt, what data would have to move, and what makes the move
   hard.

**Conventions**

* **Every failure mode names its fallback**, or writes `None.` in that cell. Where a row says neither, whoever is on call
  works it out during the incident.
* **A `critical` or `important` integration says how we would leave it.** `Exit` states what would have to be rebuilt
  and what makes the move hard. A vendor's lock-in rarely appears in the contract.
* **A `trial` integration says what would end the trial.** A trial with no decision criteria stays a trial forever.
* **A `retired` integration names its `successor`**, and the successor names it back with `replaces`. CI checks both
  ends, so a reader arriving from an old citation reaches the system that took the traffic.
* **Record the commercial facts**: cost model, renewal date, account owner. Nobody else writes them down, and you want
  them on the day the vendor raises the price or stops answering.
* **`used-by` names the services that call this system**, by id. CI fails an id that names no service.

## What CI checks

<!-- BEGIN GENERATED: checks-integrations -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                     | Level   | What it verifies                                                                                      |
|---------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `ref-resolves`            | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `reciprocal`              | error   | A reciprocal field and its counterpart agree in both directions.                                      |
| `retired-has-successor`   | warning | A retired integration states what took over from it.                                                  |
| `trial-criteria-required` | warning | An integration on trial states what would end the trial.                                              |
| `exit-required`           | warning | A critical or important integration still in use states how the estate would leave it.                |
| `fallback-required`       | warning | Every row of the Failure modes table fills its fallback cell, with `None.` where there is none.       |
| `no-credentials`          | error   | Nothing reads as a token, key, password or connection string.                                         |

<!-- END GENERATED: checks-integrations -->
