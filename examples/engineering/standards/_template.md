---
id: std-{{nnnn}}
tier: normative
status: draft
axis:
derived-from:
implements:
verified-by:
applies-to:
review-by:
owner:
tags: [ a, b ]
---

# {{Title}}

`Standard: std-{{nnnn}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** It says where a document goes, how it is written
and how it is reviewed. What is below is only what a standard adds to that.

**Frontmatter**

* **`id`**: `std-` plus a four-digit number, never reused.
* **`status`**: `draft` until agreed, then `active`, and later `deprecated` or `superseded`. Values are lowercase.
* **`axis`**: the layer where the rule is actually true.
* **`derived-from`**: the ADR id (s) this standard distils. A standard citing neither an ADR here nor a policy in
  `implements` is guidance, not a standard.
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

- Services **MUST** ...
- Responses **SHOULD** ...
- Clients **MAY** ...

Keep each rule testable. If a rule can't be checked against a concrete artefact, it's guidance, not a standard. Either
sharpen it or move it to the Rationale section.

Where the section runs past about six rules, group them under `###` headings. A heading says what the rules beneath it
hold a reader to, so somebody hunting one rule finds the group before the bullet.

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

## Changelog

- {{YYYY-MM-DD}}: initial version.

[adr-{{a}}]: {{a}}.md
