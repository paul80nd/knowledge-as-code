---
id: tol-<slug>
tier: descriptive
status: trial
category:
versions:
licence:
decided-in:
replaces:
owner:
tags: [ a, b ]
---

# <Tool name>

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`status`** — `approved` · `trial` · `deprecated` · `rejected`. Approved means approved **for new work**; something
  already in use but not approved is drift, and the drift report will say so.
* **`versions`** — A range, not a pin: the register states what we stand behind, the manifests state what is installed.
* **`licence`** — An SPDX identifier — the field nobody wants until they urgently do.
* **`decided-in`** — An ADR, where one exists.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence: what it is and what we use it for.

## What we use it for

Where it sits in the stack and which problem it solves for us specifically.

## Status

**<approved / trial / deprecated / rejected>** — since <date>.

For `trial`: what is being evaluated, where, and what would decide it. A trial with no decision criteria stays a trial
forever.

For `deprecated`: what replaces it and by when.

For `rejected`: this entry exists so the evaluation isn't repeated in eighteen months. Say what was wrong with it.

## Where it is used

* [svc-example](/services/example.md)

_(Generated drift detection will compare this against the actual package manifests once it exists — both directions.)_

## Alternatives considered

* **<Alternative>** — why it lost out.

_(Brief. If the choice was contested or expensive, the reasoning belongs in an [ADR](/adrs) and this cites it via
`decided-in`. Small, uncontroversial adoptions need only this section.)_

## Licence and obligations

<SPDX identifier>. Any attribution, copyleft or commercial-use obligations that follow from it.

## Related

* [std-NNNN](/standards/…) — standards that mandate or constrain its use.
* [adr-NNNN] — the decision, where there was one.

[adr-NNNN]: /adrs/nnnn-kebab-slug.md
