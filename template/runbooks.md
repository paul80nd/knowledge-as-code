# Runbooks

What to do when something is broken.

**[→ Index](runbooks/_index.md)**

## What is a runbook?

An incident-time procedure, read under pressure by someone who may not have seen this failure before. It opens on
the symptoms, so a reader can tell at once that this is the right document. Impact, immediate actions, a diagnosis
tree, resolution, escalation and communication follow in that order.

Keep it terse and imperative, and structure it so the reader finds their branch without reading the whole document.

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

1. Copy [`_template.md`](runbooks/_template.md) to `<slug>.md`. Runbooks use slug ids: `rbk-estate-rebuild`.
2. Lead with **symptoms**: what the reader is seeing. That is how they find this document.
3. State the **impact** next: who cannot do what while this is broken. `severity` says how urgent, and this says who.
4. Give the immediate actions before the diagnosis. Stop the bleeding, then work out why.
5. Structure the diagnosis as a tree, not prose. Each branch ends in a resolution or an escalation.
6. End the **resolution** with a line opening `Confirmed when`. Say how the reader knows service is back, then link
   to the escalation for a reader whose step did not work.
7. Put the escalation path where the reader finds it without scrolling.
8. Name in **communication** who is waiting and what they need. Escalation wakes the people who can help.
9. Set `last-rehearsed` and `rehearsal-frequency` honestly, and `rehearsal-method` once somebody has walked it. List
   tools in `requires-tools` and permissions in `requires-access`.

**Conventions**

* **Short sentences, imperative mood.** No background, no rationale. Link to an [explanation](explanations.md) where the
  reader wants the theory afterwards.
* **No prerequisite the reader cannot satisfy at 2am.** Where a step needs someone else's approval, name who and how to
  reach them.
* **Rehearse on a cadence, or on the change that breaks it.** Every runbook sets `rehearsal-frequency`, and
  `last-rehearsed` records the last time someone walked it. `staleness-loud` warns at 92 days whichever cadence you
  set, because a runbook is read during an incident.

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
