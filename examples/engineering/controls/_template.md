---
id: ctl-{{nnnn}}
type: control
tier: normative
status: planned
verifies:
mechanism:
frequency:
review-by: "{{date}}"
owner:
tags: [ a, b ]
---

# {{Title}}

`Control: ctl-{{nnnn}}` `PLANNED`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a control adds to it. Add an
optional field from [the type page](../controls.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `ctl-` plus a four-digit number, as `ctl-0001`. Save the file as `{{nnnn}}-kebab-slug.md`.
* **`verifies`**: the rules this control checks, as `verifies: [ std-SECRET.ROTATE ]`. A bare standard id is refused, so
  a control checking every rule of a standard lists every rule. The standard names the control once in `verified-by`.
  When the way a rule is checked changes, update this control and leave the standard as it is.
* **`mechanism`**: pick the one that runs today. `not-enforced` is a first-class value and the whole point of the
  coverage report, so do not invent a mechanism to avoid using it. Write the control for an unenforced rule all the
  same, because an unenforced rule you know about is worth more than one you do not.
* **`last-verified`**: the last time somebody ran the check, as `{ at: "2026-10-20", by: human:alex.doe }`, or
  `never`. Add it where `mechanism` is `manual`. `by` is a person or an agent with its version, because a check a
  process runs is a `ci` control.
* **`evidence`**: where the proof lives, such as the build log, the audit note or the dashboard. Someone asking "how do
  you know?" should be able to follow it to an artefact. Add it unless `mechanism` is `not-enforced`.
* **`review-by`**: the day somebody re-reads this record against the check, quoted. `control-in-date` warns once it
  has passed.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this control proves.

## What it checks

The rules this control verifies, quoted or anchored:

* [std-{{a}}.{{ID}}] says "Services **MUST** …"
* [std-{{b}}.{{ID}}] says "Responses **MUST NOT** …"

_(Name rules, not intentions. "We review carefully" is not a control; "the PR template requires a tick against each
conformance checklist item" is.)_

## How it works

The mechanism, concretely. Which pipeline step, which tool, which checklist, which alert rule. Enough that someone can
find it and confirm it is running.

## Coverage and gaps

What this control does **not** catch. Most controls are partial, and a stated gap is far more useful than an implied
guarantee.

_(If `mechanism` is `not-enforced`, this is the section that matters: say what would need to exist, and what the
exposure is meanwhile.)_

[std-{{a}}.{{ID}}]: ../standards/{{a}}.md#{{anchor}}
[std-{{b}}.{{ID}}]: ../standards/{{b}}.md#{{anchor}}
