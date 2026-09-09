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

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what an explanation adds to that.

**Frontmatter**

* **`status`**: `draft` · `active` · `stale`. `stale` is an honest state. Say so rather than let the page quietly rot.
* **`explains`**: the service or capability ids this explains.
* **`review-by`**: a quoted date. Explanations are the residual category, so they need the tightest staleness
  discipline, not the loosest.

**Fields this template leaves out.** `tags` is optional, so the frontmatter above does not carry it. Add a key where you
have a value for it, and leave it out where you do not. [The type page](../explanations.md#metadata) says what each one
holds.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what this explains and who it's for.

## {{Section}}

Narrative. Link out to the services, capabilities, ADRs and standards that hold the detail rather than restating them
here.

## Where the detail lives

- [{{Service}}](../services/{{a}}.md) holds {{what it holds}}.
- [adr-{{a}}] records {{what it decided}}.

[adr-{{a}}]: ../adrs/{{a}}.md
