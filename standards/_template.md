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

<!-- DELETE FROM HERE — guidance for whoever fills this in, not part of the document ----------------------------- -->

**Frontmatter.** Delete this block once the fields above are filled in.

* **`id`** — `std-` plus a four-digit number, never reused.
* **`status`** — `draft` until agreed, then `active`, and later `deprecated` or `superseded`. Values are lowercase.
* **`axis`** — Places the standard at its true altitude — the layer where the rule is actually true.
* **`derived-from`** — The ADR id(s) this standard distils. A standard citing neither an ADR here nor a policy in
  `implements` is guidance, not a standard.
* **`applies-to`** — Service ids, or `all`.
* **`review-by`** — A quoted `"YYYY-MM-DD"`. Drives the staleness report.

**The identity line.** The line beneath the title — the type, the `id`, then the `status` in upper case. It is what a
reader arriving from a citation sees first, and CI checks all three against the frontmatter above.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

Standards are living documents — unlike ADRs they are edited in place as practice matures. Record material changes in
the Changelog at the bottom rather than rewriting silently.

## Summary

The rule in a single scannable sentence — what a reader needs to take away in one breath.

## Rules

The normative content. State each rule imperatively and use [RFC 2119](https://datatracker.ietf.org/doc/html/rfc2119)
keywords (**MUST**, **MUST NOT**, **SHOULD**, **MAY**) so compliance levels are unambiguous.

- Services **MUST** ...
- Responses **SHOULD** ...
- Clients **MAY** ...

Keep each rule testable. If a rule can't be checked against a concrete artefact, it's guidance, not a standard — either
sharpen it or move it to the Rationale section.

## Examples

A canonical good/bad pair. Examples carry more weight than prose for both human readers and AI sessions — make them
copy-paste accurate.

```
✅ Good
{{example}}

❌ Avoid
{{example}}  — why it's wrong
```

## Conformance checklist

A tickable list a designer (or an AI session) can mechanically verify a new design against. This is the section that
makes the standard *usable* rather than merely readable.

- [ ] ...
- [ ] ...

## Rationale and provenance

One or two lines on *why*, then link down to the ADR(s) that decided it. Do not restate the ADR's full reasoning — the
ADR owns the "why"; this standard owns the "what". Link, don't duplicate.

- [adr-{{a}}] — {{what it decided}}

## Changelog

- {{YYYY-MM-DD}} — Initial version.

[adr-{{a}}]: {{a}}.md

_(Link definitions, at the very foot, sorted by label. Internal references use **shortcut reference links** — write
`[adr-0007]` in the prose and define it once here, so a rename is a one-line change. The label is also the display text,
so it must be the canonical id; where you want prose link text instead, use an inline link. Filename slugs are at most
30 characters.)_
