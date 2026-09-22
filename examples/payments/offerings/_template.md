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

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what an offering adds to that.

**Frontmatter**

* **`implemented-by`**: service ids. The `Where the detail lives` list names the same ids, and
  `related-matches-section` reports either end naming one the other does not.
* **`feature-files`**: each path names its repository first, spelled as an entry of that repository's service
  `repos:`, then the path inside it. `feature-file-repo` warns where that first segment is no repository of a service
  you listed.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../offerings.md#metadata) lists
every field and says what each one holds.

**The work items.** They live in the list and nowhere else. This estate plans in GitHub issues, so an issue is
labelled `gh#2101`, with its link defined at the foot of the document.

**The NFRs.** Add an `nfrs:` key naming each one, and link the same ids from the `Constrained by` bullet. Both ends
are checked against each other, and the NFR names this offering back in its own `applies-to`. An offering at
`status: live` must name at least one: a customer already has it, and nothing else here says how well it has to work.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

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
* **Specified in**: [gh#{{a}}], [gh#{{b}}]
* **Constrained by**: [nfr-{{a}}]

_(This list is the point of the document. If the prose above it grows longer than the links in it, ask whether what
you are writing belongs in a work item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[gh#{{a}}]: https://git.example.com/example-payments/payment-api/issues/{{a}}
[gh#{{b}}]: https://git.example.com/example-payments/payment-api/issues/{{b}}
[nfr-{{a}}]: ../nfrs/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
[svc-{{b}}]: ../services/{{b}}.md
