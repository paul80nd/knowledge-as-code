---
id: nfr-{{nnnn}}
tier: normative
status: draft
applies-to:
target:
measured-by:
constrained-by:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`NFR: nfr-{{nnnn}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what an NFR adds to that.

**Frontmatter**

* **`status`**: `draft` until someone has agreed it, then `agreed`. An agreed target is a commitment, not an
  aspiration.
* **`applies-to`**: service or capability ids. Estate-wide targets are almost always wrong, since a marketing page and
  the checkout flow don't deserve the same budget.
* **`target`**: a concrete figure, such as `99.5% monthly`, `p95 < 400ms` or `RTO 4h`.
* **`measured-by`**: required. An NFR you cannot measure is a wish, and "we'd notice" is not a measurement method.
* **`constrained-by`**: integration ids whose own SLA caps this.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence stating the target.

## Target

The commitment, stated precisely enough to be argued about. Include the measurement window: a percentage without a
period means nothing.

## How it is measured

The instrument, where the reading can be seen, and who looks at it. If no measurement exists today, say so plainly and
either build one or state the target you *can* observe instead.

## Current actual

What we are achieving now, and as of when. The gap between this and the target is usually the most useful line in the
document.

## If it is breached

What actually happens: degraded service, contractual exposure, a customer conversation, nothing much. An NFR with no
consequence is documentation theatre, and saying "nothing much" is a legitimate and clarifying answer.

## Constraints

External dependencies that cap this target:

* **[int-{{a}}]** has an SLA of {{x}}, so anything built on it cannot promise more.

_(If a target exceeds what a dependency promises, it is a hope rather than a commitment. Record that here rather than
discovering it during an incident.)_

## Related

* [cap-{{a}}] is the capability this constrains.
* [pmt-{{a}}] records an incident measured against this target.

[cap-{{a}}]: ../capabilities/{{a}}.md
[int-{{a}}]: ../integrations/{{a}}.md
[pmt-{{a}}]: ../postmortems/{{a}}.md
