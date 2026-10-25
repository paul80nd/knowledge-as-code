---
id: prc-change-the-schema
type: process
tier: procedural
status: active
last-rehearsed: { at: "2026-09-20", by: claude-code/opus-5 }
rehearsal-frequency: on-change
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

* The schema guidance at `.schema/CLAUDE.md`, which states the closed key space, the field-order trap and how a rule is
  written.
* The fixture guidance at `tooling/CLAUDE.md`, which states what a schema edit does to the fixtures and the behaviour
  specs.
* `kac` run from inside a corpus as `dotnet run --project ../../tooling/kac --`.
* The .NET SDK, for the golden suite and the behaviour specs at step 6.
* Reading a specification, which step 1 needs.
* Repairing a Reqnroll feature file by hand, which step 6 needs.

## Steps

1. Establish that the change is new and matches its source. The schema may already express it. A `description:` that
   paraphrases the specification `lineage:` cites can drop a word the source states.
2. Make the change in the schema language where that language can express it. Run [prc-change-the-tool] where it
   cannot: a question the expression grammar cannot ask, or a fault one fixed message cannot name. Run
   [prc-change-a-skill] as well where the change alters what the type exports, because a lookup skill restates the
   exported sections.
3. Search every tree for the words the old value used, and make each place that restates it agree: each type page and
   `_template.md`, `_tiers.yaml`, and `docs/framework/`. Nothing compares one statement with another. Both pages are
   `seed`, so nothing compares the copies either. Count the trees from the files, because which have a type depends
   on which corpora adopted it.
4. Write each `description:` and `notes:` for the author who reads it. The generator prints a `description:` onto the
   type page of every corpus that adopted the type.
5. Account for every rule the change adds or removes. The coverage gate fails a new check id that no fixture trips.
   Hand-written pages cite rule ids, and nothing checks that a removed one is cited nowhere.
6. Bring each golden and behaviour spec the change moved up to date, once you have read why it moved. The fixtures
   validate against the real schema, so a schema change can move either.
7. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean, `generate --check` reports the generated blocks fresh, and the
golden suite and the behaviour specs pass.

Close by stating what the schema now declares, which pass would catch it being wrong, and any golden or pinned
expectation that moved.

## If it goes wrong

Revert the schema file and run `kac generate` in every corpus that adopted the type. A generated block left behind
still states the rule you removed.

A field that has shipped in an `export:` block is different. Removing it moves `export.version`, and every consumer
reading the shape before it has to be told.

## Related

* [prc-add-a-type] is the wider job a schema file is one part of. Its steps 3 and 4 set the bar for measuring a whole
  type against its sources.
* [prc-change-a-skill] changes the lookup skill that restates a type's exported sections.
* [prc-change-the-tool] adds a rule the expression grammar cannot ask.
* [std-CONFIG.COPIES] states what you must do for a type page and a
  `_template.md`, because both live in every tree that adopted the type. Step 3 is where that applies.
* [prc-pull-request] merges the change.

[prc-add-a-type]: add-a-type.md
[prc-change-a-skill]: change-a-skill.md
[prc-change-the-tool]: change-the-tool.md
[prc-pull-request]: pull-request.md
[std-CONFIG.COPIES]: ../standards/configuration.md#copies-a-value-living-in-more-than-one-tree-is-copied-and-proved
