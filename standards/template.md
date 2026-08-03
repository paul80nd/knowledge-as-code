# Standard: <Title>

**Status:** Draft | Active | Deprecated | Superseded by [<standard>](<slug>.md)
**Since:** YYYY-MM-DD &nbsp; **Last updated:** YYYY-MM-DD
**Applies to:** <scope — e.g. "All public HTTP APIs">
**Derived from:** [ADR-NNNN]

_(Keep one status. Standards are living documents — unlike ADRs they are edited in place as practice matures. Record
material changes in the Changelog at the bottom rather than rewriting silently.)_

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
<example>

❌ Avoid
<example>  — why it's wrong
```

## Conformance checklist

A tickable list a designer (or an AI session) can mechanically verify a new design against. This is the section that
makes the standard *usable* rather than merely readable.

- [ ] ...
- [ ] ...

## Rationale & provenance

One or two lines on *why*, then link down to the ADR(s) that decided it. Do not restate the ADR's full reasoning — the
ADR owns the "why"; this standard owns the "what". Link, don't duplicate.

- [ADR-NNNN] — <what it decided>

## Changelog

- YYYY-MM-DD — Initial version.

[ADR-NNNN]: NNNN-kebab-slug.md
[ADR-NNNN]: NNNN-kebab-slug.md

_(Link definitions, at the very foot, sorted by label. Internal references use **shortcut reference links** — write
`[ADR-0007]` in the prose and define it once here, so a rename is a one-line change. The label is also the display text,
so it must be the canonical id; where you want prose link text instead, use an inline link. Filename slugs are at most
30 characters.)_
