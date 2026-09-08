---
id: rpt-{{slug}}
type: report
tier: descriptive
status: draft
owner:
generated:
sources:
confirmed:
tags:
---

# {{Title}}

`Report: rpt-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a report adds to that.

**Everything under the H1 comes from `kac report <name>`.** Run it, paste what it printed, and answer the judgement
cells it left open. Keep the section it prints about its own limits: a reader meets the numbers without the command
beside them.

**Frontmatter**

* **`generated`**: `{ at: "{{2026-09-08T10:00:00Z}}", by: kac/{{0.24.0}} }`. The moment the content was produced and
  what produced it. `by` names a producer and its version together, so a report an agent extended names the agent the
  same way the tool names itself.
* **`sources`**: one entry per corpus the run read, naming the corpus and the `content-version` it was at. Raise a
  version by hand where a corpus moved and nothing in this report changed, and add a `confirmed` entry saying you
  checked.
* **`confirmed`**: every confirmation this report has had, oldest first. A report nobody confirmed is output.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Limits

What this run could see, and what it could not. `kac report` writes this section.

## {{Totals}}

The counts, as the command printed them.

## {{The rows}}

One row per thing counted, with the judgement cells answered.

## What this leaves open

What the tool declined to decide, and what whoever confirmed this decided instead.
