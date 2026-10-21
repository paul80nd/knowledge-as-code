# Runbooks

What to do when something is broken.

**[→ Index](runbooks/_index.md)**

## What is a runbook?

An incident-time procedure, read under pressure by someone who may not have seen this failure before. It opens on
the symptoms, so a reader can tell at once that this is the right document. Impact, immediate actions, a diagnosis
tree, resolution, escalation and communication follow in that order.

Its structure lets the reader find their branch without reading the whole document.

## Why we use them

At 2am nobody reconstructs a recovery sequence from first principles, and the person who knows it is asleep. A runbook
is the difference between a twenty-minute incident and a two-hour one.

Writing one also exposes the step nobody has tried. A disaster-recovery runbook carrying `last-rehearsed: "never"` says
the recovery has never been run, while there is still time to run it.

## Scope

**Broken, not planned.** If you are doing this because you decided to, it is a [process](processes.md).

Two other types sit close enough to confuse:

* **[Fix](fixes.md)**: a known problem with a known resolution, usually one or two steps, no urgency. If it needs a
  diagnosis tree and an escalation path, it is a runbook.
* **[Postmortem](postmortems.md)**: an account of an incident that happened. A runbook gives instructions for an
  incident that has not happened yet. A good postmortem frequently produces a runbook.

Disaster recovery and rebuilding the estate from nothing belong here. You rehearse them deliberately, which makes them
look like processes. You open the document on a day when the estate is already down.

## Level of detail and cadence

Each step is one command or action the reader can run, and says what they see when it worked. Each diagnosis branch
ends in a resolution or an escalation. Background and theory belong in an explanation.

A runbook changes after each incident that uses it, and after each rehearsal. New runbooks arrive rarely, often from a
postmortem's actions.

## Metadata

<!-- BEGIN GENERATED: schema-runbooks -->

| Field                   | Value                                          | Notes                                                                                                                                        |
|-------------------------|------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `tier` *†               | `procedural`                                   | The record's trust level, fixed for the type and checked against the folder.                                                                 |
| `status` *†             | `active` `draft` `retired`                     | Whether the runbook is current, drafted, or stood down. In force at `active`.                                                                |
| `applies-to`            | list                                           | Service ids this runbook covers.                                                                                                             |
| `severity` *            | `sev1` `sev2` `sev3`                           | The severity this runbook is written for.                                                                                                    |
| `last-rehearsed` *      | date                                           | Quoted. The day somebody last walked the runbook end to end, or `"never"`.                                                                   |
| `rehearsal-frequency` * | `on-change` `per-release` `quarterly` `annual` | How often, or on what event, to rehearse the runbook.                                                                                        |
| `rehearsal-method`      | `live` `tabletop`                              | Which walk the date records: `live` caused the fault, `tabletop` read the steps against the system. Required when `last-rehearsed != never`. |
| `requires-tools`        | list                                           | The tools the reader needs installed before starting.                                                                                        |
| `requires-access`       | list                                           | The permissions or roles the reader needs before starting.                                                                                   |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-runbooks -->

## Adding a runbook

Copy [`_template.md`](runbooks/_template.md). Its guidance block states how to write the id, the title and each field.
The prompt under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-runbooks -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                                          | Level   | What it verifies                                                                                      |
|------------------------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `ref-resolves`                                 | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `diagnosis-dead-end / diagnosis-no-escalation` | error   | Every diagnosis branch routes to a section or falls through, and one branch reaches `Escalation`.     |
| `symptoms-first`                               | error   | Symptoms is the first section after the H1.                                                           |
| `outcome-stated`                               | error   | Resolution contains a line opening `Confirmed when`.                                                  |
| `failure-route-stated`                         | error   | Resolution links to Escalation for a reader whose step did not work.                                  |
| `staleness-loud`                               | warning | A runbook nobody has walked, or one walked more than 92 days ago.                                     |

<!-- END GENERATED: checks-runbooks -->
