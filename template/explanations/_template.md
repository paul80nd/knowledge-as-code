---
id: exp-{{slug}}
type: explanation
tier: descriptive
status: draft
owner:
explains:
review-by:
---

# {{Title}}

`Explanation: exp-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what an explanation adds to it. Add
an optional field from [the type page](../explanations.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `exp-` plus a slug naming the subject. Save the file as `<slug>.md`, with no number prefix, because
  explanations are named, not sequenced.
* **`status`**: `stale` is an honest state. Use it where the page no longer matches what runs.
* **`explains`**: the ids of what this explains. A service, an offering, a standard, a process or an ADR.
* **`review-by`**: a quoted date. Explanations are the residual category, so they need the tightest staleness
  discipline, not the loosest.

**Required sections.** `What this covers` states what the record covers, and what it leaves to another record.
`Where the detail lives` lists the records with the facts. Everything between them is yours to shape.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what this explains and who it's for.

## What this covers

{{What this record covers}}. Say what it leaves to another record, and name that record.

## {{Section}}

Narrative. Link out for every concrete fact, to the services, offerings, standards, processes and ADRs that state it.
Draw the subject as a mermaid diagram where it has a shape, and label it.

## Where the detail lives

- [svc-{{a}}] describes {{what it describes}}.
- [adr-{{a}}] records {{what it decided}}.

[adr-{{a}}]: ../adrs/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
