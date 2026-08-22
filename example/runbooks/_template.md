---
id: rbk-{{slug}}
tier: procedural
status: draft
applies-to:
severity:
last-rehearsed:
rehearsal-frequency:
requires-access:
owner:
tags: [ a, b ]
---

# {{What is broken}}

`Runbook: rbk-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a runbook adds to that.

**Frontmatter.** Delete this block once the fields above are filled in.

Title this document for the **failure**, not the fix — that is what someone searches for at 2am.

* **`severity`** — `sev1` · `sev2` · `sev3`.
* **`last-rehearsed`** — A quoted date; `"never"` is permitted, and is exactly the thing worth knowing before the
  incident rather than during it.
* **`requires-access`** — Must be complete. Discovering you lack a permission mid-incident is its own outage.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Symptoms

* What you are seeing — alerts, error messages, customer reports, in the words they actually appear in.
* What you are **not** seeing, where its absence is diagnostic.

_(Symptoms come first because that is how the reader finds this document.)_

## Immediate actions

1. {{Stop the bleeding.}}
2. {{Notify whom.}}

_(Before diagnosis. Contain first, understand afterwards.)_

## Diagnosis

**Is {{condition}}?**

* **Yes** → {{action}}, then go to [Resolution](#resolution).
* **No** → continue.

**Is {{next condition}}?**

* **Yes** → {{action}}.
* **No** → [escalate](#escalation).

_(A tree, not prose. Each branch ends in a resolution or an escalation — never in a dead end.)_

## Resolution

Steps to restore service, imperative and numbered. Then how to confirm it is actually restored.

## Escalation

| When          | Who             | How         |
|---------------|-----------------|-------------|
| {{condition}} | {{name / role}} | {{channel}} |

_(Near the top of the page if this document is long — it must be findable without scrolling.)_

## Afterwards

* Raise a [postmortem](../postmortems.md) if severity warrants it.
* Update this runbook with anything that was wrong or missing.

## Related

* [svc-{{a}}] — the service this covers.
* [exp-{{a}}] — how the system works, for afterwards. Not now.

[exp-{{a}}]: ../explanations/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
