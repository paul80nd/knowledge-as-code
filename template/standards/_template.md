---
id: std-{{MNEM}}
tier: normative
status: draft
derived-from:
implements:
verified-by:
applies-to:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Standard: std-{{MNEM}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a standard adds to that.

**Frontmatter**

* **`id`**: `std-` plus a mnemonic for the concept the standard governs, two to seven characters and upper-case. A
  reader meets it in a control's `verifies:`, so draw it from the concept rather than from the current wording. The
  filename carries none of it: `std-SECRET` sits in `common/secret-handling.md`. **Immutable once the standard is
  active.**
* **`status`**: `draft` until agreed, then `active`, and later `deprecated` or `superseded`. Values are lowercase.
* **Where you save it**: the folder below `standards/` becomes the standard's category, and folders can
  nest. Write the rule in the most general folder where it is still true. A standard saved straight into
  `standards/` has no category, which is fine while there are few enough to read as one list.
* **`derived-from`**: the ADR id (s) this standard distils. A standard citing neither an ADR here nor a policy in
  `implements` is guidance, not a standard.
* **`implements`**: the policy clauses this standard puts into practice, as
  `implements: [ pol-EVER.BRANCH, pol-EVER.HISTORY ]`. A bare policy id is refused: a standard discharges some of a
  policy's clauses and seldom all of them, and the bare id reads to anything counting coverage as every clause covered.
  A reader takes the same list from the `Covers` lines below, so this field is written for whatever counts coverage.
* **`applies-to`**: service ids, or `all`.
* **`review-by`**: a quoted `"YYYY-MM-DD"`. Drives the staleness report.

**The identity line.** The line beneath the title carries the type, the `id`, then the `status` in upper case. It is
what a reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

Standards are living documents. Unlike an ADR, a standard is edited in place as practice matures. Record material
changes in the Changelog at the bottom rather than rewriting silently.

## Summary

The rule in a single scannable sentence: what a reader needs to take away in one breath.

## Rules

The normative content. State each rule imperatively and use [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119)
keywords (**MUST**, **MUST NOT**, **SHOULD**, **MAY**) so compliance levels are unambiguous.

Group the rules under `###` headings, one heading per thing the rules beneath it hold a reader to. The heading is what
somebody hunting one rule finds before the bullet, and it is the address a citation and an export both carry. A
standard with a single group writes one heading. `part-none` reports a Rules section with none.

Keep each rule testable. If a rule can't be checked against a concrete artefact, it's guidance, not a standard. Either
sharpen it or move it to the Rationale section.

Write every rule the subject has, and stop there. A standard binding every piece of work usually runs to seven or
eight. One covering a single interface may run to thirty.

**Under a heading, write the rules and nothing else.** An export carries the first block beneath the heading, which is
the bullet list. A paragraph after the bullets is dropped from it without a word, so put the reasoning in Rationale and
provenance where it travels.

**Close a heading with the clauses it covers**, as a footnote in italic with the label bold. CI holds the union of
those lines equal to `implements`, so the frontmatter says which obligations the standard discharges and each heading
says which rule discharges which. Write it as the last thing under the heading, and leave it off a heading that covers
no clause.

**The rules themselves carry no clause citation.** One line at the foot of the heading says what that heading
discharges. An id in brackets on every bullet buries the obligation a reader came for.

### {{What the rules below hold a reader to}}

- Services **MUST** ...
- Responses **SHOULD** ...
- Clients **MAY** ...

_**Covers:** [pol-{{MNEM}}].{{CLAUSE}}_

## Examples

A canonical good/bad pair. Examples carry more weight than prose for both human readers and AI sessions. Make them
copy-paste accurate.

```
✅ Good
{{example}}

❌ Avoid
{{example}}
{{Why this one is wrong.}}
```

## Conformance checklist

A tickable list a designer (or an AI session) can mechanically verify a new design against. This is the section that
makes the standard *usable* rather than merely readable.

- [ ] ...
- [ ] ...

## Rationale and provenance

One or two lines on *why*, then link down to the ADR (s) that decided it. Do not restate the ADR's full reasoning. The
ADR owns the "why" and this standard owns the "what". Link, don't duplicate.

- [adr-{{a}}] decided {{what this rule rests on}}

## Sources and further reading

Optional. The external documents this standard defers to. Mark an entry **normative** where a reader has not read the
rule until they have read the source, and **informative** where the source is background. A compliance posture is not a
source, and [frameworks.md](../frameworks.md) is where one goes.

- **Normative.** [{{Source}}] sets {{the baseline this standard adds exceptions to}}.
- **Informative.** [{{Other source}}] covers {{what a reader takes from it}}.

## Changelog

- {{YYYY-MM-DD}}: initial version.

[adr-{{a}}]: {{a}}.md
[{{Other source}}]: {{url}}
[{{Source}}]: {{url}}
