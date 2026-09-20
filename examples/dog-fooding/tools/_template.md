---
id: tol-{{slug}}
type: tool
tier: descriptive
status: trial
homepage: https://{{project}}
decided-on: "{{date}}"
review-by: "{{date}}"
owner:
tags: [ a, b ]
---

# {{Tool name}}

`Tool: tol-{{slug}}` `TRIAL`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a tool adds to that.

**Frontmatter**

* **`status`**: `approved` · `trial` · `deprecated` · `rejected`. Approved means approved **for new work**; something
  already in use but not approved is drift, and the drift report will say so.
* **`packages`**: one entry per package this entry approves, each a
  [package URL](https://github.com/package-url/purl-spec) with the range approved for new work. Give a range, not a
  pin: the register states what we stand behind, and the manifests state what is installed. Leave `versions` out
  where nothing pins the package, and leave `packages` out where nothing distributes the tool.
* **`homepage`**: the project's own page, so a reader reaches the project and not a fork.
* **`licence`**: an SPDX identifier. Nobody wants the field until they urgently do.
* **`decided-on`**: the day the current `status` was decided.
* **`review-by`**: the day somebody checks this entry is still right. `review-in-date` warns once it has passed.
* **`decided-in`**: an ADR, where one exists.
* **`successor`**: what replaces this, once the status is `deprecated`. `replaces` is the same edge read the other way,
  and CI enforces both ends.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../tools.md#metadata) lists every
field and says what each one holds.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what it is and what we use it for.

## What we use it for

Where it sits in the stack and which problem it solves for us specifically.

## Status

_(Optional. Why the tool sits where it does, which `status` and `decided-on` cannot say. Delete it where an approval
has nothing to add.)_

`exit-states-a-reason` asks for this section once the status is `deprecated` or `rejected`. For `deprecated`: say what
took over, and name it in `successor`. For `rejected`: say what was wrong with it, so nobody runs the same evaluation
in two years.

## Trial criteria

_(Required while the status is `trial`, and deleted once it is not.)_

What is being evaluated, where it is being evaluated, and what would settle it either way. A trial with no decision
criteria stays a trial forever.

## Accessibility

_(Optional, and deleted where this tool renders nothing a person reads. A standard in this corpus may require it of a
component that renders a governed surface.)_

What was checked, how, and the version it was checked against. Then what falls short, or that nothing found does.

## Where it is used

* [svc-{{a}}]

_(Generated drift detection will compare this against the actual package manifests in both directions, once it exists.)_

## Alternatives considered

* **{{Alternative}}**: why it lost out.

_(Brief. If the choice was contested or expensive, the reasoning belongs in an ADR and this cites it via
`decided-in`. Small, uncontroversial adoptions need only this section.)_

## Licence and obligations

{{SPDX identifier}}. Any attribution, copyleft or commercial-use obligations that follow from it.

## Related

* [std-{{a}}] mandates or constrains its use.
* [adr-{{a}}] records the decision, where there was one.

[adr-{{a}}]: ../adrs/{{a}}.md
[std-{{a}}]: ../standards/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
