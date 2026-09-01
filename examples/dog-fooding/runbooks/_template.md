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

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a runbook adds to that.

**Frontmatter**

Title this document for the **failure**, not the fix. A reader at 2am searches for the failure.

* **`severity`**: `sev1` · `sev2` · `sev3`.
* **`last-rehearsed`**: a quoted date. `"never"` is permitted, and it is what you want to know before the incident
  rather than during it.
* **`requires-access`**: name every system and role the fix needs. Discovering you lack a permission mid-incident is its
  own outage.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Symptoms

* What you are seeing: alerts, error messages and customer reports, in the words they appear in.
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

_(A tree, not prose. Each branch ends in a resolution or an escalation, never in a dead end.)_

## Resolution

Steps to restore service, imperative and numbered. Then how to confirm it is actually restored.

## Escalation

| When          | Who             | How         |
|---------------|-----------------|-------------|
| {{condition}} | {{name / role}} | {{channel}} |

_(If this document is long, put this table near the top of the page. A reader has to find it without scrolling.)_

## Afterwards

* Raise a postmortem if severity warrants it.
* Update this runbook with anything that was wrong or missing.

## Related

* [svc-{{a}}] is the service this covers.
* [exp-{{a}}] explains how the system works. Read it afterwards, not now.

[exp-{{a}}]: ../explanations/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
