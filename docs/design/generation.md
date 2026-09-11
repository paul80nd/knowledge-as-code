# Generation

What a corpus derives from its own records, where each derived thing lands, and why generation writes into hand-written
files rather than beside them.

A **corpus** is one repository of knowledge records kept in git. A **record** is one Markdown document in it, filed
under a type, with YAML frontmatter above its prose. A **type** is one kind of record, declared in the corpus's own
`.schema/`.

The argument for generating any of it is [Schema before prose](../framework/principles.md#schema-before-prose). Where a
machine could derive something and a person maintains it instead, the person's copy is the one that goes stale, and
nothing says so.

## What is generated, and from what

| Artefact                                              | Built from                              |
|-------------------------------------------------------|-----------------------------------------|
| `<type>/_index.md`                                    | frontmatter across the folder           |
| `schema-<type>` block in `<type>.md`                  | `_universal.yaml` and the type's fields |
| `checks-<type>` block in `<type>.md`                  | the checks the validator implements     |
| `schema-universal` in `knowledge-as-code/metadata.md` | `_universal.yaml`                       |
| `types-metadata` in `knowledge-as-code/metadata.md`   | the adopted types                       |
| five blocks in `knowledge-as-code/taxonomy.md`        | the adopted types                       |
| two blocks in `knowledge-as-code/lineage.md`          | the adopted types                       |
| `types-index` block in `README.md`                    | the adopted types                       |

**`kac` regenerates the index whole.** Its columns and its sort come from the schema's `index` block, and it gets a
do-not-edit banner. A type with no records yet gets an index saying so, rather than a table with no rows. A type with no
folder gets no index at all.

**An index heads a table per folder.** A type that declares a field with `from: sub-path` reads that field out of the
folders below the type, and the index groups its rows on it.
[Discovery](discovery.md#from-sub-path) says how the value is read. Only the first folder heads a table, so
`standards/platform/dotnet/testing.md` joins the Platform table instead of opening one of its own. A type whose records
all sit directly in its folder gets a single table and no headings.

**`kac` drops the derived column where the heading already gives it.** Every row under a heading of Security holds
`security`, so the column says nothing the reader has not read. A record filed deeper keeps it: `platform/dotnet` under
a heading of Platform is the one place `dotnet` is written down.

**The frontmatter reference lists the universal fields first, marked `†`, then the type's own.** Each row renders that
field's `description`, falling back to `notes` where the schema declares none.

**The checks table leaves out the rows a type cannot trip.** Each row carries a predicate over the type's own
declaration, so a rule it does not declare, a field shape it does not use and a parts source it does not have are all
left out. Each page lists only its own checks.

**`schema-universal` documents the universal fields once for the whole taxonomy**, in `knowledge-as-code/metadata.md`,
as the schema declares them. A type page lists them again under `†`, narrowed to whatever that type made of them.

**The taxonomy has five blocks.** `types-placement` is the decision table, `types-detail` is the catalogue by tier,
`types-versus` is the disambiguations, and `types-graph` and `types-edges` are the relation diagram and its edges.

**Lineage has two.** `types-lineage` records where each type's name came from, and `types-collisions` records where that
name already means something else to a reader.

**The corpus's own README block is the one a corpus may decline**, by deleting the pair of markers around it. The file
belongs to the corpus, so the choice does too.

`GeneratedFiles` holds this list, and [`validate`](../cli/validate.md) checks a corpus against the same blocks. An
`_index.md` has no markers, so nothing checks that a corpus has one.

## The region between the markers

Generated content sits between markers inside otherwise hand-written files. You keep your prose and the generator keeps
the tables current. `kac` preserves the rest of the file byte for byte.

`kac` regenerates every adopted type whether or not it has records, because the blocks derive from the schema alone. It
writes an index for an empty type too, since every type page links to one and a withheld file would leave a dead link.

If a block loses its markers, `generate` no longer writes it.
[Discovery](discovery.md#generated-block-markers) is the pass that catches that.

## Which types are generated

Generation covers the types listed in `types:` in [`.corpus.yaml`](../corpus-descriptor.md) and no others. A corpus that
has not declared the key yet has its adoption read off the folders instead, and a type counts there only where both its
page and its folder are present.

`kac` leaves a declined type alone whatever `.schema/` says about it, down to the hand-written text between the markers
on a page left behind. Writing there would create an artefact no generated list of this corpus's types mentions, and
`generate --check` would then hold the corpus to keeping it fresh. Standing a type up without adopting it is a defect
[`validate`](../cli/validate.md) reports.

## The Mermaid subset

The types graph goes into a fenced Mermaid block, in the subset an Azure DevOps wiki renders. That subset is narrower
than Mermaid's own, and a diagram that exceeds it renders nothing at all, with no error to say why.

So the generator writes `graph` rather than `flowchart`, uses no subgraphs, and keeps every arrow to `-->`. A corpus
publishing somewhere more capable gets the same diagram, because one generator writes for every publishing target.

Those three are what the rule covers, and node shapes are not among them. Nobody has held a wiki to one, so the
generator draws every node as `[...]` and the question stays unasked. A diagram on this site is free of the rule
entirely, because no wiki reads one.

## Byte-stable output

Generation is a pure function of frontmatter and schema, so running it twice produces no diff. `kac` pads tables to a
width the content decides, escapes `|`, and writes files LF with a trailing newline.

That is what makes `--check` meaningful. It recomputes every generated file and compares, so a difference is real
staleness rather than a formatting wobble.

## Why a pipeline never commits

Where generated content is stale the build fails and names the command to run locally.
[Contributing](../framework/contributing.md#what-a-pipeline-will-not-do) says what a pipeline that pushed fixes would
cost instead.

## Where to go next

[`generate`](../cli/generate.md) is the command that runs this, and `--check` is what CI runs instead.
