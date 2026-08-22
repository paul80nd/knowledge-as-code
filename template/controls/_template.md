---
id: ctl-{{nnnn}}
tier: normative
status: planned
verifies:
mechanism:
frequency:
evidence:
applies-to:
owner:
tags: [ a, b ]
---

# {{Title}}

`Control: ctl-{{nnnn}}` `PLANNED`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a control adds to that.

**Frontmatter**

* **`status`** — `active` · `planned` · `retired`.
* **`verifies`** — Standard ids, ideally rule-level anchors.
* **`mechanism`** — `ci` · `review-checklist` · `manual-periodic` · `runtime-alert` · `not-enforced`. Pick the real one:
  `not-enforced` is a first-class value and the whole point of the coverage report, so do not invent a mechanism to
  avoid using it.
* **`frequency`** — `per-pr` · `per-deploy` · `daily` · `monthly` · `quarterly` · `annual`.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this control proves.

## What it checks

The rules this control verifies, quoted or anchored:

* [std-{{a}}] — "Services **MUST** …"
* [std-{{b}}] — "Responses **MUST NOT** …"

_(Name rules, not intentions. "We review carefully" is not a control; "the PR template requires a tick against each
conformance checklist item" is.)_

## How it works

The mechanism, concretely. Which pipeline step, which tool, which checklist, which alert rule. Enough that someone can
find it and confirm it is running.

## Evidence

Where the proof lives — the build log, the audit note, the dashboard. Someone asking "how do you know?" should be able
to follow this to an artefact.

## Coverage and gaps

What this control does **not** catch. Most controls are partial, and a stated gap is far more useful than an implied
guarantee.

_(If `mechanism` is `not-enforced`, this is the section that matters: say what would need to exist, and what the
exposure is meanwhile.)_

## Owner

Who is answerable for this control continuing to run, and who notices when it stops.

[std-{{a}}]: ../standards/{{a}}.md#{{anchor}}
[std-{{b}}]: ../standards/{{b}}.md#{{anchor}}
