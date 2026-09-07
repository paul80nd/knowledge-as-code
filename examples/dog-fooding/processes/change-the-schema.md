---
id: prc-change-the-schema
tier: procedural
status: active
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
owner: paul.law
tags: [ schema, validation ]
---

# Change the schema

`Process: prc-change-the-schema` `ACTIVE`

Add or alter a field, a section or a rule in a type's schema file.

## When to use this

You are changing what a type declares. A schema edit on its own leaves every corpus stale, because each type page
carries generated blocks derived from these files.

## Prerequisites

* The schema guidance at `.schema/CLAUDE.md`, which carries the closed key space and the field-order trap.
* `kac` run from inside a corpus as `dotnet run --project ../../tooling/kac --`.
* The .NET SDK, for the golden suite at step 8.

## Steps

1. Check the question is not already answered. A `reciprocal:`, a `mirrors-section:`, a `mirrors-citations:`, a
   `required-when:`, a scalar type or a required section may already say it.
2. Make the edit. A field using keys the schema language already has costs nothing in C#. A key the language does not
   have means an edit to `Schema.cs`, to the code reading what it parsed into, and to `meta/type.schema.json` in the
   same change.
   * Where the obligation is conditional, `required-when:` is the existing key and it always reports an error.
   * Where it is a should-have-done-this, write a rule with an `expr:`, which chooses its own severity.
3. Add the field to the type's `_template.md` by hand, in both trees. Nothing generates a template and a template is
   `seed`, so nothing holds the two copies equal. `template-fields` reads `Required` alone.
4. Write the `description:` and `notes:` to `technical-writing`, then `writing-a-record`. The generator prints them onto
   the type page, so an author reads them.
5. Run `kac generate` in every corpus that adopted the type, then `kac validate` in each.
6. Write a fixture that trips a new rule, one per check id it reports. The coverage gate fails a rule nothing
   exercises.
7. Run the golden suite. The fixtures validate against the real schema, so this edit can move expectations already
   committed there. Regenerate with `--update` after reading the diff.
8. Copy nothing. `.schema/` is authored once at the repository root and read from there by every corpus. A type page or
   a `_template.md` you also touched does live in every tree.
9. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean, `generate --check` reports the generated blocks fresh, and the
golden suite passes.

Close by naming what the schema now declares, which pass would catch it being wrong, and any golden expectation that
moved.

## Related

* [prc-add-a-type] is the wider job a schema file is one part of.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] says what a type page and a `_template.md` owe
  you, because both live in every tree.
* [prc-pull-request] is how the change lands.

[prc-add-a-type]: add-a-type.md
[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
