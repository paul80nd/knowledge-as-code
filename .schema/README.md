# Schema

Machine-readable definitions of the frontmatter every knowledge type carries. These files are the **single source** for
three things:

1. **Validation**: what CI checks a document's frontmatter against.
2. **The `## Metadata` block** generated into each `<type>.md`.
3. **The `## What CI checks` block** generated into each `<type>.md`.

Plus the columns and sort order for `<type>/_index.md`.

Edit the schema, regenerate, review the diff. Never hand-edit anything inside a `BEGIN GENERATED` marker.

**`<type>/_template.md` is not generated.** A template is written and kept in step by hand. It is also excluded from
validation, so nothing ties one to its schema in either direction. Changing a type's fields or required sections means
opening its template and making the same change there. Assume that, rather than that a regeneration will catch it.

## Files

| File                    | Contents                                      |
|-------------------------|-----------------------------------------------|
| `_universal.yaml`       | Fields every document in the taxonomy carries |
| `_enums.yaml`           | Enums shared by more than one type            |
| `_tiers.yaml`           | What each tier is called, and how it behaves  |
| `_checks.yaml`          | Every check the validator can report          |
| `<folder>.yaml`         | One per knowledge type, named for its folder  |
| `meta/type.schema.json` | The shape of a `<folder>.yaml`. See below     |

Type files are named for the **folder**, not the type: `adrs.yaml`, `services.yaml`, `data.yaml`. CI infers a document's
type from its folder, so folder to schema is an identity lookup with no singularisation step.

A tier is declared twice, deliberately. `_universal.yaml` gives the `tier` field its range, and every record is
validated against it. `_tiers.yaml` says what each of those values is called and how a document of it behaves, and
carries the thing worth saying before the types beneath it are listed. Neither is derivable from the other, so the two
are reconciled when the schema loads. A value one knows and the other does not is a record that can carry a tier no page
can name, or a heading no document will ever sit under. Order is load-bearing in `_tiers.yaml`. Every generated list of
types is grouped in the order it sets.

None of them carries a version stamp. Answering "which version of the schema is this corpus on" takes something that
reconciles the answer against an upstream. A number nothing compares is a number a corpus can be wrong about silently,
so the stamp and its reader arrive together or not at all. Tracked in
[knowledge-as-code#16](https://github.com/paul80nd/knowledge-as-code/issues/16).

## The keys a type file may carry

**[`meta/type.schema.json`](meta/type.schema.json) is the reference for them**: every key a type file may carry, what
its value may be, what each is for, and the edge to weigh before reaching for it. Each type file opens with a modeline
pointing at it. An editor with YAML language-server support then offers the keys, describes each one on hover, and marks
a wrong one as it is typed:

```yaml
# yaml-language-server: $schema=./meta/type.schema.json
```

**No build reads it.** It is an editor's view of a contract `kac` enforces. A schema file written outside an editor
meets the same gate as one written in it, and neither is admitted by the JSON alone.

It answers shape and vocabulary. What it cannot answer is anything spanning two files or reading the code, and those two
questions are documented on the site:

* <https://paul80nd.github.io/knowledge-as-code/design/expressions/> is what a rule's `expr:` may say.
* <https://paul80nd.github.io/knowledge-as-code/design/held-to/> is what `kac` refuses when it loads these files, and
  why a declaration the tool ignores counts as a defect.

## Open question

**ID styles** are assigned per type. Numbered where documents accrete in sequence and the number is useful in
navigation. Slug where the thing has a natural stable name. Mnemonic where a small, heavily-cited set benefits from an
id that says something. The split is a convention that nothing derives, and it is worth a review pass.
