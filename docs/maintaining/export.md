# Export

`kac export` writes a corpus into `.dist/export/` as data a consumer reads without cloning it. A corpus is one
repository of knowledge records kept in git, and the export is the set of files it publishes. This page lists what the
export is built to do, and the scenario proving each behaviour. [`export`](../use/cli/export.md) and
[the export format](../reference/export.md) own the full statement of each one.

## What it does

* It writes a manifest, a file per record and a flat file per type. [The tree](../reference/export.md#the-tree)
* It loads the corpus whole, and `--type` narrows what it writes to one type.
  [How a run writes it](../reference/export.md#how-a-run-writes-it)
* It deletes `.dist/export/` and writes it again, so a record deleted from the corpus leaves nothing behind.
  [How a run writes it](../reference/export.md#how-a-run-writes-it)
* It writes only under `.dist/export/`, and never into the corpus. [The tree](../reference/export.md#the-tree)
* It records the commit and whether the tree was dirty, unknown where git could not say.
  [A dirty tree](../use/cli/export.md#a-dirty-tree)
* It exports a draft or a lapsed record with its own state. `export.exclude:` withholds either from this corpus's own
  records, and never an inherited one.
  [How a run writes it](../reference/export.md#how-a-run-writes-it)
* It merges what the corpus consumes into each flat file, and files an inherited record under its producer's
  shortcode. [A consumer inherits](../reference/export.md#a-consumer-inherits-what-its-producers-published)
* It keeps a grandparent's shortcode on a record that arrived through a chain.
  [A chain of any depth](../reference/export.md#a-chain-of-any-depth)
* It reads a cross-reference into `seeAlso` only where the link names a part.
  [A cross-reference is read](../reference/export.md#a-cross-reference-is-read-never-inferred)
* It writes each part line's link, spelled for the publishing target.
  [The address](../reference/export.md#the-address)

## What it refuses

Each refusal exits `1` and writes nothing. [What stops the run](../reference/export.md#what-stops-the-run)

* a `--type` the corpus has not adopted
* a consumed corpus with nothing restored
* a consumed corpus at an export format this build does not read
* a type exported by two corpora at two shapes, or with a section at two fidelities
* a type two corpora state at two in-force statuses
* one corpus arriving twice, at two versions

## What it promises a consumer

* Absent is `null`, and `shortcode` is the one key that may be missing.
  [What holds in every file](../reference/export.md#what-holds-in-every-file)
* Two runs from one commit write identical bytes but for `generatedAt`.
  [What holds in every file](../reference/export.md#what-holds-in-every-file)
* `formatVersion` comes from the build, `shapeVersion` from each type's schema and `contentVersion` from
  `.corpus.yaml`, so none moves another.
  [Three versions](../reference/export.md#three-versions-and-none-implies-another)

## Specified by

[`Export.feature`][Export.feature]:

* An export writes a manifest, a file per record and a flat file per type
* An export of one type writes that type alone
* An export of a type the corpus has not adopted is refused
* An export rebuilds .dist/export/ whole
* The manifest records whether the tree was dirty
* A draft or a lapsed record travels with its own state
* export.exclude withholds the records it names
* A cross-reference adds to seeAlso only where it names a part
* A part line carries the link to its part, spelled for the target
* Each of the three versions is read from its own source
* What the corpus consumes travels under the shortcode of the corpus that wrote it
* export.exclude withholds this corpus's own records alone
* An export refuses a consumed corpus that has not been restored
* An export refuses a consumed corpus at a format it cannot read
* An export refuses a consumed corpus exporting a section at another fidelity
* An export refuses a consumed corpus exporting a type at another shape
* An export refuses one corpus arriving twice, at two versions

## Unspecified

* What every file holds, and two runs writing identical bytes. The golden suite diffs the export against
  `expected-dist/` twice per run, and no scenario restates it.

## Where to go next

[The export format](../reference/export.md) is the contract a consumer reads these files against.

[Export.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Export.feature
