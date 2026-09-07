---
id: prc-add-a-type
type: process
tier: procedural
status: active
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
owner: paul.law
tags: [ schema, taxonomy ]
---

# Add a knowledge type

`Process: prc-add-a-type` `ACTIVE`

Declare a new type in `.schema/`, and stand up the three files a corpus needs beside it.

## When to use this

An existing type will not hold the records you have. A type with a schema and no folder counts as absent, and one with
a folder and no page fails `type-setup`.

## Prerequisites

* A candidate set of records no existing type would take.
* `kac` run from inside a corpus as `dotnet run --project ../../tooling/kac --`.

## Steps

1. Argue the type belongs. [Taxonomy](../knowledge-as-code/taxonomy.md) carries the types that exist and what each is
   not.
2. Pick the tier before the fields. The tier fixes how records are written and what the review bar is, and several
   types share one.
3. Write the type's schema file. Run [prc-change-the-schema] for the rules governing it.
4. Write the root page and the `_template.md` beside it. Both are `seed`, so nothing holds the two trees equal and the
   copy across is yours. Where either names another type, link that type's page and give the link the type's own noun,
   because `kac new` drops the link for a corpus that declined that type and the noun is what carries the sentence
   afterwards. Never link into another type's folder, which `TemplateLinkTests` refuses.
5. Record the lineage. The schema's `lineage:` and `collision:` blocks are read into
   [lineage](../knowledge-as-code/lineage.md).
6. Add the type to `types:` in `.corpus.yaml`. Generation and validation cover the types a corpus adopted and no
   others.
7. Write at least one record. Most type folders here hold none, so their rules have never run, and a throwaway record
   is how you find out whether the schema says what you meant.
8. Run `kac generate` and `kac validate` in every corpus that adopted it, then the golden suite.
9. Run [prc-pull-request].

## Verification

Every corpus that adopted the type validates clean, and the golden suite passes.

Close by naming what the type holds, what it is not, which existing type it was nearly, and what the first record
found.

## Related

* [prc-change-the-schema] carries the schema half of this.
* [prc-add-a-record] is how the first record is written.
* [prc-pull-request] is how the type lands.

[prc-add-a-record]: add-a-record.md
[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
