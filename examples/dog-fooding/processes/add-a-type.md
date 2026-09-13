---
id: prc-add-a-type
type: process
tier: procedural
status: active
last-rehearsed: "never"
owner: human:paul.law
tags: [ schema, taxonomy ]
---

# Add a knowledge type

`Process: prc-add-a-type` `ACTIVE`

Declare a new type in `.schema/`, write the pages a corpus needs, and update the files that count the types.

## When to use this

No existing type will take the records you have. A type with a schema and no folder counts as absent. A type with a
folder and no page fails `type-setup`. Removing a type reverses these steps.

## Prerequisites

* A candidate set of records no existing type would take.
* `kac` run from inside a corpus as `dotnet run --project ../../tooling/kac --`.

## Steps

1. Argue the type belongs. [Taxonomy](../knowledge-as-code/taxonomy.md) lists the types that exist and what each one
   excludes.
2. Pick the tier before the fields. The tier fixes how records are written and what the review bar is. Several types
   share one tier.
3. Write the type's schema file. Run [prc-change-the-schema] for the rules governing it.
4. Write the type's root page.
5. Write the type's `_template.md`.
6. Where either page mentions another type, link that type's root page and use the type's own noun as the link text.
   Never link into another type's folder. `TemplateLinkTests` rejects that.
7. Copy both pages into every other tree by hand. Both are `seed`, so nothing keeps the trees equal. `kac generate`
   writes the type's `_index.md`, so leave that one alone.
8. Record the lineage. The schema's `lineage:` and `collision:` blocks are read into
   [lineage](../knowledge-as-code/lineage.md).
9. Add the type to `types:` in `.corpus.yaml`. `kac update --add-type <folder>` writes that block. Generation and
   validation cover only the types a corpus adopted.
10. Add the folder to `.order` in `template/` and in every corpus that adopted the type. The type folders are
    alphabetical, after `knowledge-as-code` and before `frameworks`. `--add-type` leaves this file alone, and nothing
    checks what it lists.
11. Increment `version:` in `manifest.yaml`. A type changes which files a corpus receives, so the template's shape
    moved.
12. Write the new number into `upstream.template-version` in every `.corpus.yaml` under `examples/`, and in
    `tooling/tests/fixtures/new/expected-descriptor.yaml`.
13. Add the type's four files to `tooling/tests/fixtures/new/expected-tree.txt`: the schema file, the root page, the
    `_index.md` and the `_template.md`.
14. Add the type to `docs/framework/types.md`, in the table under its own tier. `DefaultTypesTests` fails a type this
    page misses, and one filed under the wrong tier.
15. Change the type count in that page's opening sentence, which spells it as a word. Reword every tracked `.md` and
    `.yaml` file using that word about something else. `No_other_file_states_that_count` fails on each of them.
16. Change the count in the transcripts in `docs/getting-started.md` and `docs/cli/new.md`. Both quote
    `validated 3 document(s) and N template(s)`, where N is the number of types. Nothing checks them.
17. Write at least one record. Most type folders here contain none, so their rules have never run. A throwaway record
    shows whether the schema says what you meant.
18. Run `kac generate` in `template/` and in every corpus that adopted the type. It writes the `_index.md` a corpus
    needs before its owner has run anything.
19. Run `kac validate` in each of those corpora.
20. Run `kac update --check --from ../../` in every corpus under `examples/`. It reports a tree that missed a copy.
21. Run the unit tests. `DefaultTypesTests` is what the documentation site fails on.
22. Run the golden suite.
23. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean. `kac update --check` reports no difference in any tree. The unit
tests pass, `DefaultTypesTests` included, and the golden suite passes.

Close by stating what the type contains and what it excludes. Name the existing type it came closest to, and what the
first record found.

## Related

* [prc-change-the-schema] covers the schema half of this.
* [prc-add-a-record] is how the first record is written.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] states what you owe a page every tree holds.
  Steps 7 and 20 are where that applies.
* [std-VERS.which-stamps-move-together-and-which-move-alone] states the stamps that move with `manifest.yaml`. Steps
  11 and 12 are where they move.
* [prc-pull-request] merges the type.

[prc-add-a-record]: add-a-record.md
[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-VERS.which-stamps-move-together-and-which-move-alone]: ../standards/versioning.md#which-stamps-move-together-and-which-move-alone
