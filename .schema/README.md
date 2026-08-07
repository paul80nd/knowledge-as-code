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
| `id`                       | Prefix, style and width — see the note below on which styles the validator acts on                               |
| `filename`                 | Pattern and slug length limit                                                                                    |
| `sections`                 | Required and optional H2s — the required ones are checked for presence                                           |
| `clauses`                  | The clause table's section, id pattern and modals, where a type states its obligations as addressable rows       |
| `index`                    | Columns and sort order for the generated index                                                                   |
| `rules`                    | Type-level behaviours — see the note below on which of them run                                                  |

**`id.style`.** Five styles appear across the type files: `numbered`, `slug`, `mnemonic`, `literal` and
`single-document`. The validator's id checks act on two — `numbered` and `mnemonic` — and a type declaring any other
receives the prefix check alone. Link-label canonicalisation covers `slug` as well, so the shortfall is in the id checks
rather than in the idea.

**`rules`.** These are declarations, not a dispatch table. Two rule ids are implemented — `y-statement-present` and
`alternatives-have-verdicts`, both on the decision-record type, and both only where the rule declares
`severity: warning`. Every other id across the type files names a behaviour that does not run, and a rule the tool does
not implement is also absent from the generated `## What CI checks` block, so nothing on the page marks the gap.

Two checks that read as rules are not driven from here at all: reciprocity comes from a field's `reciprocal:`, and
section mirroring from its `mirrors-section:`. A `rules:` entry naming either has no effect.

Treat an entry here as a statement of intent, and read the validator before relying on one.

## Open questions

* **A schema can declare something the tool does nothing with, and nothing objects.** An unimplemented rule id, a
  `ref:` naming a folder no schema covers, a `values:` list on a `type: list` field, an `id.style` with no branch in the
  id checks — each is accepted at load and silently ignored thereafter. The declaration then reads as a commitment to
  anyone who takes a copy of these files. The fix is to fail at load on anything undispatchable, and to give genuinely
  aspirational entries a marker the checks table can render as *not yet enforced*; the aspiration is worth keeping, the
  silence is not.
* **`_enums.yaml` `used-by:` is unparsed.** It lists the types an enum serves and nothing reconciles it against the
  loaded schemas, so in a corpus that has adopted only some of those types it is simply wrong. Either check it or say in
  the file that it is a comment.
* **`standards.yaml` `axis` values are unresolved** — four different formulations exist across the corpus. The schema
  currently carries the `standards.md` version with a `TODO` note. Settle it before generating.
* **ID styles** are assigned per type. Numbered where documents accrete in sequence and the number is useful in
  navigation; slug where the thing has a natural stable name; mnemonic where a small, heavily-cited set benefits from an
  id that says something. Worth a review pass — the split is a convention, not a derivation.
