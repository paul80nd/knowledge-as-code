---
id: tol-{{slug}}
tier: descriptive
status: trial
category:
versions:
licence:
decided-in:
replaces:
successor:
owner:
tags: [ a, b ]
---

# {{Tool name}}

`Tool: tol-{{slug}}` `TRIAL`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Read with [contributing](/knowledge-as-code/contributing.md)** — how links and ids are written, and how a contribution
is reviewed — and [authoring](/knowledge-as-code/authoring.md), where the prose rules follow the document's tier. What
is below is only what a tool adds to those.

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `approved` · `trial` · `deprecated` · `rejected`. Approved means approved **for new work**; something
  already in use but not approved is drift, and the drift report will say so.
* **`versions`** — A range, not a pin: the register states what we stand behind, the manifests state what is installed.
* **`licence`** — An SPDX identifier — the field nobody wants until they urgently do.
* **`decided-in`** — An ADR, where one exists.
* **`successor`** — What replaces this, once the status is `deprecated`. `replaces` is the same edge read the
  other way, and CI enforces both ends.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what it is and what we use it for.

## What we use it for

Where it sits in the stack and which problem it solves for us specifically.

## Status

**{{approved / trial / deprecated / rejected}}** — since {{date}}.

For `deprecated`: name the replacement in `successor`, and say by when.

For `rejected`: this entry exists so the evaluation isn't repeated in eighteen months. Say what was wrong with it.

## Trial criteria

_(Required while the status is `trial`, and deleted once it is not.)_

What is being evaluated, where it is being evaluated, and what would settle it either way. A trial with no
decision criteria stays a trial forever.

## Where it is used

* [svc-{{a}}]

_(Generated drift detection will compare this against the actual package manifests once it exists — both directions.)_

## Alternatives considered

* **{{Alternative}}** — why it lost out.

_(Brief. If the choice was contested or expensive, the reasoning belongs in an [ADR](/adrs) and this cites it via
`decided-in`. Small, uncontroversial adoptions need only this section.)_

## Licence and obligations

{{SPDX identifier}}. Any attribution, copyleft or commercial-use obligations that follow from it.

## Related

* [std-{{a}}] — standards that mandate or constrain its use.
* [adr-{{a}}] — the decision, where there was one.

[adr-{{a}}]: /adrs/{{a}}.md
[std-{{a}}]: /standards/{{a}}.md
[svc-{{a}}]: /services/{{a}}.md
