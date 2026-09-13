---
id: prc-change-the-schema
type: process
tier: procedural
status: active
last-rehearsed: "never"
owner: human:paul.law
tags: [ schema, validation ]
---

# Change the schema

`Process: prc-change-the-schema` `ACTIVE`

Add or alter a field, a section or a rule in a type's schema file.

## When to use this

You are changing what a type declares. A schema edit on its own leaves every corpus stale. Each type page has
generated blocks derived from these files.

## Prerequisites

* The schema guidance at `.schema/CLAUDE.md`, which states the closed key space and the field-order trap.
* `kac` run from inside a corpus as `dotnet run --project ../../tooling/kac --`.
* The .NET SDK, for the golden suite at step 8 and the behaviour specs at step 9.

## Steps

1. Check the question is not already answered. A `reciprocal:`, a `mirrors-section:`, a `mirrors-citations:`, a
   `required-when:`, a scalar type or a required section may already say it.
2. Make the edit. A field using keys the schema language already has costs nothing in C#. A key the language does not
   have means an edit to `Schema.cs`. The code reading what it parsed into and `meta/type.schema.json` move in the same
   change.
   * Where the obligation is conditional, use `required-when:`. It always reports an error.
   * Where the obligation is advisory, write a rule with an `expr:`. The rule chooses its own severity.
3. Add the field to the type's `_template.md` by hand, in both trees. Nothing generates a template, and a template is
   `seed`, so nothing keeps the two copies equal. `template-fields` reads `Required` alone.
4. Load `technical-writing`, then `writing-a-record`, before you write the `description:` and `notes:`. The generator
   prints them onto the type page, so an author reads them.
5. Run `kac generate` in every corpus that adopted the type, then `kac validate` in each.
6. Write a fixture that trips a new rule, one per check id it reports. The coverage gate fails a rule nothing
   exercises.
7. Where the edit added a required field to `_universal.yaml`, write that field into every record under
   `tooling/tests/fixtures/`. Every record already there fails `required-field` without it. An optional field needs
   none of this.
8. Run the golden suite. The fixtures validate against the real schema, so this edit can move expectations already
   committed there. Regenerate with `--update` after reading the diff.
9. Repair by hand every pinned line in `tooling/kac.features/*.feature` that step 7 moved. Each one moves down by one
   line, and no flag regenerates a feature file. A finding reported against the frontmatter stays at line 1, and a
   document with no frontmatter keeps the numbers it had. Prove the repair with `dotnet test tooling/kac.features`.
10. Copy no schema file. `.schema/` is authored once at the repository root, and every corpus reads it from there. A
    type page or a `_template.md` you also touched lives in every tree.
11. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean, `generate --check` reports the generated blocks fresh, and the
golden suite and the behaviour specs pass.

Close by stating what the schema now declares, which pass would catch it being wrong, and any golden expectation or
pinned line that moved.

## Related

* [prc-add-a-type] is the wider job a schema file is one part of.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] states what you must do for a type page and a
  `_template.md`, because both live in every tree. Step 10 is where that applies.
* [prc-pull-request] merges the change.

[prc-add-a-type]: add-a-type.md
[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
