---
id: cap-{{slug}}
tier: descriptive
status: planned
implemented-by:
ado-epics:
feature-files:
nfrs:
owner:
tags: [ a, b ]
---

# {{Capability name}}

`Capability: cap-{{slug}}` `PLANNED`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](/knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a capability adds to that.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `planned` · `building` · `live` · `deprecated`.
* **`implemented-by`** — Service ids.
* **`feature-files`** — Repo-relative paths, checked in both directions — a path that doesn't exist fails, and a feature
  file claimed by no capability is reported.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what a customer gets from this.

## What it does

The customer-visible behaviour, in customer terms. Two or three paragraphs at most.

## Why it exists

The problem it solves and who for. This is the part nothing else here holds — the ADO epics describe *what* gets
built, not *why the surface exists at all*.

## Surfaces

Where a customer encounters this: the web UI, the admin screen, the API endpoint, the email.

## Where the detail lives

|                    |                                |
|--------------------|--------------------------------|
| **Implemented by** | [svc-{{a}}], [svc-{{b}}]       |
| **Specified in**   | ADO epics #{{epic}}, #{{epic}} |
| **Tested by**      | `{{repo}}/{{path}}.feature`    |
| **Constrained by** | [nfr-{{a}}]                    |
| **Decided in**     | [adr-{{a}}]                    |

_(This table is the point of the document. If the prose above it grows longer than the links below it, ask whether what
you are writing belongs in an ADO item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[adr-{{a}}]: /adrs/{{a}}.md
[nfr-{{a}}]: /nfrs/{{a}}.md
[svc-{{a}}]: /services/{{a}}.md
[svc-{{b}}]: /services/{{b}}.md
