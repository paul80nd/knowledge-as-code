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
* **An explanation.** How the pipeline works is an [explanation](explanations.md). How to use it is a process.

## Level of detail and cadence

Each step states one outcome, with its reason where the reason is not obvious. The reader judges how to get there. A
command to run belongs in a runbook, a fix or the tool's own documentation. Answer a case a step does not cover by
sharpening that step's outcome or its reason.

A process changes when the task it describes changes, or when a rehearsal shows a step no longer works. New processes
arrive rarely.

## Metadata

<!-- BEGIN GENERATED: schema-processes -->

| Field                   | Value                                          | Notes                                                                         |
|-------------------------|------------------------------------------------|-------------------------------------------------------------------------------|
| `tier` *†               | `procedural`                                   | The record's trust level, fixed for the type and checked against the folder.  |
| `status` *†             | `active` `draft` `retired`                     | Whether the process is current, drafted, or stood down. In force at `active`. |
| `applies-to`            | list                                           | Service ids this process concerns.                                            |
| `last-rehearsed` *      | date                                           | Quoted. The day somebody last followed the process end to end, or `"never"`.  |
| `rehearsal-frequency` * | `on-change` `per-release` `quarterly` `annual` | How often, or on what event, to rehearse the process.                         |
| `requires-access`       | list                                           | The systems or roles the reader needs before step 1.                          |

\* Field is required  
† Every document has this field, and this type refines it.  
[Metadata](knowledge-as-code/metadata.md#universal-fields) lists the fields every document has.

<!-- END GENERATED: schema-processes -->

## Adding a process

1. Copy [`_template.md`](processes/_template.md) to `<slug>.md`. Processes use slug ids: `prc-releasing`.
2. Write the steps in order and in the imperative, for a reader who has not done this before.
3. Include prerequisites, a verification step ("you know it worked when…") and a rollback.
4. Set `last-rehearsed` and `rehearsal-frequency`. Write `"never"` rather than guess a date. Pick `quarterly` or
   `annual` where somebody keeps the cadence, and `on-change` or `per-release` where an event is the trigger.
5. Name the systems and roles in `requires-access`, and say who to ask for each. "Obtain the file from the repository
   owner" helps nobody who does not know the owner.

**Conventions**

* **Name the skills a step needs.** `Prerequisites` names them beside the access, the tools and the prior work. A
  reader who finds out at the step that they cannot do it has already changed something. Nothing checks it.
* **No hedging.** A step that opens "typically the order is…" leaves the reader to decide. Where the order varies, say
  what decides it.
* **Say what a step produces.** Put the result in the step itself, so a reader can tell it worked before starting the
  next one.
* **Open an optional step with `Optional:`.** Nothing else tells a reader which steps they may skip.
* **Put a warning before the step it guards.** A caution written after the action arrives too late to act on.
* **Verification is not optional.** A process that stops at the last action leaves the reader guessing whether it
  worked.
* **Say how to back out.** `If it goes wrong` is required. Where nothing undoes the process, write that, and warn the
  reader before step 1.
* **Rehearse before you trust.** Walk the process end to end before you rely on it. `last-rehearsed` records that
  walk-through and not the last edit. `staleness` warns on `"never"`, and on a `quarterly` or `annual` process past
  its window. It measures no event, so it reports an `on-change` or `per-release` process only while
  `last-rehearsed` is `"never"`.

## What CI checks

<!-- BEGIN GENERATED: checks-processes -->

CI runs the [universal checks](knowledge-as-code/metadata.md#universal-checks) on every record, and the checks below on
this type's records.

| Check                | Level   | What it verifies                                                                                      |
|----------------------|---------|-------------------------------------------------------------------------------------------------------|
| `ref-resolves`       | error   | An id in a field that references another document names one that exists, of the type the field names. |
| `staleness`          | warning | A process past its rehearsal frequency, or one whose `last-rehearsed` is `never`.                     |
| `no-hedged-ordering` | warning | No step hedged with "typically", "usually" or "normally".                                             |

<!-- END GENERATED: checks-processes -->
