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

**Link out.** Do not say how the offering works. Link the services and explanations that already do.
`hub-not-specification` weighs the whole document against its outbound links, at roughly forty words each, so an
offering that grows a section of its own trips it.

**Frontmatter**

Keep the slug to 30 characters at most: the filename without `.md` and without any number or mnemonic at its
head. Write each list field's entries in alphabetical order.

* **`id`**: `ofr-` plus a slug naming the offering. Save the file as `<slug>.md`.
* **`implemented-by`**: service ids. The `Where the detail lives` list names the same ids, and
  `related-matches-section` reports either end naming one the other does not.
* **`feature-files`**: each path names its repository first, spelled as an entry of that repository's service
  `repos:`, then the path inside it. `feature-file-repo` warns where that first segment is no repository of a service
  you listed. Past that first segment the field is a plain string: `ref-resolves` never sees it, and
  `feature-file-orphans` is declared and does not run. Keep the rest of the path correct yourself.

**The work items.** They live in the list, because there is no field for them. Label each one the way your tracker
labels it, and define the link at the foot of the document. `ADO#1150` and `gh#2101` both read well. The framework names
no tracker. A consumer reading an export sees the label and not the address, because a section travels and its link
definitions do not.

**The NFRs.** Where this corpus adopts `nfrs`, add a `Constrained by` bullet linking each id, and add the ids to an
`nfrs:` key in the frontmatter. Both ends are checked against each other, and an offering at `status: live` must name
at least one. A corpus that declined `nfrs` is not asked for any.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One or two sentences on what a customer gets from this.

## What it does

The customer-visible behaviour, in customer terms. Two or three paragraphs at most.

## Who it is for

The group of customers this is designed for, named the way they would name themselves. An offering that serves two
groups in two different ways is two offerings, however much they share underneath.

## Why it exists

The problem it solves. This is the part nothing else here holds: a work item says *what* gets built, not *why the
surface exists at all*.

## Surfaces

Where a customer encounters this: the web UI, the admin screen, the API endpoint, the email.

## Where the detail lives

* **Implemented by**: [svc-{{a}}], [svc-{{b}}]
* **Specified in**: [{{tracker}}#{{a}}], [{{tracker}}#{{b}}]

_(This list is the point of the document. If the prose above it grows longer than the links in it, ask whether what
you are writing belongs in a work item instead.)_

## Known limitations

What it deliberately does not do, and anything a reader would otherwise assume works. Link to work items where a
limitation is scheduled to change.

[svc-{{a}}]: ../services/{{a}}.md
[svc-{{b}}]: ../services/{{b}}.md
[{{tracker}}#{{a}}]: {{url}}
[{{tracker}}#{{b}}]: {{url}}
