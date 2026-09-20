---
id: nfr-{{nnnn}}
type: nfr
tier: normative
status: draft
characteristic:
applies-to:
target:
window:
measured-by:
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

* **`status`**: `draft` while the figure is still being settled. `aspirational` where it is settled and nobody is held
  to it yet. `agreed` where the estate is held to it. `retired` where it no longer applies.
* **`characteristic`**: the quality this target commits to, from the values [the type page](../nfrs.md#metadata) lists.
  One record states one quality. A service needing both a latency target and a recovery target gets two records.
* **`applies-to`**: service or offering ids. Estate-wide targets are almost always wrong, since a marketing page and
  the checkout flow don't deserve the same budget.
* **`target`**: a concrete figure, such as `99.5%`, `p95 under 400ms` or `RTO 4h`.
* **`window`**: the period the figure is read over, such as `monthly` or `rolling 4 weeks`. Required for an
  availability, capacity, latency or throughput target, because a rate without a period means nothing.
* **`measured-by`**: required. An NFR you cannot measure is a wish, and "we'd notice" is not a measurement method.
* **`constrained-by`**: integration ids whose own SLA caps this.
* **`agreed-on`**: the day the owner accepted the figure. Required once `status` is `agreed`.

**Fields this template leaves out.** This type takes optional fields the frontmatter above does not carry. Add a key
where you have a value for it, and leave it out where you do not. [The type page](../nfrs.md#metadata) lists every field
and says what each one holds.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

One sentence stating the target.

## Target

The commitment, stated precisely enough to be argued about. State the period it is read over, and say what the number
is taken from end to end.

## Why this number

Why this figure, and not a looser or a tighter one. Name what fixes it: what a customer will tolerate, what a
dependency already limits you to, or a measurement you have taken. Say what the target deliberately leaves out.

## How it is measured

The instrument, where the reading can be seen, and who looks at it. If no measurement exists today, say so plainly and
either build one or state the target you *can* observe instead.

## Current actual

What we are achieving now, and as of when. The gap between this and the target says whether the estate meets the
number or is aiming at it. Where it is aiming, say under `What we do about a breach` that nobody is paged.

## What a breach costs

What a miss costs: degraded service, contractual exposure, a customer conversation, nothing much. An NFR with no
consequence is documentation theatre, and "nothing much" is a legitimate and clarifying answer.

## What we do about a breach

The response once the target is missed. Name the alert that fires, who it pages, and the repair that follows. Where
the cost above is nothing much, say that nobody is paged and nothing is stopped.

## Constraints

External dependencies that cap this target:

* **[itg-{{a}}]** has an SLA of {{x}}, so anything built on it cannot promise more.

_(If a target exceeds what a dependency promises, it is a hope rather than a commitment. Record that here rather than
discovering it during an incident.)_

## Related

* [ofr-{{a}}] is the offering this constrains.
* [pmt-{{a}}] records an incident measured against this target.

[ofr-{{a}}]: ../offerings/{{a}}.md
[itg-{{a}}]: ../integrations/{{a}}.md
[pmt-{{a}}]: ../postmortems/{{a}}.md
