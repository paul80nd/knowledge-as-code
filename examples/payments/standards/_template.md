---
id: std-{{MNEM}}
type: standard
tier: normative
status: draft
applies-to:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Standard: std-{{MNEM}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a standard adds to it. Add an
optional field from [the type page](../standards.md#metadata) where you have a value for it.

**Frontmatter**

* **`id`**: `std-` plus a mnemonic for the concept the standard governs, two to seven characters and upper-case. A
  reader meets it in a control's `verifies:`, so draw it from the concept rather than from the current wording. The
  filename carries none of it: `std-SECRET` sits in `common/secret-handling.md`. **Immutable once the standard is
  active.**
* **`status`**: `draft` until agreed, then `active`, and later `deprecated` or `superseded`. Values are lowercase. A
  `superseded` standard names its replacement in `superseded-by`. A citation written before the replacement still
  resolves, so the record it lands on says what to read now.
* **Where you save it**: the folder below `standards/` becomes the standard's category, and folders can
  nest. Write the rule in the most general folder where it is still true. A standard saved straight into
  `standards/` has no category, which is fine while there are few enough to read as one list.
* **`derived-from`**: the ADR id (s) this standard distils. A standard citing neither an ADR here nor a policy in
  `implements` is guidance, not a standard.
* **`implements`**: the policy clauses this standard puts into practice, as
  `implements: [ pol-EVER.BRANCH, pol-EVER.HISTORY ]`. A bare policy id is refused: a standard discharges some of a
  policy's clauses and seldom all of them, and the bare id reads to anything counting coverage as every clause covered.
  A reader takes the same list from the `Covers` lines below, so this field is written for whatever counts coverage.
* **`depends-on`**: the standards a reader has to apply to apply this one, as `depends-on: [ std-TEST ]`. A standard
  adding detail to a more general one names it here, because a folder cannot express that. A standard worth reading
  next goes in `Sources and further reading`.
* **`applies-to`**: service ids, or `all`.
* **`review-by`**: a quoted `"YYYY-MM-DD"`. Drives the staleness report.

**The closing line.** Keep the line after the Changelog in every standard. It tells a reader how to depart from a
rule. Set the link's `../` to the depth of the folder the standard sits in. Where the corpus keeps no deviations, leave
"deviation" unlinked, because the `request-deviation` skill files the request on the tracker the corpus names.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

Standards are living documents. Unlike an ADR, a standard is edited in place as practice matures. Record every
material change in the Changelog at the bottom, newest entry first, rather than rewriting silently.

## Summary

The rule in a single scannable sentence: what a reader needs to take away in one breath.

## Rules

The normative content, and the only section carrying any. Write each rule as a bullet, and give it one [BCP
14](https://www.rfc-editor.org/rfc/rfc8174) keyword: **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT** or **MAY**. A
keyword binds only in bold capitals. `part-modal` reports a bullet with none, and one that leaves a keyword in plain
text. It calls the keyword a modal, because the same declaration governs a policy's clause table.

Choose the keyword by what a departure costs:

| Keyword                    | What it states                                        | Departing from it takes         |
|----------------------------|-------------------------------------------------------|---------------------------------|
| **MUST**, **MUST NOT**     | the outcome the owner of this standard answers for    | an accepted deviation           |
| **SHOULD**, **SHOULD NOT** | the usual way to reach that outcome                   | a reason, written in the change |
| **MAY**                    | something a reader might otherwise think is ruled out | nothing                         |

Write **MUST** for the outcome and **SHOULD** for the way to reach it. A reader then keeps the latitude the outcome
allows.

`modal-outside-rules` warns where a bullet in any other section names one of those keywords, because no citation
reaches a rule written there. Where a bullet names a keyword rather than using one, as a changelog entry recording a
modal that moved does, write the keyword in backticks.

Group the rules under `###` headings, one heading per thing the rules beneath it hold a reader to. The heading is what
somebody hunting one rule finds before the bullet, and it is the address a citation and an export both carry. A
standard with a single group writes one heading. `part-none` reports a Rules section with none.

Keep each rule testable. If a rule can't be checked against a concrete artefact, it's guidance, not a standard. Either
sharpen it or move it to the Rationale section.

Write every rule the subject has, and stop there. A standard binding every piece of work usually runs to seven or
eight. One covering a single interface may run to thirty.

**Under a heading, write the rules and nothing else.** An export carries the first block beneath the heading, which is
the bullet list. A paragraph after the bullets is dropped from it without a word.

**Give a heading one short reason where the reason is not obvious.** End the bullet it explains with a clause opening
"because", so the reason travels in the export with the rule. Put the longer argument in Rationale and provenance.

**Close a heading with the clauses it covers**, as a footnote in italic with the label bold. CI holds the union of
those lines equal to `implements`, so the frontmatter says which obligations the standard discharges and each heading
says which rule discharges which. Write it as the last thing under the heading, and leave it off a heading that covers
no clause. CI reports a line naming none, and one a stray space left out of italic.

**The rules themselves carry no clause citation.** One line at the foot of the heading says what that heading
discharges. An id in brackets on every bullet buries the obligation a reader came for.

### {{What the rules below hold a reader to}}

- Services **MUST** {{the outcome}}, because {{the reason, where it is not obvious}}.
- Responses **SHOULD** {{the usual way}}.
- Clients **MAY** {{what is allowed}}.

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

Test what the rules above require, and nothing else. An item for a SHOULD is met where the work follows it or the
change says why it departs.

- [ ] ...
- [ ] ...

## Rationale and provenance

One or two lines on *why*, then link down to the ADR (s) that decided it. Do not restate the ADR's full reasoning. The
ADR owns the "why" and this standard owns the "what". Link, don't duplicate.

- [adr-{{a}}] decided {{what this rule rests on}}

## Sources and further reading

Optional. The external documents this standard defers to. Mark an entry **normative** where a reader has not read the
rule until they have read the source, and **informative** where the source is background. A standard in this corpus
goes in `depends-on` above. A compliance posture is not a source, and [frameworks.md](../frameworks.md) is where one
goes.

- **Normative.** [{{Source}}] sets {{the baseline this standard adds exceptions to}}.
- **Informative.** [{{Other source}}] covers {{what a reader takes from it}}.

## Changelog

- {{YYYY-MM-DD}}: initial version.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[adr-{{a}}]: {{a}}.md
[{{Other source}}]: {{url}}
[{{Source}}]: {{url}}
