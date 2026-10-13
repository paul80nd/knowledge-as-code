# Layers and update

`kac new` and `kac update` write the framework's files into a corpus, deciding each file by the layer the template
gives it. A corpus is one repository of knowledge records kept in git. The template is the repository or folder a
corpus takes the framework from, and a layer says who owns a file. This page lists what the two commands are built to
do, and the scenario proving each behaviour. [Layers](../use/layers.md), [`new`](../use/cli/new.md) and
[`update`](../use/cli/update.md) own the full statement of each one.

## What it does

### `new`

* It writes the files the template's manifest sends, and composes `.corpus.yaml` and `README.md` itself.
  [Three files no template can send](../use/layers.md#three-files-no-template-can-send)
* It runs `git init` in a folder that is not a repository, and keeps a file the folder already holds.
  [`new`](../use/cli/new.md#known-limits)
* It writes a corpus that validates, whichever types it adopts. [`new`](../use/cli/new.md#what-it-does)
* It removes the links to the types the corpus declined, and keeps the links between the types it adopted.
  [`new`](../use/cli/new.md#what-it-does)
* It writes the CI starter for the system named, and no other. [CI starters](../use/layers.md#ci-starters)

### `update`

* It writes an overlay file wherever the corpus holds it differently.
  [One file, one layer](../use/layers.md#one-file-one-layer)
* It writes a seed file only where it is absent, or under the `full` policy. [Seed files](../use/layers.md#seed-files)
* It leaves a seeded record the corpus filed deeper.
  [A seeded record that moved](../use/layers.md#a-seeded-record-that-moved)
* It deletes only what `layer: removed` names. [Deletions](../use/layers.md#deletions)
* It never writes a path under `skip:`. [`skip:`](../use/layers.md#skip)
* It writes none of a shared plugin tree to a corpus naming `plugin.from`.
  [A shared plugin tree](../use/layers.md#a-shared-plugin-tree)
* It leaves every change in the working tree and commits nothing. [`update`](../use/cli/update.md#what-it-does)
* `--check` writes nothing, exits non-zero where anything would change, and runs over a tree in any state.
  [A check that writes nothing](../use/cli/update.md#a-check-that-writes-nothing)
* `--check` reports an overlay file the corpus holds that the template sends nothing to.
  [`--check`](../use/layers.md#--check)
* It reports a type the corpus has not adopted, and adopts nothing. [`update`](../use/cli/update.md#known-limits)
* `--add-type` writes the type's page, template and schema, and adds it to `types:`.
  [A type adopted, or given up](../use/cli/update.md#a-type-adopted-or-given-up)
* `--drop-type` deletes the page of a type whose folder holds no record, once it has an answer.
  [A type adopted, or given up](../use/cli/update.md#a-type-adopted-or-given-up)
* It stamps `descriptor-version` with the format this tool writes. [`update`](../use/cli/update.md#known-limits)

## What it refuses

Each refusal exits `1` and writes nothing.

### `new`

* a folder already inside a corpus [`new`](../use/cli/new.md#known-limits)
* a repository holding uncommitted changes [`new`](../use/cli/new.md#known-limits)
* a folder with no repository, no terminal to ask on and no `--yes` [`new`](../use/cli/new.md#known-limits)
* a template it cannot read [`new`](../use/cli/new.md#known-limits)

### `update`

* a tree holding uncommitted changes, unless the run is `--check` [`update`](../use/cli/update.md#known-limits)
* a `--drop-type` whose folder still holds a record
  [A type adopted, or given up](../use/cli/update.md#a-type-adopted-or-given-up)
* a `--drop-type` with no terminal to ask on and no `--yes`
  [A type adopted, or given up](../use/cli/update.md#a-type-adopted-or-given-up)
* a descriptor using a renamed key [`update`](../use/cli/update.md#known-limits)

### Both

* a template whose `minimum-tool` is newer than this `kac` [The two halves](../use/layers.md#the-two-halves)

## What it promises a consumer

* A corpus `new` has just created is in step: a full update changes nothing. [Layers](../use/layers.md)
* A seed file belongs to the corpus once written. [Seed files](../use/layers.md#seed-files)
* A file missing from the template is never deleted. [Deletions](../use/layers.md#deletions)

## Specified by

[`Creation.feature`][Creation.feature]:

* A folder already inside a corpus is refused
* A repository holding uncommitted work is refused
* A folder with no repository has one initialised
* A repository holding its own committed files keeps them
* A template that cannot be read is refused
* A corpus that declined types is sent no link it cannot follow
* A corpus adopting a single type validates
* A corpus adopting every type keeps the links between its type pages
* A full update changes nothing on the corpus new just created

[`Update.feature`][Update.feature]:

* A check over a corpus in step passes and writes nothing
* A check over a corpus behind its framework fails and writes nothing
* An update takes the framework's copy of a file the corpus holds differently
* An update leaves a seed file the corpus changed
* An update under the full policy writes the seed files again
* An update over a tree with uncommitted changes is refused
* A check runs over a tree with uncommitted changes
* A type the corpus has not adopted is not adopted by an update
* An update adding a type writes its page and template, and adopts it
* An update dropping a type whose folder holds no record deletes its page
* An update dropping a type whose folder holds a record is refused
* An update dropping a type with nobody to answer is refused
* An update over a descriptor using a renamed key is refused

## Unspecified

* `new` composing `.corpus.yaml` and `README.md`, and writing the CI starter for the system named.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* `new` refusing a folder with no repository when there is nobody to ask.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A seeded record filed deeper being left alone. [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* `layer: removed` deleting what it names, and nothing else.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A path under `skip:` never being written. [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A corpus naming `plugin.from` receiving none of the shared plugin tree.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* `--check` reporting an overlay file the template sends nothing to.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* `update` stamping `descriptor-version`. [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* A template whose `minimum-tool` is newer than this `kac` being refused.
  [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* The breadcrumb hook arriving with its execute bit. [#827](https://github.com/paul80nd/knowledge-as-code/issues/827)
* `--drop-type` asking before it deletes, and taking no for an answer. The specs have no terminal to answer on.
* The plan `--check` prints, and the note that an arriving type's pages are not linked from the pages already there.
  A scenario cannot read what a command prints.

## Where to go next

[The corpus descriptor](../use/corpus-descriptor.md) is the reference for every key `new` writes and `update` reads.

[Creation.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Creation.feature
[Update.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Update.feature
