# Generation

`kac generate` rewrites the parts of a corpus that `kac` derives from frontmatter and the schema. A corpus is one
repository of knowledge records kept in git. This page lists what generation is built to do, and the scenario proving
each behaviour. [`generate`](../use/cli/generate.md) and [Generation](../reference/generation.md) own the full
statement of each one.

## What it does

* It rebuilds each adopted type's `_index.md` whole from the frontmatter in the type's folder.
  [What is generated](../reference/generation.md#what-is-generated-and-from-what)
* It writes the type pages' blocks, the framework pages' blocks and the README's `types-index` block.
  [What is generated](../reference/generation.md#what-is-generated-and-from-what)
* It writes a block only between its markers, and keeps the rest of the file byte for byte.
  [The region between the markers](../reference/generation.md#the-region-between-the-markers)
* It leaves a block whose markers are gone unwritten, and `validate` reports the loss.
  [The region between the markers](../reference/generation.md#the-region-between-the-markers)
* It covers only the types `types:` lists, and leaves a declined type's files alone.
  [Which types are generated](../reference/generation.md#which-types-are-generated)
* It creates no page, folder or index for an adopted type that has no folder.
  [`generate`](../use/cli/generate.md#known-limits)
* It writes nothing where every generated file is fresh. [A plain run](../use/cli/generate.md#a-plain-run)
* With `--check` it exits `1` where a file is stale, exits `0` where none is, and writes nothing either way.
  [A stale check](../use/cli/generate.md#a-stale-check)

## What it promises a consumer

* Two runs over one corpus produce no diff. [Byte-stable output](../reference/generation.md#byte-stable-output)
* The types graph uses the Mermaid subset an Azure DevOps wiki renders.
  [The Mermaid subset](../reference/generation.md#the-mermaid-subset)

## Specified by

[`Generate.feature`][Generate.feature]:

* A run rewrites a stale index whole, and no other file
* A run rewrites a block between its markers and keeps the words around it
* A run over a fresh corpus writes nothing
* A check over a fresh corpus passes
* A check over a stale corpus fails and writes nothing
* A type the descriptor does not adopt gets no index
* A type adopted with no folder is not created

[`Shape.feature`][Shape.feature]:

* Every file carrying a generated block is checked for its markers, and for its links

## Unspecified

* What an index holds: its columns and sort, its banner, a table per sub-path folder, and the message for a type with
  no records. A scenario could read `_index.md`. [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* What the frontmatter reference and the checks table on a type page hold.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A block whose markers are gone left unwritten by `generate`.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A corpus declining the README block by deleting its markers.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* The types graph keeping to the Mermaid subset. A scenario could read the taxonomy's `types-graph` block.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* What a run prints: the up-to-date line, each stale file `--check` names, and the `run: kac generate` hint. A scenario
  cannot read printed output.

## Where to go next

[Generation](../reference/generation.md) says what each generated file is built from.

[Generate.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Generate.feature
[Shape.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Shape.feature
