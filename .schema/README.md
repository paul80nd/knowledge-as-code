# Schema

Machine-readable definitions of the frontmatter every knowledge type carries. These files are the **single source** for
three things:

1. **Validation** — what CI checks a document's frontmatter against.
2. **The `## Metadata` block** generated into each `<type>.md`.
3. **The `## What CI checks` block** generated into each `<type>.md`.

Plus the columns and sort order for `<type>/INDEX.md`.

Edit the schema, regenerate, review the diff. Never hand-edit anything inside a `BEGIN GENERATED` marker.

**`<type>/template.md` is not generated.** A template is written and kept in step by hand, and it is also excluded from
validation, so nothing ties one to its schema in either direction. Changing a type's fields or required sections means
opening its template and making the same change there — assume that, rather than that a regeneration will catch it.

## Files

| File              | Contents                                      |
|-------------------|-----------------------------------------------|
| `_universal.yaml` | Fields every document in the taxonomy carries |
| `_enums.yaml`     | Enums shared by more than one type            |
| `<folder>.yaml`   | One per knowledge type, named for its folder  |

Type files are named for the **folder**, not the type — `adrs.yaml`, `services.yaml`, `data.yaml`. CI infers a
document's type from its folder, so folder → schema is an identity lookup with no singularisation step.

## Field specification

```yaml
fields:
  <name>:
    required: true|false        # default false
    required-when: '<other> == <value>' | '<other> != <value>' | '<other> in [a, b]'
    type: string|date|enum|id|list|bool|int
    of: id|string               # element type, when type is list
    values: [ ... ]             # when type is enum, or an $enums.<name> reference
    ref: <folder>               # when type is id or list-of-id: which type the id must belong to
    reciprocal: <field>         # the field on the target that must point back
    pattern: '<regex>'          # additional constraint
    default: <value>
    description: >              # one line, rendered into the generated Metadata table
    notes: >                    # the longer why; schema-only, and the fallback when there is no description
```

`required-when` takes those three forms and no others — a condition the loader cannot read stops the load rather than
reading as one that never holds. It tests one other field of the same document; a condition needing more than that is a
rule with an `expr:`, not a field declaration. Where the field it names is absent the condition does not hold, `!=`
included: `required-field` is already reporting that absence, and requiring a second field on top would report one
omission as two.

`description` and `notes` answer different questions. `description` is what a reader of the type page needs at a glance
and is what the Metadata table renders; `notes` is the reasoning, which belongs here in the schema where there is room
for it. A field declaring only `notes` still renders them, so the two can be adopted a schema at a time — but where a
note has grown past a line, that is the signal it wants a `description` beside it rather than a trim.

**Keep a `description` under ~100 characters.** The generated table pads every column to its widest cell, so one long
description widens every row on the page — a 153-character cell once made all ten ADR rows 190 wide. Enum `values` are
not part of that budget: they render in a small table of their own beneath it rather than inside the cell, so declaring
a sixth value costs nothing in the width of the main table.

**Conventions the validator enforces globally**

* Dates are quoted strings in `YYYY-MM-DD` form.
* An absent value is a **bare key** (`decided-on:`) — never `null`, `~`, `""`, `—` or `TBD`.
* Enum values are lowercase and hyphenated.
* Unknown keys fail, except the Azure DevOps reserved keys listed in `_universal.yaml` under `reserved`.

## Type specification

Beyond `fields`, each type file declares:

| Key                        | Purpose                                                                                                          |
|----------------------------|------------------------------------------------------------------------------------------------------------------|
| `type` / `folder` / `page` | Identity, and where the type lives                                                                               |
| `shape`                    | `collection` (the default) or `single-document` — see the note below                                             |
| `label`                    | The singular display name — "Policy", "ADR" — used to head the generated index                                   |
| `tier` / `lifecycle`       | Fixed for the type; `tier` is written into frontmatter as a reader-facing trust signal, and CI checks it matches |
| `id`                       | Prefix, style and width — see the note below on which styles the validator acts on                               |
| `filename`                 | Pattern and slug length limit                                                                                    |
| `sections`                 | Required and optional H2s — the required ones are checked for presence                                           |
| `clauses`                  | The clause table's section, id pattern and modals, where a type states its obligations as addressable rows       |
| `index`                    | Columns and sort order for the generated index                                                                   |
| `rules`                    | Type-level behaviours — see the note below on which of them run                                                  |

**`shape`.** Most types are a **collection** — a folder of records, a page describing them, and a template to copy. The
glossary is a **single-document** type: one document read end to end, whose page *is* the record. A collection declares
its `folder:`; a single-document type declares none, because it has none, and nothing indexes it.

It is declared rather than inferred. An absent `folder:` and a deliberate `folder: null` are the same string once
parsed, so a shape read off the folder cannot tell a single-document type from a collection whose folder key was lost.
It defaults to `collection`, so only the type that is not one has to say so.

**`id.style`.** Four styles appear across the type files: `numbered`, `slug`, `mnemonic` and `literal`. The id checks
act on two — `numbered` and `mnemonic` — and a type declaring either of the others receives the prefix check alone.
Link-label canonicalisation covers `slug` as well, so the shortfall is in the id checks rather than in the idea.

**`rules`.** A rule declaring an `expr:` runs. It is evaluated against every document of its type, reports under its
own id, is listed by `kac checks`, and renders its own row into the generated `## What CI checks` block from its
`description:` — so adding one is adding YAML rather than editing the tool. It must also declare a `severity:` and a
`message:`, and an expression the tool cannot compile stops the load rather than passing silently.
[`../.tooling/SPEC.md`](../.tooling/SPEC.md) holds the grammar and the facts an expression may ask for.

Two ids keep a hand-written arm instead — `y-statement-present` and `alternatives-have-verdicts`, both on the
decision-record type — because what they ask needs more than the grammar can say. Every remaining id is a statement of
intent: a behaviour someone wants, written down, that no code answers to yet. Those do not appear in the checks table,
so a reader of a type page sees what is enforced rather than what is hoped for.

Reciprocity and section mirroring are declared on the **field**, not here: `reciprocal:` and `mirrors-section:` drive
them. So does a conditional requirement, through `required-when:`. A `rules:` entry restating any of those has no
effect, and there are none left — an entry that duplicates a declaration reads as a second, weaker source for the same
obligation.

## Open questions

* **A schema can still declare something the tool does nothing with.** A `ref:` naming a folder no schema covers, a
  `values:` list on a `type: list` field, an `id.style` with no branch in the id checks — each is accepted at load and
  silently ignored thereafter, and then reads as a commitment to anyone who takes a copy of these files. An `expr:` and
  a `required-when:` no longer can: both fail at load naming what they could not read. The remainder want the same
  treatment, and a rule that is deliberately aspirational wants a marker saying so rather than silence.
* **`_enums.yaml` `used-by:` is unparsed.** It lists the types an enum serves and nothing reconciles it against the
  loaded schemas, so in a corpus that has adopted only some of those types it is simply wrong. Either check it or say in
  the file that it is a comment.
* **`standards.yaml` `axis` values are unresolved** — four different formulations exist across the corpus. The schema
  currently carries the `standards.md` version with a `TODO` note. Settle it before generating.
* **ID styles** are assigned per type. Numbered where documents accrete in sequence and the number is useful in
  navigation; slug where the thing has a natural stable name; mnemonic where a small, heavily-cited set benefits from an
  id that says something. Worth a review pass — the split is a convention, not a derivation.
