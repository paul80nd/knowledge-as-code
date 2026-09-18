---
id: prc-add-a-type
type: process
tier: procedural
status: active
last-rehearsed: "never"
rehearsal-frequency: on-change
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
2. Pick the tier before the fields. It fixes how a record of this type is written, and several types may share one.
3. Compare the shape you have in mind against the prior art you mean to declare, and against at least two other
   current sources for the same kind of document. Prefer a primary source to a summary of one.
4. Close a gap two primary sources ask for. File a gap resting on one source, or on a secondary reading of a paywalled
   one, as its own issue naming the source and what the gap costs.
5. Write the type's schema file. Run [prc-change-the-schema] for the rules governing it.
6. Record the lineage in that file. `prior-art` names the source, `alignment` and `divergence` state what step 3
   found, and `collision:` names a word this type uses differently from the practice it borrows from. All are read
   into [lineage](../knowledge-as-code/lineage.md).
7. Write the type's root page.
8. Write the type's `_template.md`.
9. Link another type by its root page, with that type's own noun as the link text. `TemplateLinkTests` rejects a link
   into another type's folder.
10. Copy both pages into every other tree by hand. Both are `seed`, so nothing keeps the trees equal. `kac generate`
    writes the type's `_index.md`, so leave that one alone.
11. Add the type to `types:` in `.corpus.yaml`, which `kac update --add-type <folder>` writes. Nothing generates or
    validates a type a corpus has not adopted.
12. Add the folder to `.order` in `template/` and in every corpus that adopted the type. The type folders are
    alphabetical, after `knowledge-as-code` and before `frameworks`. `--add-type` leaves this file alone, and nothing
    checks what it lists.
13. Increment `version:` in `manifest.yaml`.
14. Write the new number into `upstream.template-version` in every `.corpus.yaml` under `examples/`, and in
    `tooling/tests/fixtures/new/expected-descriptor.yaml`.
15. Add the type's four files to `tooling/tests/fixtures/new/expected-tree.txt`: the schema file, the root page, the
    `_index.md` and the `_template.md`.
16. Add the type to `docs/framework/types.md`, in the table under its own tier. `DefaultTypesTests` fails a type this
    page misses, and one filed under the wrong tier.
17. Change the type count in that page's opening sentence, which spells it as a word. Reword every tracked `.md` and
    `.yaml` file using that word about something else. `No_other_file_states_that_count` fails on each of them.
18. Change the count in the transcripts in `docs/getting-started.md` and `docs/cli/new.md`. Both quote
    `validated 3 document(s) and N template(s)`, where N is the number of types. Nothing checks them.
19. Write at least one record, even a throwaway. Most type folders here hold none, so their rules have never run
    against anything.
20. Run `kac generate` in `template/` and in every corpus that adopted the type. It writes the `_index.md` a corpus
    needs before its owner has run anything.
21. Run `kac validate` in each of those corpora.
22. Run `kac update --check --from ../../` in every corpus under `examples/`. It reports a tree that missed a copy.
23. Run the unit tests. `DefaultTypesTests` is what the documentation site fails on.
24. Run the golden suite.
25. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean. `kac update --check` reports no difference in any tree. The unit
tests pass, `DefaultTypesTests` included, and the golden suite passes.

Close by stating what the type contains and what it excludes. Name the existing type it came closest to, what the
first record found, and which gaps the comparison left to an issue.

## If it goes wrong

A type nobody adopted costs nothing. Remove the folder from `types:` in `.corpus.yaml`, and the corpus stops
generating and validating it.

A type a corpus has already adopted comes out with every record of it, and with every citation pointing at one, in
the same change.

## Related

* [prc-change-the-schema] covers the schema half of this.
* [prc-add-a-record] is how the first record is written.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] states what you owe a page every tree holds.
  Steps 10 and 22 are where that applies.
* [std-VERS.which-stamps-move-together-and-which-move-alone] states the stamps that move with `manifest.yaml`. Steps
  13 and 14 are where they move.

[prc-add-a-record]: add-a-record.md
[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-VERS.which-stamps-move-together-and-which-move-alone]: ../standards/versioning.md#which-stamps-move-together-and-which-move-alone
