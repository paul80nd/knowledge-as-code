---
id: pol-{{MNEM}}
tier: normative
category: security
status: draft
aligns-with:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Policy: pol-{{MNEM}}` `DRAFT`

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](/knowledge-as-code/contributing.md)** — where a document goes, how it is written, and how it
is reviewed. What is below is only what a policy adds to that.

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

What we commit to and why, in one or two short paragraphs. This is the section a person reads if they read nothing else,
so write it as prose with a point of view — plain, declarative, unhedged. A policy that hedges isn't one.

Say the thing and the reason for it. The first paragraph states the position; the second earns it, ideally with the
argument someone sceptical would need rather than the one that sounds best. Avoid previewing the clauses: if a sentence
here could be re-read as an obligation, it belongs in the table instead.

_(Test before writing: would this still be true after replacing the entire technology estate? If it names a tool, a
framework or a protocol, it is a [standard](/standards), not a policy.)_

## Scope

Who and what this binds, and where the boundary falls — "all product repositories", "any system processing personal
data", "production environments only". Two or three sentences at most.

Where the boundary is arguable, say which way it falls and why, rather than leaving the reader to infer it. This is also
where ownership belongs if it needs stating: who is bound is a scope question, not a purpose one.

**The boundary line.** Where a sibling policy covers ground a reader might think is yours, close the section with a line
in italics opening `_Boundary:_`. Name each policy they would otherwise reach for and say what it owns instead. Write it
whenever two policies touch the same subject from different sides — the reader who needs it is the one who has arrived
at the wrong document and does not yet know it. Optional, and worth the two lines wherever the question is real.

## Clauses

One row per clause. Ordered **MUST**, **MUST NOT**, SHOULD, COULD — binding levels bold, non-binding plain, so the
weight drops off at the boundary without needing a divider the table format can't give you.

| Id       | Clause                       | Alignment               |
|----------|------------------------------|-------------------------|
| `{{ID}}` | **MUST** {{obligation}}      | [{{FRAMEWORK}}].{{ref}} |
| `{{ID}}` | **MUST NOT** {{prohibition}} |                         |
| `{{ID}}` | SHOULD {{recommendation}}    |                         |
| `{{ID}}` | COULD {{aspiration}}         |                         |

**Ids.** `[A-Z][A-Z0-9]{1,6}`, unique within the document, immutable once the policy is active — anything else cites
them as `pol-{{MNEM}}.{{ID}}`, CI holds every citation to a clause that exists, and removing one is a breaking change.
Prefer the shortest natural word; compress only when there isn't one. They are reconciliation keys, not prose, and every
report that shows an id shows its clause text alongside — but a CI failure or a diff shows the id alone, so it should
still be guessable.

**Shared ids.** Where two policies bind the same obligation from their own side, they take the same clause id and each
signposts the other — `— see [pol-OTHR]` closing the clause, and a boundary line above saying which side owns what. The
shared id is what makes the binding visible from either document instead of looking like an accident. The rule cuts both
ways: where two clauses mean different things, they take different ids however natural the same word felt, because one
id carrying two meanings is what turns a diff into a false reading.

**Writing a clause.** Start with the modal verb; the subject is always us, so don't restate it. One obligation per
clause — if it needs an "and" joining two different actions, it is two clauses. Keep them implementation-agnostic: a
clause says what must be true, a standard says how, and if an engineer could action it without a standard beneath it, it
has escaped downward and belongs in the standard instead. A clause no standard implements is an ordinary state and takes
no annotation: the graph reports the gap, and Notes carries the explanation where one is owed.

**The evidenceability test.** Before writing a clause, ask what an auditor would ask to *see*. If you can't answer, the
clause is unevidenceable and needs rewriting — "consider accessibility" fails, "establish accessibility requirements
during design" passes. The question itself is not published; controls carry evidence.

**Alignment.** Per clause, and only where a genuine mapping exists — an invented mapping is worse than none, and an
empty cell is honest. Reference-style links resolving into `/frameworks`, where the anchor is the framework's name with
no version and no punctuation (`iso-27001`); the label carries the version (`[ISO 27001:2022]`). Clause refs within a
framework use `.`.

## Exceptions

Where the clauses above do not hold, and who can grant a departure. Name the clauses being qualified rather than leaving
it to inference. Stays below the clauses — an exception cannot be read before the thing it excepts.

Exceptions stated up front are honest; exceptions discovered later are erosion. If there are none, say so in a sentence.
Recording a gap is acceptable; concealing it is not — and a one-off departure is a recorded deviation ([pol-DEVI]), not
an exception.

## Notes

Only what has no other home: why an expected alignment is absent, why a clause is worded unusually, a caveat a future
reader would otherwise raise. If a note is doing the job of a section, it belongs in the section.

Delete this heading if there is nothing to say. An empty section is worse than a missing one.

[pol-DEVI]: devi-deviations-are-recorded.md
[{{FRAMEWORK}}]: /frameworks.md#{{framework}}
