---
id: ofr-{{slug}}
type: offering
tier: descriptive
status: planned
implemented-by:
owner:
tags: [ a, b ]
---

# {{Offering name}}

`Offering: ofr-{{slug}}` `PLANNED`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what an offering adds to it. Add an
optional field from [the type page](../offerings.md#metadata) where you have a value for it.

**Frontmatter**

* **`implemented-by`**: service ids. The `Where the detail lives` list names the same ids, and
  `related-matches-section` reports either end naming one the other does not.
* **`feature-files`**: each path names its repository first, spelled as an entry of that repository's service
  `repos:`, then the path inside it. `feature-file-repo` warns where that first segment is no repository of a service
  you listed.

**The work items.** They live in the list and nowhere else. The consortium plans in Azure DevOps, so an epic is
labelled `ADO#1150`, with its link defined at the foot of the document.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what a customer gets from this.

## What it does

The customer-visible behaviour, in customer terms. Two or three paragraphs at most.

## Who it is for

The group of customers this is designed for, named the way they would name themselves. An offering that serves two
groups in two different ways is usually two offerings.

## Why it exists

The problem it solves. This is the part nothing else here holds: a work item says *what* gets built, not *why the
surface exists at all*.

## Surfaces

Where a customer encounters this: the web UI, the admin screen, the API endpoint, the email.

## Where the detail lives

* **Implemented by**: [svc-{{a}}], [svc-{{b}}]
* **Specified in**: [ADO#{{a}}], [ADO#{{b}}]

_(This list is the point of the document. If the prose above it grows longer than the links in it, ask whether what
you are writing belongs in a work item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[ADO#{{a}}]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/{{a}}
[ADO#{{b}}]: https://dev.azure.com/example-libraries/consortium/_workitems/edit/{{b}}
[svc-{{a}}]: ../services/{{a}}.md
[svc-{{b}}]: ../services/{{b}}.md
