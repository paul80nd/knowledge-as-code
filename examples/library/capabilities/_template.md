---
id: cap-{{slug}}
type: capability
tier: descriptive
status: planned
implemented-by:
owner:
tags: [ a, b ]
---

# {{Capability name}}

`Capability: cap-{{slug}}` `PLANNED`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a capability adds to that.

**Frontmatter**

* **`status`**: `planned` · `building` · `live` · `deprecated`.
* **`implemented-by`**: service ids. The `Where the detail lives` table links the same ids, and
  `related-matches-section` reports either end naming one the other does not.
* **`feature-files`**: each path names its repository first, spelled as that repository's service spells `repo:`, then
  the path inside it. `feature-file-repo` warns where that first segment is no repository of a service you listed.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../capabilities.md#metadata) lists
every field and says what each one holds.

**The work items.** They live in the table and nowhere else. The consortium plans in Azure DevOps, so an epic is
written `ADO#1150` as an inline link to the work item.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what a customer gets from this.

## What it does

The customer-visible behaviour, in customer terms. Two or three paragraphs at most.

## Why it exists

The problem it solves and who for. This is the part nothing else here holds: a work item says *what* gets built, not
*why the surface exists at all*.

## Surfaces

Where a customer encounters this: the web UI, the admin screen, the API endpoint, the email.

## Where the detail lives

|                    |                                          |
|--------------------|------------------------------------------|
| **Implemented by** | [svc-{{a}}], [svc-{{b}}]                 |
| **Specified in**   | [ADO#{{a}}](https://dev.azure.com/example-libraries/consortium/_workitems/edit/{{a}}) |
|                    | [ADO#{{b}}](https://dev.azure.com/example-libraries/consortium/_workitems/edit/{{b}}) |

_(This table is the point of the document. If the prose above it grows longer than the links below it, ask whether what
you are writing belongs in a work item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[svc-{{a}}]: ../services/{{a}}.md
[svc-{{b}}]: ../services/{{b}}.md
