---
id: tol-{{slug}}
type: tool
tier: descriptive
status: trial
homepage: https://{{project}}
licence: "{{SPDX}}"
decided-on: "{{date}}"
review-by: "{{date}}"
owner:
tags: [ a, b ]
---

# {{Tool name}}

`Tool: tol-{{slug}}` `TRIAL`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a tool adds to it. Add an
optional field from [the type page](../tools.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `tol-` plus a slug naming the tool, as `tol-vitest`. Save the file as `<slug>.md`.
* **`status`**: approved means approved **for new work**. Something already in use but not approved is drift, and
  finding it is a manual job until something implements `drift-against-manifests`. `trial` covers something being
  evaluated in one place. Promote or reject it once the evaluation ends.
* **`packages`**: one entry per package this entry approves, each a
  [package URL](https://github.com/package-url/purl-spec) with the range approved for new work. Give a range, not a
  pin: the register states what we stand behind, and the manifests state what is installed. Leave `versions` out
  where nothing pins the package, and leave `packages` out where nothing distributes the tool.
* **`homepage`**: the project's own page, so a reader reaches the project and not a fork.
* **`licence`**: the licence you concluded applies, as an SPDX expression. Write `NOASSERTION` where nobody has
  checked yet, so the gap shows.
* **`licence-declared`**: what the package's own licence files state, written as they state it. Fill it wherever you
  read them, including where your conclusion matches. Where the two differ, `conclusion-states-a-reason` asks for a
  `## Licence and obligations` section.
* **`decided-on`**: the day the current `status` was decided.
* **`review-by`**: the day somebody checks this entry is still right. `review-in-date` warns once it has passed.
* **`decided-in`**: an ADR, where one exists. Where the choice was contested and no ADR exists, write one.
* **`replaces`**: the older tools this takes over from, one id per line, so the deprecation path is visible.
* **`successor`**: what replaces this, once the status is `deprecated`. `replaces` is the same edge read the other way,
  and CI enforces both ends.

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

_(Required while the tool's status is `trial`, and deleted once it is not.)_

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

What you read, what you concluded from it, and any attribution, copyleft or commercial-use obligations that follow.

## Related

* [std-{{a}}] mandates or constrains its use.

[std-{{a}}]: ../standards/{{a}}.md
[svc-{{a}}]: ../services/{{a}}.md
