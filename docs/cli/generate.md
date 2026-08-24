# `generate` rewrite the parts of a corpus derived from its records

<!-- BEGIN GENERATED: usage-generate -->

```text
kac generate [--check] [--no-color]
```

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--check`    | Fail if a generated file is stale instead of writing it.    |
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-generate -->

## What it is for

`generate` regenerates the content of a corpus that is derived from frontmatter and the schema, so that nobody maintains
it by hand. Its readers are the people who read the corpus: a type's index page, the frontmatter reference and checks
table on a type page, and the taxonomy's own tables are all written from what the corpus holds now. Only the region
between each pair of markers is rewritten, so the words around a generated block stay the author's.

## What it is not

**It is not `export`.** `generate` writes Markdown into the corpus for a person to read. `export` writes JSON outside it
for an agent. Both are built from the same frontmatter, and a change to one implies nothing about the other.

**`generate --check` is not `mechanism --check`.** This one recomputes a corpus's generated content from that corpus's
own records and compares. The other compares a corpus's authored files against an upstream copy. A file can be fresh and
drifted, or in step and stale.

**It does not stand a type up.** Generation covers what the corpus adopted, so a folder appearing without its type
declared is not something `generate` fills in. `validate` reports it.

## How it works

A run reads the corpus's frontmatter and its schema, and rewrites each generated file and each generated block from
them. It writes only between the markers, and only for the types the corpus adopted.

### What is generated, and from what

| Artefact                                              | Built from                              | Rule                                                                                                                                                                                                                                      |
|-------------------------------------------------------|-----------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `<type>/_index.md`                                    | frontmatter across the folder           | Regenerated **wholly**. Columns and sort come from the schema's `index` block, and it carries a do-not-edit banner. A type with no records yet gets an index saying so rather than a table with no rows. A type with no folder gets none. |
| `<!-- … schema-<type> -->` block in `<type>.md`       | `_universal.yaml` + the type's `fields` | The frontmatter reference table: universal fields first, marked `†`, then the type's own. Each row renders the field's `description`, falling back to `notes` where the schema declares none.                                             |
| `schema-universal` in `knowledge-as-code/metadata.md` | `_universal.yaml`                       | The universal field reference, documented once for the taxonomy rather than per type.                                                                                                                                                     |
| `<!-- … checks-<type> -->` block in `<type>.md`       | the checks the validator implements     | The "What CI checks" table. Rows a type cannot trip (a rule it does not declare, a reciprocal or mirrors-section field it does not have) are omitted, so each page lists only its own checks.                                             |
| five blocks in `knowledge-as-code/taxonomy.md`        | the adopted types                       | `types-placement` holds the decision table, `types-detail` holds the catalogue by tier, `types-versus` holds the disambiguations, and `types-graph` and `types-edges` hold the relation diagram and its edges.                            |
| `types-metadata` in `knowledge-as-code/metadata.md`   | the adopted types                       | Which types carry which of the fields the universal table above it describes.                                                                                                                                                             |
| two blocks in `knowledge-as-code/lineage.md`          | the adopted types                       | `types-lineage` records where each type's name came from, and `types-collisions` records where it already means something else to a reader.                                                                                               |
| `<!-- … types-index -->` block in `README.md`         | the adopted types                       | The corpus's own index of the types it carries. The one block a corpus may decline, by deleting the markers, because the file is the corpus's own.                                                                                        |

`GeneratedFiles` holds that list, so [`validate`](validate.md) holds a corpus to the same blocks this writes. An
`_index.md` carries no markers, so nothing holds a corpus to having one.

### The graph is written to the narrowest renderer

The types graph goes into a fenced Mermaid block, in the subset an Azure DevOps wiki renders. That subset is narrower
than Mermaid's own, and a diagram exceeding it renders nothing at all with no error to say why. So the generator writes
`graph` rather than `flowchart`, uses no subgraphs, and keeps every arrow to `-->`. A corpus publishing somewhere more
capable still receives the same diagram, because one generator writes for every publishing target.

### Only the region between the markers is rewritten

The rest of the file is byte-preserved. Every adopted type is regenerated whether or not it holds records, because the
blocks derive from the schema alone. An index is written for an empty type too: every type page links to one, and a
withheld file would leave a dead link.

### Only adopted types are generated

Generation covers the types the corpus adopted and no others. [`.corpus.yaml`](../corpus-descriptor.md) decides, through
its `types:` key.

A type the corpus declined is left alone whatever `.schema/` says about it, down to the hand-written text between the
markers on a page left behind. Writing there would create an artefact no generated list of this corpus's types names,
and `generate --check` would then hold the corpus to keeping it fresh. Standing a type up without adopting it is a
defect `validate` reports.

### Two rules hold this together

- **CI never commits.** `generate` writes locally. In CI, run `generate --check`: it recomputes the generated content,
  and if any file differs it prints the stale files, names the command to run (`kac generate`), and exits `1`.
  [Contributing](../framework/contributing.md#what-a-pipeline-will-not-do) says what that buys.
- **Output is byte-stable.** Generation is a pure function of frontmatter + schema, so running `generate` twice produces
  no diff. Tables are padded to a width the content decides, `|` is escaped, and files are LF with a trailing newline.
  If a Markdown formatter is added later, the freshness check keeps working instead of failing forever.

