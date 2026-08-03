# Runbooks

What to do when something is broken.

**[→ Index](runbooks/INDEX.md)**

## What is a runbook?

An incident-time procedure, read under pressure by someone who may not have seen this failure before. Symptoms first,
then immediate actions, then a diagnosis tree, then resolution and escalation.

Terse, imperative, and structured so the reader can find their branch without reading the whole document.

## Why we use them

At 2am nobody reconstructs a recovery sequence from first principles, and the person who knows it is asleep. A runbook
is the difference between a twenty-minute incident and a two-hour one.

They also make the untested assumption visible. `last-rehearsed: "never"` on a disaster-recovery runbook is a much more
useful thing to know before the disaster than after it.

## Scope

**Broken, not planned.** If you are doing it because you decided to, it is a [process](/processes).

Runbooks sit next to two other types and the boundaries are worth holding:

* **[FAQ](/faqs)** — a known problem with a known fix, usually one or two steps, no urgency. If it needs a diagnosis
  tree and an escalation path, it is a runbook.
* **[Postmortem](/postmortems)** — an account of an incident that happened. A runbook is instructions for one that
  might. A good postmortem frequently produces a runbook.

Disaster recovery and "ground-zero the estate" belong here, not in processes, however planned the rehearsal is — the
document is written for the day it isn't.

## Metadata

<!-- BEGIN GENERATED: schema-runbooks -->

| Field                 | Req | Type | Notes                                      |
|-----------------------|-----|------|--------------------------------------------|
| `status`              | ●   | enum | `active` · `draft` · `retired`             |
| `applies-to`          |     | list | Service ids                                |
| `last-rehearsed`      | ●   | date | Quoted. `"never"` is permitted and honest. |
| `rehearsal-frequency` |     | enum | `per-release` · `quarterly` · `annual`     |
| `requires-access`     |     | list | Systems or roles needed                    |
| `severity`            |     | enum | `sev1` · `sev2` · `sev3`                   |

<!-- END GENERATED: schema-runbooks -->

## Adding a runbook

1. Copy [`template.md`](runbooks/template.md) to `<slug>.md`. Runbooks use slug ids — `rbk-estate-rebuild`.
2. Lead with **symptoms** — what the reader is seeing. That is how they will find this document.
3. Give immediate actions before diagnosis. Stop the bleeding, then work out why.
4. Structure the diagnosis as a tree, not prose. Each branch ends in a resolution or an escalation.
5. Put the escalation path where it can be found without scrolling.
6. Set `last-rehearsed` honestly, and `requires-access` completely — discovering you lack a permission mid-incident is
   its own outage.

**Conventions**

* **Short sentences, imperative mood.** No background, no rationale. Link to the [explanation](/explanations) if the
  reader needs the theory afterwards.
* **No prerequisites the reader can't satisfy at 2am.** If a step needs someone else's approval, say who and how to
  reach them.
* **Rehearse on a schedule.** An unrehearsed runbook is flagged by the staleness report, loudly, and that is deliberate.

## What CI checks

<!-- BEGIN GENERATED: checks-runbooks -->

_No automated checks yet — see [Automation](/knowledge-as-code/automation.md)._

<!-- END GENERATED: checks-runbooks -->
