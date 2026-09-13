# Schema

Machine-readable definitions of the frontmatter every knowledge type declares. Nothing about a type is hard-coded in
`kac`, so these files are the single source for all of it:

1. **Validation**: what CI checks a document's frontmatter against.
2. **The `## Metadata` block** generated into each `<type>.md`.
3. **The `## What CI checks` block** generated into each `<type>.md`.
4. **The columns and sort order** for `<type>/_index.md`.
5. **The parts** a citation such as `pol-VURM.TIMEBOX` points at.
6. **The export**: which types travel to another corpus, and how much of each.

Edit the schema, regenerate, then review the diff. Never edit anything inside a `BEGIN GENERATED` marker.

**`<type>/_template.md` is not generated.** You write a template and keep it in step by hand. Changing a type's fields
or required sections means opening its template and making the same change there. `template-fields` catches a
**required** field the template omits, and a key the type no longer declares. It does not catch an **optional** field
you added, because leaving one out of a template is an editorial choice.

## Files

| File                    | Contents                                     |
|-------------------------|----------------------------------------------|
| `_universal.yaml`       | Fields every document in the taxonomy has    |
| `_enums.yaml`           | Enums shared by more than one type           |
| `_shapes.yaml`          | Object shapes shared by more than one type   |
| `_tiers.yaml`           | What each tier is called, and how it behaves |
| `_checks.yaml`          | Every check the validator can report         |
| `<folder>.yaml`         | One per knowledge type, named for its folder |
| `meta/type.schema.json` | The shape of a `<folder>.yaml`. See below    |

A type file is named for the **folder**, not the type: `adrs.yaml`, `services.yaml`, `data.yaml`. CI reads a document's
type from its folder, so folder to schema is an identity lookup with no singularisation step.

**A tier is declared twice, deliberately.** `_universal.yaml` gives the `tier` field its range, and validates every
record against it. `_tiers.yaml` says what each value is called and how a document of that tier behaves. Neither is
derivable from the other, so `kac` reconciles the two when it loads the schema. A value one file knows and the other
does not would let a record take a tier no page describes, or leave a heading no document sits under.

**Order is load-bearing in `_tiers.yaml`.** Every generated list of types is grouped in the order it sets.

**No file here has a version stamp.** Answering "which version of the schema is this corpus on" needs something that
compares the answer against an upstream, so the stamp and its reader arrive together or not at all. Tracked in
[knowledge-as-code#16](https://github.com/paul80nd/knowledge-as-code/issues/16).

## The keys a type file may take

**[`meta/type.schema.json`](meta/type.schema.json) is the reference for them**: every key a type file may take, what its
value may be, what each is for, and the edge to weigh before reaching for it. Each type file opens with a modeline
pointing at it. An editor with YAML language-server support then offers the keys, describes each one on hover, and marks
a wrong one as you type:

```yaml
# yaml-language-server: $schema=./meta/type.schema.json
```

**No build reads it.** It is an editor's view of a contract `kac` enforces. A schema file written outside an editor
meets the same gate as one written in it, and passing the JSON alone admits neither.

It answers shape and vocabulary. It cannot answer anything that spans two files or reads the code, and the site
documents those:

* <https://paul80nd.github.io/knowledge-as-code/design/expressions/> is what a rule's `expr:` may say.
* <https://paul80nd.github.io/knowledge-as-code/design/held-to/> is what `kac` refuses when it loads these files, and
  why a declaration the tool ignores counts as a defect.
* <https://paul80nd.github.io/knowledge-as-code/design/shaping-a-type/> is why each type the framework ships chose the
  fields, sections and export it did.

## Open question

**ID styles are assigned per type.** Numbered where documents accrete in sequence and the number helps you navigate.
Slug where the thing has a stable natural name. Mnemonic where a small, heavily cited set benefits from an id that says
something. Nothing derives the split, so it is worth a review pass.
