---
id: pol-XXXX
tier: normative
category: security
status: draft
aligns-with:
implemented-by:
review-by:
owner:
tags: [ a, b ]
---

# Policy: {{Title}}

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
* **`implemented-by`** — The standard ids that put this into practice.
* **`review-by`** — A quoted date. Annual is usually right for a policy.

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

## Implemented by

* [std-NNNN] — {{what it requires}}.

_(A policy nothing implements is a statement of intent. That is allowed — mark it aspirational and say why the standard
doesn't exist yet.)_

## Review

Reviewed {{frequency}} by {{role}}. Last reviewed: {{date}}.

[pol-XXXX]: xxxx-kebab-slug.md
[std-NNNN]: /standards/nnnn-kebab-slug.md

_(Link definitions, at the very foot, sorted by label. References to another document **by its id** use **shortcut
reference links** — write `[pol-DEVI]` in the prose and define it once here, so a rename is a one-line change. The label
is also the display text, so it must be the id exactly as that document carries it. Where you want prose link text
instead — "recorded as [NFRs](/nfrs)" — use an inline link and no definition.)_
