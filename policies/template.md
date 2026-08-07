---
id: pol-XXXX
tier: normative
category: security
status: draft
aligns-with:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Policy: pol-XXXX` `DRAFT`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`id`** — `pol-` plus a four-character mnemonic for the policy's concept, upper-case here and lower-case in the
  filename: `pol-VURM` in `vurm-vulnerability-remediation.md`. **Immutable once the policy is active** — a change of
  meaning that big is a new policy and a retirement of this one.
* **`category`** — `security` · `delivery` · `operations` · `governance`. The broad area the commitment belongs to,
  which is a different question from the topics `tags` records.
* **`status`** — `draft` · `active` · `retired`.
* **`aligns-with`** — ISO/IEC 27001:2022 Annex A references, e.g. `ISO27001:2022 A.8.25`. This records **alignment, not
  compliance or certification**, and the wording matters if this is ever read externally.
* **`review-by`** — A quoted date. Annual is usually right for a policy.

A policy names no implementers. A standard points up at the policy it puts into practice, and a downstream corpus
inherits these policies to write its own standards against — so what implements this is not knowable from here.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Purpose

What we commit to, in one or two sentences, and why it matters. State the commitment plainly — a policy that hedges
isn't one.

_(Test before writing: would this still be true after replacing the entire technology estate? If it names a tool, a
framework or a protocol, it is a [standard](/standards), not a policy.)_

## Scope

Who and what this applies to. Be explicit about the boundary — "all product repositories", "any system processing
personal data", "production environments only".

## Commitments

* We **will** …
* We **will not** …

_(Principle-level. The specific, checkable rules belong in the implementing standards; this is the position they
implement.)_

## Alignment

| Reference                 | Area             |
|---------------------------|------------------|
| ISO/IEC 27001:2022 A.N.NN | {{control area}} |

The product is **aligned with** these areas; it is not certified against ISO/IEC 27001:2022 and is not audited.
Alignment exists because the framework covers the right ground.

_(Delete this section if no ISO area corresponds — an invented mapping is worse than none.)_

## Exceptions

Where this policy does not apply, and who can grant an exception. Exceptions stated up front are honest; exceptions
discovered later are erosion. If there are none, say so.

## Notes

Anything genuinely contextual about this policy — why it carries no framework alignment, how it relates to the rest of
the taxonomy, what it deliberately leaves to an implementing standard or process.

_(Optional, and it should stay that way. Delete the section rather than fill it: a note that would be true of every
policy is boilerplate, and the review point is already `review-by`.)_

[pol-XXXX]: xxxx-kebab-slug.md
[std-NNNN]: /standards/nnnn-kebab-slug.md

_(Link definitions, at the very foot, sorted by label. References to another document **by its id** use **shortcut
reference links** — write `[pol-DEVI]` in the prose and define it once here, so a rename is a one-line change. The label
is also the display text, so it must be the id exactly as that document carries it. Where you want prose link text
instead — "recorded as [NFRs](/nfrs)" — use an inline link and no definition.)_
