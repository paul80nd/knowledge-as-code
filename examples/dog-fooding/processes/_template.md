---
id: prc-{{slug}}
type: process
tier: procedural
status: draft
last-rehearsed:
rehearsal-frequency:
owner:
tags: [ a, b ]
---

# {{Process name}}

`Process: prc-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a process adds to it. Add an
optional field from [the type page](../processes.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `prc-` plus a slug naming the task, as `prc-releasing`. Save the file as `<slug>.md`.
* **`last-rehearsed`**: the last time somebody followed the process end to end, as
  `{ at: "2026-10-20", by: human:alex.doe }`. `at` is the day, quoted. `by` is a person as `human:alex.doe`, or an
  agent with its version. Write `never` rather than guess, because an unrehearsed procedure is a hypothesis. Update it
  when someone follows the process end to end, not when someone edits the document.
* **`rehearsal-frequency`**: pick `quarterly` or `annual` where somebody keeps the cadence, and `staleness` measures
  this date against it. Pick `on-change` or `per-release` where an event is the trigger. No window measures an event,
  so `staleness` reports a process using either only while `last-rehearsed` is `never`.
* **`requires-access`**: the systems or roles the reader needs. `Prerequisites` says who to ask for each.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this achieves and when you would do it.

## When to use this

The trigger. If you are here because something is broken, you probably want a [runbook](../runbooks.md) instead.

## Prerequisites

* Access to {{system}}, requested from {{who}}.
* {{Tool}} installed at version {{x}}.
* {{Prior process}} completed.
* {{Skill}}, which step {{n}} needs.

_(Everything the reader needs before step 1: the access, the tools, the prior work and the skills. "Obtain the file
from the repository owner" is not useful to someone who doesn't know who that is. Name them, or name the role. A
reader who finds out at the step that they cannot do it has already changed something.)_

## Steps

1. Do the thing. Imperative, one action per step. {{What the reader sees when it worked}}.
2. Do the next thing.
3. Optional: do this only where {{condition}}.

* Sub-steps where a step branches.

_(Assume the reader has not done this before. State what a step produces, in the same step, so the reader can tell it
worked. Open an optional step with `Optional:`. Put a warning before the step it guards. Do not hedge: "typically the
order would be" is not followable. Where the order varies, say what decides it.)_

## Verification

How you know it worked. Say what the reader should see, and where.

_(Not optional. A process that ends at the last action leaves the reader guessing.)_

## If it goes wrong

How to back out, or who to tell. Where there is no way back, say so here, and warn the reader before step 1 as well.

_(Not optional. A reader reads this before step 1, to decide whether to start at all.)_

## Related

* [svc-{{a}}] is the service this operates on.
* [std-{{a}}] carries the rules this process must respect.

[std-{{a}}]: ../standards/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
