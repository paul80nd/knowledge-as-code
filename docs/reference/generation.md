# Generation

This page says what a corpus derives from its own records, and where each derived thing lands.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, filed
under a type, with YAML frontmatter above its prose. A **type** is one kind of record, declared in the corpus's own
`.schema/`.

## What is generated, and from what

| Artefact                                              | Built from                              |
|-------------------------------------------------------|-----------------------------------------|
| `<type>/_index.md`                                    | frontmatter across the folder           |
| `schema-<type>` block in `<type>.md`                  | `_universal.yaml` and the type's fields |
| `checks-<type>` block in `<type>.md`                  | the checks the validator implements     |
| `schema-universal` in `knowledge-as-code/metadata.md` | `_universal.yaml`                       |
| `checks-universal` in `knowledge-as-code/metadata.md` | the checks every type shares            |
| `types-metadata` in `knowledge-as-code/metadata.md`   | the adopted types                       |
| four blocks in `knowledge-as-code/taxonomy.md`        | the adopted types                       |
| two blocks in `knowledge-as-code/lineage.md`          | the adopted types                       |
| `types-index` block in `README.md`                    | the adopted types                       |

**`kac` regenerates the index whole.** Its columns and its sort come from the schema's `index` block, and it gets a
do-not-edit banner. A type with no records yet gets an index saying so, rather than a table with no rows. A type with no
folder gets no index at all.

**An index lists the records in force first.** A record is in force where its `status` is the value its type names
in `in-force:`. Where any record is not in force, the index heads the records in force `## In force` and the rest
`## Not in force`. An index whose records are all in force has neither heading. No record moves, so no link to one
breaks.

**An index heads a table per folder.** A type that declares a field with `from: sub-path` reads that field out of the
folders below the type, and the index groups its rows on it.
[Discovery](../use/discovery.md#from-sub-path) says how the value is read. Only the first folder heads a table, so
`standards/platform/dotnet/testing.md` joins the Platform table instead of opening one of its own. A type whose records
all sit directly in its folder gets a single table and no headings. Under `## In force` and `## Not in force`, each
folder heads its table one level down, at `###`.

**A column heading is its field name in sentence case, with an initialism in capitals.** `review-by` heads Review by,
and `their-sla` heads Their SLA. `kac` recognises `id` and `sla`. A field whose initialism is neither of those heads a
column in sentence case, so open an issue on this repository where you declare one.

**`kac` drops the derived column where every row in a table repeats its heading.** One record filed deeper keeps the
column for the whole table, with each full value. Under Platform, `platform/containers.md` shows `platform` and
`platform/dotnet/testing.md` shows `platform/dotnet`.

**The frontmatter reference lists the universal fields the type refines, marked `†`, then the type's own.** A
universal field whose row would read as it does on `metadata.md` is left out, and the legend links that page. `tier`
and `status` always print, because each type sets its own values. Each row renders that field's `description`,
falling back to `notes` where the schema declares none.

**The checks table leaves out the rows a type cannot trip**: a rule it does not declare, a field shape it does not use,
and a parts source it does not have. It also leaves out the universal checks, which every type shares, and opens on a
link to them.

**`checks-universal` lists the universal checks once**, in `knowledge-as-code/metadata.md`. A universal check is one
that asks nothing of the type's schema and runs on every record.

**`schema-universal` documents the universal fields once for the whole taxonomy**, in `knowledge-as-code/metadata.md`,
as the schema declares them. A type page lists again only the ones it refines, under `†`.

**The taxonomy has four blocks.** `types-placement` is the decision table, `types-detail` is the catalogue by tier,
`types-versus` is the disambiguations, and `types-graph` is the relation diagram.

**Lineage has two.** `types-lineage` records where each type's name came from, and `types-collisions` records where that
name already means something else to a reader.

**The corpus's own README block is the one a corpus may decline**, by deleting the pair of markers around it.

[`validate`](../use/cli/validate.md) checks a corpus against the same blocks. An `_index.md` has no markers, so nothing
checks that a corpus has one.

## The region between the markers

Generated content sits between markers inside otherwise hand-written files. `kac` preserves the rest of the file byte
for byte, and regenerates every adopted type's blocks whether or not it has records. If a block loses its markers,
`generate` no longer writes it, and [Discovery](../use/discovery.md#generated-block-markers) reports the loss.

## Which types are generated

Generation covers the types listed in [`types:`](../use/corpus-descriptor.md#types) and no others. `kac` leaves a
declined type alone whatever `.schema/` says about it, down to the hand-written text between the markers on a page left
behind.

## The Mermaid subset

The types graph goes into a fenced Mermaid block, in the subset an Azure DevOps wiki renders. A diagram that exceeds it
renders nothing there. So the generator writes `graph` and never `flowchart`, uses no subgraphs, keeps every arrow to
`-->`, and draws every node as `[...]`. It labels each edge with its field, as `-- text -->`. A reciprocal pair is one
edge labelled with both fields, as `-- superseded-by / supersedes -->`. Every publishing target gets the same diagram.

## Byte-stable output

Running `generate` twice produces no diff. `kac` pads tables to a width the content decides, escapes `|`, and writes
files LF with a trailing newline. So a difference `--check` reports is real staleness.

## Where to go next

[`generate`](../use/cli/generate.md) is the command that runs this, and `--check` is what
[CI runs](../use/ci.md#read-only-permission).
