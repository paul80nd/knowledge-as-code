# Processes

How to perform the planned tasks that keep the platform running.

**[→ Index](processes/_index.md)**

## What is a process?

A procedure you follow **deliberately**: onboarding a new developer, cutting a release, provisioning an environment,
rotating a secret. A process gives numbered steps, prerequisites, a verification step and a rollback.

## Why we use them

Of everything we know, a procedure is the most likely to live in one person's head, and the most expensive to lose.
Nobody finds out that a step everyone "just knows" stopped working three months ago until someone writes the process
down.

Processes carry `last-rehearsed` for that reason. A procedure nobody has walked through is a hypothesis.

## Scope

**Are you doing this because you planned to, or because something is broken?**

* Planned: a process.
* Broken: a [runbook](runbooks.md).

A process and a runbook have different readers, a different tone, and different consequences when they go stale. A
process that is slightly out of date is annoying. A runbook that is slightly out of date is dangerous.

A process is also not:

* **A rule**: "deployments happen in dependency order" is a [standard](standards.md). The release process cites it.
* **A reference list.** System requirements and port tables belong in the [service](services.md) catalogue. A document
  with no steps is not a process.
* **An explanation.** How the pipeline works is an explanation. How to use it is a process.

## Level of detail and cadence

Each step states one outcome, with its reason where the reason is not obvious. The reader judges how to get there. A
command to run belongs in the tool's own documentation, or in a runbook or a fix where something is broken. Answer a
case a step does not cover by sharpening that step's outcome or its reason.

A process changes when the task it describes changes, or when a rehearsal shows a step no longer works. New processes
arrive rarely.

## Metadata

<!-- BEGIN GENERATED: schema-processes -->

| Field                   | Value                                          | Notes                                                                                   |
|-------------------------|------------------------------------------------|-----------------------------------------------------------------------------------------|
| `tier` *†               | `procedural`                                   | The record's trust level, fixed for the type and checked against the folder.            |
| `status` *†             | `active` `draft` `retired`                     | Whether the process is current, drafted, or stood down. In force at `active`.           |
| `applies-to`            | list                                           | Service ids this process concerns.                                                      |
| `last-rehearsed` *      | object                                         | The last time somebody followed the process end to end, as the day and who. Or `never`. |
| `rehearsal-frequency` * | `on-change` `per-release` `quarterly` `annual` | How often, or on what event, to rehearse the process.                                   |
| `requires-access`       | list                                           | The systems or roles the reader needs before step 1.                                    |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-processes -->

## Adding a process

Copy [`_template.md`](processes/_template.md). Its guidance block states how to write the id and each field. The prompt
under each section states how to write that section.

## What CI checks

<!-- BEGIN GENERATED: checks-processes -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                           | Level   | What it verifies                                                                                      |
|---------------------------------|---------|-------------------------------------------------------------------------------------------------------|
| `entry-shape / entry-key`       | error   | An object field, and each entry of an object list, carries the keys the field declares and no others. |
| `ref-resolves`                  | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `staleness`                     | warning | A process past its rehearsal frequency, or one whose `last-rehearsed` is `never`.                     |
| `followed-by-a-person-or-agent` | error   | A walk-through states a person or an agent, and never a process.                                      |
| `rehearsal-states-no-method`    | error   | A walk-through states no `method`, which belongs to a runbook.                                        |
| `no-hedged-ordering`            | warning | No step hedged with "typically", "usually" or "normally".                                             |

<!-- END GENERATED: checks-processes -->
