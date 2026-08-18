# `index` — generation

## Intent

`index` regenerates the content of a corpus that is derived from frontmatter and the schema, so that nobody maintains
it by hand. Its readers are the people who read the corpus: a type's index page, the frontmatter reference and checks
table on a type page, and the taxonomy's own tables are all written from what the corpus holds now. Only the region
between each pair of markers is rewritten, so the words around a generated block stay the author's.

## What it is not

**It is not `export`.** `index` writes Markdown into the corpus for a person to read. `export` writes JSON outside it
for an agent. Both are built from the same frontmatter, and a change to one implies nothing about the other.

**`index --check` is not `mechanism --check`.** This one recomputes a corpus's generated content from that corpus's own
records and compares. The other compares a corpus's authored files against an upstream copy. A file can be fresh and
drifted, or in step and stale.

**It will not stand a type up.** Generation covers what the corpus adopted, so a folder appearing without its type
declared is not something `index` fills in. `validate` reports it.

## Approach

`index` writes the content derived from frontmatter and the schema:

| Artefact                                             | Built from                              | Rule                                                                                                                                                                                                                               |
|------------------------------------------------------|-----------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `<type>/_index.md`                                   | frontmatter across the folder           | Regenerated **wholly**; columns and sort come from the schema's `index` block; carries a do-not-edit banner. A type with no records yet gets an index saying so rather than a table with no rows; a type with no folder gets none. |
| `<!-- … schema-<type> -->` block in `<type>.md`      | `_universal.yaml` + the type's `fields` | The frontmatter reference table — universal fields first, marked `†`, then the type's own. Each row renders the field's `description`, falling back to `notes` where the schema declares none.                                     |
| `<!-- … schema-universal -->` block in `metadata.md` | `_universal.yaml`                       | The universal field reference, documented once for the taxonomy rather than per type.                                                                                                                                              |
| `<!-- … checks-<type> -->` block in `<type>.md`      | the checks the validator implements     | The "What CI checks" table. Rows a type cannot trip — a rule it does not declare, a reciprocal or mirrors-section field it does not have — are omitted, so each page lists only its own checks.                                    |
| five blocks in `knowledge-as-code/taxonomy.md`       | the adopted types                       | `types-placement`, `types-detail`, `types-versus`, `types-graph`, `types-edges` — the decision table, the catalogue by tier, the disambiguations, the relation diagram and its edges.                                              |
| `<!-- … types-metadata -->` block in `metadata.md`   | the adopted types                       | Which types carry which of the fields the universal table above it describes.                                                                                                                                                      |
| two blocks in `knowledge-as-code/lineage.md`         | the adopted types                       | `types-lineage` and `types-collisions` — where each type's name came from, and where it already means something else to a reader.                                                                                                  |
| `<!-- … types-index -->` block in `README.md`        | the adopted types                       | The corpus's own index of the types it carries. The one block a corpus may decline, by deleting the markers, because the file is the corpus's own.                                                                                 |

`GeneratedFiles` holds that list, so `validate` holds a corpus to the same files and blocks this writes.

Only the region **between** each `BEGIN`/`END` marker is rewritten; the rest of the file is byte-preserved. Every
adopted type is regenerated whether or not it holds records: the blocks derive from the schema alone, and an index that
waits for its first record is a dead link from the type page until then.

Generation covers the types the corpus adopted and no others. `types:` in `.corpus.yaml` decides, and a corpus that has
not declared is read off its folders — a type counts where both halves are there, the page and the folder.

A type the corpus declined is left alone whatever `.schema/` says about it, down to the hand-written text between the
markers on a page left behind. Writing there would create an artefact no generated list of this corpus's types names,
and `index --check` would then hold the corpus to keeping it fresh. Standing a type up without adopting it is a defect
`validate` reports.

Two rules hold this together:

- **CI never commits.** `index` writes locally. In CI, run `index --check`: it recomputes the generated content, and if
  any file differs it prints the stale files, names the command to run (`dotnet run .tooling/kac.cs -- index`, or just
  `./kac index`), and exits `1`. A pipeline never pushes.
- **Output is byte-stable.** Generation is a pure function of frontmatter + schema, so running
  `index` twice produces no diff. Tables use fixed column widths, `|` is escaped, and files are LF with a trailing
  newline — so if a Markdown formatter is added later, the freshness check keeps working instead of failing forever.

