# Schema

Machine-readable definitions of the frontmatter every knowledge type carries. These files are the **single source** for
four things:

1. **Validation** — what CI checks a document's frontmatter against.
2. **The `## Metadata` block** generated into each `<type>.md`.
3. **The `## What CI checks` block** generated into each `<type>.md`.
4. **`<type>/template.md`** — the frontmatter block and section skeleton.

Plus the index columns for `<type>/INDEX.md` and the field set for `.index.json`.

Edit the schema, regenerate, review the diff. Never hand-edit anything inside a `BEGIN GENERATED` marker.

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
| `label`                    | The singular display name — "Policy", "ADR" — used to head the generated index                                   |
| `tier` / `lifecycle`       | Fixed for the type; `tier` is written into frontmatter as a reader-facing trust signal, and CI checks it matches |
| `id`                       | Prefix, style (`numbered` or `slug`), and width                                                                  |
| `filename`                 | Pattern and slug length limit                                                                                    |
| `title`                    | H1 pattern, and `id-as-code` where the H1 opens with the ID as a code span                                       |
| `sections`                 | Required and optional H2s — drives template generation and structural validation                                 |
| `index`                    | Columns and sort order for the generated index                                                                   |
| `rules`                    | Type-level behaviours the validator applies (immutability, reciprocity)                                          |

## Open questions

* **`standards.yaml` `axis` values are unresolved** — four different formulations exist across the corpus. The schema
  currently carries the `standards.md` version with a `TODO` note. Settle it before generating.
* **Numbered vs slug ids** are assigned per type below. Numbered where documents accrete in sequence and the number is
  useful in navigation; slug where the thing has a natural stable name. Worth a review pass — the split is a convention,
  not a derivation.
