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

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `planned` · `building` · `live` · `deprecated`.
* **`implemented-by`** — Service ids.
* **`feature-files`** — Repo-relative paths, checked in both directions — a path that doesn't exist fails, and a feature
  file claimed by no capability is reported.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what a customer gets from this.

## What it does

The customer-visible behaviour, in customer terms. Two or three paragraphs at most.

## Why it exists

The problem it solves and who for. This is the part nothing else in the wiki holds — the ADO epics describe *what* gets
built, not *why the surface exists at all*.

## Surfaces

Where a customer encounters this: the web UI, the admin screen, the API endpoint, the email.

## Where the detail lives

|                    |                                                                      |
|--------------------|----------------------------------------------------------------------|
| **Implemented by** | [svc-example](/services/example.md), [svc-other](/services/other.md) |
| **Specified in**   | ADO epics #NNNN, #NNNN                                               |
| **Tested by**      | `repo/path/to/feature.feature`                                       |
| **Constrained by** | [nfr-example](/nfrs/example.md)                                      |
| **Decided in**     | [adr-NNNN]                                                           |

_(This table is the point of the document. If the prose above it grows longer than the links below it, ask whether what
you are writing belongs in an ADO item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[adr-NNNN]: /adrs/nnnn-kebab-slug.md
