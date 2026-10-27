---
id: pol-{{MNEM}}
type: policy
tier: normative
status: draft
active-from:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Policy: pol-{{MNEM}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a policy adds to it. Add an
optional field from [the type page](../policies.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `pol-` plus a four-character mnemonic for the policy's concept. Start it with the same letter as the slug,
  so the folder still reads alphabetically. Upper-case here and lower-case in the filename, so `pol-VURM` sits in
  `vurm-vulnerability-remediation.md`. **Immutable once the policy is active.** A change of meaning is a new policy and
  a retirement of this one. A reassigned mnemonic still resolves, so no check fires and every citation of it says
  something untrue.
* **Where you save it**: the folder below `policies/` becomes the policy's category, which is the broad area
  the commitment belongs to. That is a different question from the topics `tags` records. If two folders fit, pick
  the one a reader looking for this policy would try first. A policy saved straight into `policies/` has no category.
* **`aligns-with`**: every **binding** framework the clause table cites, each with the references reached inside it.
  These capture alignment rather than compliance or certification, and the wording matters if this is ever read
  externally. Leave the key bare where the policy cites nothing that binds.

  ```yaml
  aligns-with:
    - framework: ISO 27001:2022
      clauses: [ A.5.17, A.8.24 ]
    - framework: WCAG 2.2 AA
  ```

  The framework is the link label its `Alignment` cells carry, version included. A framework a clause cites whole
  carries no `clauses:`, as `WCAG 2.2 AA` does above. `alignment-rollup` reconciles this against the table in both
  directions, so a binding reference reaches one of them only by reaching the other.

  Binding means the framework register files it under **Obliged** or **Self-obligated**. A clause may cite a framework
  filed under **Inspiration**, and that citation is provenance rather than obligation, so it stays out of the roll-up.
  `framework-posture` reports a clause citing a framework the register does not place at all.
* **`active-from`**: the day the policy came into force, quoted `"YYYY-MM-DD"`. Leave the key bare until `status` is
  `active`. A policy becomes `active` on the day its owner approves it, in the reviewed pull request that sets
  `status: active` and sets `active-from` to that day. A retired policy keeps the date, and `review-not-before-active`
  fails a `review-by` earlier than it.
* **`review-by`**: a quoted date. Annual is usually right for a policy. Bring it forward whenever the estate,
  the law, or a framework this policy cites changes.

A policy names no implementers. A standard points up at the policy it puts into practice, and a downstream corpus
inherits these policies to write its own standards against. What implements this is not knowable from here.

**The title.** Write the H1 as the commitment in plain words, as `Everything is in version control`. CI checks the
identity line beneath it against the frontmatter.

**The boundary line.** Where a sibling policy covers ground a reader might think is yours, close `## Scope` with a line
in italics opening `_Boundary:_`. Name each policy the reader would otherwise turn to, and say what it owns instead. The
line is optional. Write it whenever two policies touch the same subject from different sides, because the reader who
needs it has arrived at the wrong document and does not know it yet.

**Clause ids.** A clause id matches `[A-Z][A-Z0-9]{1,6}`, is unique within the policy, and is immutable once the policy
is active. Anything else cites a clause as `pol-{{MNEM}}.{{ID}}`, in the form [Metadata][referring] sets out. CI checks
that every citation resolves to a clause that exists, so removing one is a breaking change. Under `validate --since`,
`part-id-kept` fails a change that removes or renames a clause id in an active policy. Reword a clause in place, and
write a new policy where one has to go. Prefer the shortest natural word. Compress only where no natural word is short
enough. A report shows an id beside its clause text, but a CI failure or a diff shows the id alone, so keep it
guessable.

**Shared clause ids.** Where two policies bind the same obligation from their own side, give both clauses the same id,
and make each signpost the other. The signpost closes the clause in brackets, addresses the other clause as
`[pol-OTHR].ID`, and states what the reader gains there: `([pol-OTHR].ID states the same for secrets)`, `([pol-OTHR].ID
states the agent case)`, or `([pol-OTHR].ID is wider, and owns the exceptions)`. The brackets keep the clause to one
sentence. A boundary line says which side owns what. End the link definition with `#clauses`, so the reader lands on the
table. Where two clauses mean different things, give them different ids, however natural the same word felt, because one
id with two meanings makes a diff read falsely.

**Writing a clause.** Start with the modal verb. The subject is always us, so do not restate it. Write one obligation
per clause, as one sentence with no closing full stop. A clause needing an "and" that joins two different actions is two
clauses. Write no **MAY** clause, because **MAY** grants a permission and a policy has no use for one. A COULD states an
aspiration beyond what the policy requires. Keep a clause implementation-agnostic. A clause says what must be true, and
a standard says how. If an engineer could act on a clause without a standard beneath it, it belongs in a standard. A
clause no standard implements needs no annotation. The graph reports the gap, and Notes explains it where an
explanation is owed.

**The evidenceability test.** Before writing a clause, ask what an auditor would ask to *see*. Where you cannot answer,
the clause is unevidenceable and needs rewriting: "consider accessibility" fails, "establish accessibility requirements
during design" passes. The question itself is not published. Controls record the evidence.

**The `Alignment` column.** Fill a clause's cell only where a genuine mapping exists. An invented mapping is worse than
none, and an empty cell is honest. Reference-style links resolve into `/frameworks`, where the anchor is the framework's
name with no version and no punctuation (`iso-27001`). The label includes the version (`[ISO 27001:2022]`). A clause
reference within a framework uses `.`. Roll every **binding** reference up into `aligns-with`, and `alignment-rollup`
checks it against this table. Cite a framework the register files under **Inspiration** freely. The roll-up leaves it
out.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Purpose

What we commit to and why in one or two short paragraphs. This is the section a person reads if they read nothing else,
so write it as prose with a point of view: plain, declarative, unhedged. A policy that hedges is not a policy.

Say the thing and the reason for it. The first paragraph states the position. The second earns it, ideally with the
argument someone sceptical would need rather than the one that sounds best.

_(Test before writing: would this still be true after replacing the entire technology estate? If it names a tool, a
framework or a protocol, it is a [standard](../standards.md), not a policy.)_

## Scope

Who and what this policy binds, and where the boundary falls: "all product repositories", "any system processing
personal data", "production environments only". Two or three sentences at most.

Where the boundary is arguable, say which way it falls and why. This is also where ownership belongs if it needs
stating: who is bound is a scope question, not a purpose one.

## Clauses

One row per clause, ordered **MUST**, **MUST NOT**, SHOULD, COULD. Binding levels are bold and non-binding levels plain,
so the weight drops off at the boundary without needing a divider the table format cannot give you.

| Id       | Clause                       | Alignment               |
|----------|------------------------------|-------------------------|
| `{{ID}}` | **MUST** {{obligation}}      | [{{FRAMEWORK}}].{{ref}} |
| `{{ID}}` | **MUST NOT** {{prohibition}} |                         |
| `{{ID}}` | SHOULD {{recommendation}}    |                         |
| `{{ID}}` | COULD {{aspiration}}         |                         |

## Exceptions

Where the clauses above do not hold, and who can grant a departure. Name the clauses being qualified. This section stays
below the clauses, because an exception cannot be read before the thing it excepts.

Exceptions stated up front are honest. Exceptions discovered later are erosion. If there are none, say so in a sentence.
Recording a gap is acceptable; concealing it is not. A one-off departure is a recorded deviation ([pol-DEVI])
rather than an exception.

## Notes

Only what has no other home: why an expected alignment is absent, why a clause is worded unusually, a caveat a future
reader would otherwise raise. If a note is doing the job of a section, it belongs in the section.

Delete this heading if there is nothing to say. An empty section is worse than a missing one.

[pol-DEVI]: devi-deviations.md
[referring]: https://paul80nd.github.io/knowledge-as-code/framework/metadata/#referring-to-an-id
[{{FRAMEWORK}}]: ../frameworks.md#{{framework}}
