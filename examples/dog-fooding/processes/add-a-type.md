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

Declare a new type in `.schema/`, and write the root page and the `_template.md` a corpus needs for it.

## When to use this

No existing type will take the records you have. A type with a schema and no folder counts as absent. A type with a
folder and no page fails `type-setup`.

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
7. Copy both files into every other tree by hand. Both are `seed`, so nothing keeps the trees equal.
8. Record the lineage. The schema's `lineage:` and `collision:` blocks are read into
   [lineage](../knowledge-as-code/lineage.md).
9. Add the type to `types:` in `.corpus.yaml`. Generation and validation cover only the types a corpus adopted.
10. Write at least one record. Most type folders here contain none, so their rules have never run. A throwaway record
    shows whether the schema says what you meant.
11. Run `kac generate` in every corpus that adopted the type.
12. Run `kac validate` in each of those corpora.
13. Run the golden suite.
14. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean, and the golden suite passes.

Close by stating what the type contains and what it excludes. Name the existing type it came closest to, and what the
first record found.

## Related

* [prc-change-the-schema] covers the schema half of this.
* [prc-add-a-record] is how the first record is written.
* [prc-pull-request] merges the type.

[prc-add-a-record]: add-a-record.md
[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
