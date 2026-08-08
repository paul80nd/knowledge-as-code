---
name: kb-review
description: Review knowledge records against the corpus authoring rules and propose rewrites in clearer, tighter language. Use when asked to review, tidy, simplify, shorten or improve the wording of one or more records, a type folder, or the corpus as a whole — for example "review the runbooks for clarity", "these policies read as verbose", "tidy up services/", "apply the authoring rules to the discoveries". Do not use for placement questions (which type a document belongs to) or for schema and validation failures.
---

# Reviewing a knowledge record

You are reviewing existing records against this corpus's authoring rules and proposing rewrites. You are not adding
knowledge, not correcting facts, and not moving documents between types.

**Read [`knowledge-as-code/authoring.md`](../../../knowledge-as-code/authoring.md) first, every time.** It is the
authority. This skill is the procedure for applying it; it does not restate the rules, and where the two appear to
disagree, `authoring.md` wins.

## The one rule that stops this going wrong

**Language follows tier, not type.** Read the record's `tier:` frontmatter before you read its prose, and apply that
tier's section of `authoring.md`. A rewrite that applies procedural terseness to an ADR has destroyed the document while
appearing to improve it.

If a record has no `tier:` field, stop and say so. Do not infer it.

## Never do these

* **Never rewrite the substance of a Decided-tier record** — any [ADR](../../../adrs.md) or
  [postmortem](../../../postmortems.md). These are immutable once accepted. You may fix a typo or a broken link.
  Everything else, including "tightening", is prohibited: to change a decision, a new one supersedes it. If a Decided
  record genuinely reads badly, report it and stop.
* **Never edit between `<!-- BEGIN GENERATED -->` and `<!-- END GENERATED -->`.** Change the frontmatter or the schema
  and regenerate.
* **Never change an identifier.** Document ids, clause ids and link reference labels are referenced from elsewhere in
  the graph and from other corpora. A renamed clause id is a broken edge.
* **Never change meaning to save words.** Losing a qualifier, an exception or a scope boundary is a defect, not a
  saving. When a sentence is long because the obligation is genuinely conditional, leave it long.
* **Never change frontmatter** other than to correct a demonstrable error, and say so explicitly if you do.
* **Never delete a section the type's `template.md` requires**, even if it is thin. An empty required section is a
  content gap to report, not a formatting problem to fix.

## Procedure

1. **Establish scope.** One record, a type folder, or a named set. If asked to review "the corpus", propose an order —
   by tier, worst offenders first — and confirm before starting. Do not silently review a hundred documents.
2. **Read `authoring.md`.** In full. Then read the type's root page (`<type>.md`) for what that type is meant to
   contain, and its `template.md` for the sections it must have.
3. **For each record**, in this order:
   a. Read the frontmatter. Note the `tier`, the `status`, and whether it is Decided. b. Read the record whole before
   changing anything. c. Identify findings against the floor, then against the tier's rules, then against
   [intent, not administration](../../../knowledge-as-code/authoring.md#intent-not-administration). d. Propose the
   rewrite as a whole document, not a diff of fragments — the result must read in one voice.
4. **Check what you produced** against the checklist below.
5. **Run `./kac validate`** and, if any frontmatter changed, `./kac index`. A rewrite that fails validation is not a
   rewrite.
6. **Report** in the shape given below. Propose; do not commit. Open a PR if asked — pushes to `main` are rejected.

## What to look for

Ordered by how much they usually cost the reader.

**Run `./kac validate` before you read anything.** The schema already catches part of this — `low-ceremony`,
`not-normative`, `no-hedged-ordering`, `hub-not-specification`, `links-rather-than-restates`,
`posture-belongs-to-frameworks` and `symptoms-first` among them. Those findings are free; do not spend judgement
rediscovering them, and do not report them as though you found them. Your value is everything below, which no expression
can see.

**Content that is not record content.** The highest-yield finding. A paragraph that would appear in more than one record
of this type is corpus guidance in the wrong place. Prose restating `owner`, `review-by`, `status` or `tags`. A review
section duplicating frontmatter. Placeholder text describing the absence of a relationship. An explanation of what a
metadata field means. Report these for removal and say where the content belongs instead.

**Duplication across records.** Where two records in the same folder carry near-identical paragraphs, flag both. One
should hold it, or neither should.

**Restated rather than cited.** Prose reproducing what an ADR, standard or glossary entry already says. Replace with a
link.

**Filler and hedging.** Sentences that announce the next sentence. *It is important to note*, *in order to*, *simply*,
*of course*. Adjectives carrying no information — *robust*, *seamless*, *comprehensive*.

**Tier violations.** Future tense in a Descriptive record. RFC 2119 keywords in a policy. Rationale inside a procedural
step. A discovery written with the confidence of an FAQ. Conditions trailing the action they guard in a runbook.

**Unmarked aspiration.** Content describing what is intended rather than what exists, with no **Planned** or
**Aspirational** marker. This is a correctness finding, not a style one — flag it prominently.

**Sentences carrying two ideas.** The most common single defect. Usually visible as a sentence over about 25 words, or
any clause joined by *and* where the two halves could each be failed independently.

## Before you hand it back

* Every identifier unchanged.
* Every link still resolves; every reference definition still used; definitions still at the foot, sorted by label.
* Required sections all present.
* Prose wrapped at 120 columns, tables exempt.
* No generated block touched.
* Nothing added that was not in the original — this is a rewrite, not an expansion.
* Read the result cold. If you cannot tell which paragraphs you changed, it is right.

## Reporting

For each record, keep it short:

**`<id>` — <what changed, in one line>.** Then the findings as a list, each naming the rule it breaches. Then the
rewritten document.

Where you are unsure whether a cut loses meaning, **do not make it** — list it separately under *Judgement calls* and
let a human decide. That list is more valuable than a larger diff.

Finish with the numbers, because they are the point: how many records, how many findings by category, and the change in
total length. If length went up, explain why — occasionally it should, when a record was terse because it was
incomplete.
