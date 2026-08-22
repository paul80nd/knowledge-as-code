---
id: exp-{{slug}}
tier: descriptive
status: draft
owner:
explains:
review-by:
tags:
---

# {{Title}}

`Explanation: exp-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what an explanation adds to that.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `draft` · `active` · `stale`. `stale` is an honest state — say so rather than let the page quietly rot.
* **`explains`** — The service or capability ids this explains.
* **`review-by`** — A quoted date. Explanations are the residual category, so they need the tightest staleness
  discipline, not the loosest.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what this explains and who it's for.

## {{Section}}

Narrative. Link out to the services, capabilities, ADRs and standards that hold the detail rather than restating them
here.

## Where the detail lives

- [{{Service}}](../services/{{a}}.md) — {{what it holds}}
- [adr-{{a}}] — {{what it decided}}

[adr-{{a}}]: ../adrs/{{a}}.md
