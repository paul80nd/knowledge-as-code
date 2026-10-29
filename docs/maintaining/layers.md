# Layers and update

`kac new` and `kac update` write the framework's files into a corpus, deciding each file by the layer the template
gives it. A corpus is one repository of knowledge records kept in git. The template is the repository or folder a
corpus takes the framework from, and a layer says who owns a file. This page lists what the two commands are built to
do, and the scenario proving each behaviour. [Layers](../use/layers.md), [`new`](../use/cli/new.md) and
[`update`](../use/cli/update.md) own the full statement of each one.

## What it does

### `new`

* It writes the files the template's manifest sends, and composes `.corpus.yaml`, `.config/dotnet-tools.json` and
  `README.md` itself.
  [Files no template can send](../use/layers.md#files-no-template-can-send)
* It runs `git init` in a folder that is not a repository, and keeps a file the folder already holds.
  [`new`](../use/cli/new.md#known-limits)
* It writes a corpus that validates, whichever types it adopts. [`new`](../use/cli/new.md#what-it-does)
* It removes the links to the types the corpus declined, and keeps the links between the types it adopted.
  [`new`](../use/cli/new.md#what-it-does)
* It writes the author as the `owner` of each record it seeds, from `git config user.email` or `--owner`.
  [A seeded record's owner](../use/layers.md#a-seeded-records-owner)
* It writes the CI starter for the system named, and no other. [CI starters](../use/layers.md#ci-starters)
* It pins the release of the `kac` running it in `.config/dotnet-tools.json`.
  [Files no template can send](../use/layers.md#files-no-template-can-send)
* It writes `.plugin/hooks/breadcrumb` with its execute bit.
  [Files no template can send](../use/layers.md#files-no-template-can-send)

### `update`

* It writes an overlay file wherever the corpus holds it differently.
  [One file, one layer](../use/layers.md#one-file-one-layer)
* It writes a seed file only where it is absent, or under the `full` policy. [Seed files](../use/layers.md#seed-files)
* It leaves a seeded record the corpus filed deeper.
  [A seeded record that moved](../use/layers.md#a-seeded-record-that-moved)
* It deletes only what `layer: removed` names. [Deletions](../use/layers.md#deletions)
* It never writes a path under `skip:`. [`skip:`](../use/layers.md#skip)
* It writes none of a shared plugin tree to a corpus naming `plugin.from`, and `--check` fails on an old copy left
  under `.plugin/`. [A shared plugin tree](../use/layers.md#a-shared-plugin-tree)
* It leaves every change in the working tree and commits nothing. [`update`](../use/cli/update.md#what-it-does)
* `--check` writes nothing, exits non-zero where anything would change, and runs over a tree in any state.
  [A check that writes nothing](../use/cli/update.md#a-check-that-writes-nothing)
* `--check` reports an overlay file the corpus holds that the template sends nothing to.
  [`--check`](../use/layers.md#-check)
* It reports a type the corpus has not adopted, and adopts nothing. [`update`](../use/cli/update.md#known-limits)
* `--add-type` writes the type's page, template and schema, and adds it to `types:`.
  [A type adopted, or given up](../use/cli/update.md#a-type-adopted-or-given-up)
* It writes the author as the `owner` of each record it seeds.
  [A seeded record's owner](../use/layers.md#a-seeded-records-owner)
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
* an owner the `owner` pattern refuses [A seeded record's owner](../use/layers.md#a-seeded-records-owner)

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
* `update` leaves `.config/dotnet-tools.json` as the corpus wrote it.
  [Files no template can send](../use/layers.md#files-no-template-can-send)
* A file missing from the template is never deleted. [Deletions](../use/layers.md#deletions)

## Specified by

[`Creation.feature`][Creation.feature]:

* A folder already inside a corpus is refused
* A repository holding uncommitted work is refused
* A folder with no repository has one initialised
* A folder with no repository and nobody to ask is refused
* A repository holding its own committed files keeps them
* A template that cannot be read is refused
* A template needing a newer kac is refused
* A new corpus is given a descriptor and a README of its own
* A corpus created with --yes is seeded with records its author owns
* An owner given as a flag is written into the seeded records
* An owner the pattern refuses is refused
* A publishing target nothing builds links for is refused
* A new corpus pins the kac that created it
* An update leaves the tool manifest to the corpus
* A corpus built by GitHub is sent the GitHub starter alone
* Each hook script arrives with its execute bit
* A corpus that declined types is sent no link it cannot follow
* A corpus adopting a single type validates
* A reference to a declined type's page loses its definition and keeps its words
* A corpus adopting every type keeps the links between its type pages
* A full update changes nothing on the corpus new just created
* A corpus publishing nowhere states no tracker, and validates

[`Update.feature`][Update.feature]:

* A check over a corpus in step passes and writes nothing
* A check over a corpus behind its framework fails and writes nothing
* An update takes the framework's copy of a file the corpus holds differently
* An update commits nothing and stages nothing
* An update leaves a seed file the corpus changed
* An update under the full policy writes the seed files again
* An update leaves a seeded record the corpus filed deeper
* An update deletes what the manifest retires, and nothing else
* An update never writes a path the corpus skips
* An update writes none of a plugin tree the corpus reads from elsewhere
* A check over a corpus keeping a copy of a plugin tree it reads from elsewhere fails
* A check over a corpus keeping a framework file the template does not send fails
* An update stamps the descriptor format this tool writes
* An update from a template needing a newer kac is refused
* An update over a tree with uncommitted changes is refused
* A check runs over a tree with uncommitted changes
* A type the corpus has not adopted is not adopted by an update
* An update adding a type writes its page and template, and adopts it
* An update adding a type writes its author as the owner of the records it seeds
* An update dropping a type whose folder holds no record deletes its page
* An update dropping a type whose folder holds a record is refused
* An update dropping a type with nobody to answer is refused
* An update over a descriptor using a renamed key is refused

## Unspecified

* `--drop-type` asking before it deletes, and taking no for an answer. The specs have no terminal to answer on.
* The plan `--check` prints, and the note that an arriving type's pages are not linked from the pages already there.
  A scenario cannot read what a command prints.
* The line `new` prints where the corpus has no tracker to file on. A scenario cannot read the output, so `NewTests`
  asserts it.

## Where to go next

[The corpus descriptor](../use/corpus-descriptor.md) is the reference for every key `new` writes and `update` reads.

[Creation.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Creation.feature
[Update.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Update.feature
