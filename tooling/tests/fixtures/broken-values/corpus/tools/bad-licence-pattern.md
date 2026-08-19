---
id: tol-bad-licence-pattern
tier: descriptive
status: approved
category: testing
licence: GPL/2.0 †
owner: alex.doe
---

# Bad licence pattern

`Tool: tol-bad-licence-pattern` `APPROVED`

## What we use it for

Covering `field-pattern` on a scalar field. `licence` is `type: string` with
`pattern: '^[A-Za-z0-9.\-+ ()]+$'`, and a scalar has no sequence for `CheckList` to walk — so this exercises the other
half of the check, where the pattern is applied to the field's own value.

## Status

Intentionally broken. The slash and the dagger are both outside the declared character class, so the value fails the
pattern once for the whole field.
