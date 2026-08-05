---
id: prc-{{slug}}
tier: procedural
status: draft
applies-to:
last-rehearsed:
rehearsal-frequency:
requires-access:
owner:
tags: [ a, b ]
---

# {{Process name}}

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `active` · `draft` · `retired`.
* **`last-rehearsed`** — A quoted date, and `"never"` is permitted — preferable to a guess, since an unrehearsed
  procedure is a hypothesis. Update it when someone actually follows the process end to end, not when someone edits the
  document.
* **`rehearsal-frequency`** — `per-release` · `quarterly` · `annual`.
* **`requires-access`** — The systems or roles needed.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what this achieves and when you would do it.

## When to use this

The trigger. If you are here because something is broken, you probably want a [runbook](/runbooks) instead.

## Prerequisites

* Access to {{system}} — request from {{who}}.
* {{Tool}} installed at version {{x}}.
* {{Prior process}} completed.

_(Everything the reader needs before step 1. "Obtain the file from the repository owner" is not useful to someone who
doesn't know who that is — name them, or name the role.)_

## Steps

1. Do the thing. Imperative, one action per step.
2. Do the next thing.
    * Sub-steps where a step branches.

_(Assume the reader has not done this before. No hedging — "typically the order would be" is not followable. If the
order genuinely varies, say what it depends on.)_

## Verification

How you know it worked. Concretely — what you should see, where.

_(Not optional. A process that ends at the last action leaves the reader guessing.)_

## If it goes wrong

How to back out, or who to tell. If there is no rollback, say so explicitly — that is important information before step
1, not after.

## Related

* [svc-example](/services/example.md) — the service this operates on.
* [std-NNNN](/standards/…) — rules this process must respect.
