# Pack and restore

`kac pack` seals a corpus's export into a versioned package, and `kac restore` fetches the packages a corpus consumes. A
corpus is one repository of knowledge records kept in git. This page lists what the two commands are built to do, and
the scenario proving each behaviour. [`pack`](../use/cli/pack.md), [`restore`](../use/cli/restore.md) and
[Imports](../use/imports.md) own the full statement of each one.

## What it does

### `pack`

* It writes one file under `.dist/package/`, named for the corpus and its content version.
  [What it does](../use/cli/pack.md#what-it-does)
* It reads the export alone, and writes nothing into the corpus or the export.
  [What it does](../use/cli/pack.md#what-it-does)
* It puts the three envelope files at the root, and the export byte for byte under `corpus/`.
  [The package contents](../use/cli/pack.md#the-package-contents)
* Two packs of one export are the same file. [The package contents](../use/cli/pack.md#the-package-contents)
* It includes what the corpus consumes, because the export does.
  [What an export inherits](../use/imports.md#what-an-export-inherits)
* It takes what a registry lists from `description`, `author` and `license`, and files a corpus naming nobody under its
  own id. [What it does](../use/cli/pack.md#what-it-does)
* It states the repository URL that `--repository` passes.
  [A push to a registry](../use/cli/pack.md#a-push-to-a-registry)

### `restore`

* It fetches each corpus `consumes:` lists, unpacks it under `.imports/<shortcode>/` and writes `resolved:`.
  [What it does](../use/cli/restore.md#what-it-does)
* A corpus that consumes nothing restores nothing. [What it does](../use/cli/restore.md#what-it-does)
* A caret range takes the newest version with the same major, and below `1.0.0` the same minor.
  [The `consumes:` block](../use/cli/restore.md#the-consumes-block)
* A prerelease is restored only where the range names it exactly. [Known limits](../use/cli/restore.md#known-limits)
* A folder holding what `pack` wrote serves as a source. [A folder source](../use/cli/restore.md#a-folder-source)
* A lock the range admits is taken without asking the source, even where a newer version exists.
  [A repeat run](../use/cli/restore.md#a-repeat-run)
* A repeat run fetches nothing and writes the same bytes. [A repeat run](../use/cli/restore.md#a-repeat-run)
* A range that moves re-resolves its lock. [A move to a newer version](../use/cli/restore.md#a-move-to-a-newer-version)

## What it refuses

Each refusal exits `1` and writes nothing.

### `pack`

[A refusal](../use/cli/pack.md#a-refusal)

* an export that is not there
* an export stating no `corpus`, no `content-version` or no `shortcode`
* a corpus name a registry cannot take as a package id
* a content version a registry cannot order

### `restore`

[A refusal](../use/cli/restore.md#a-refusal) and [Known limits](../use/cli/restore.md#known-limits)

* a package whose own manifest states a shortcode other than the one declared
* a range no version in the source satisfies
* a range in any form but `1.2.0` or `^1.2.0`, and a caret over a prerelease
* two entries claiming one shortcode, and two entries consuming one corpus
* a folder that no longer holds the locked version
* every entry, where any one of them fails
* a package with an entry past 16MB, or past 256MB in all

## What it promises a consumer

* A package is a released version, stored by a registry that never overwrites one.
  [A push to a registry](../use/cli/pack.md#a-push-to-a-registry)
* `restore` sends the registry token only to an `https://` source.
  [A private feed](../use/cli/restore.md#a-private-feed)

## Specified by

[`Pack.feature`][Pack.feature]:

* A pack writes one package named for the corpus and its content version
* The package holds the envelope and the export byte for byte
* Two packs of one export are the same file
* A package includes what its corpus consumes
* A corpus that names nobody is filed under its own id
* What a registry lists comes from the corpus
* A pack for a repository states its URL
* A pack with no export is refused
* A pack whose export lacks a fact it is named or cited by is refused
* A pack of a corpus whose name a registry cannot take is refused
* A pack of a content version a registry cannot order is refused

[`Restore.feature`][Restore.feature]:

* A corpus that consumes nothing restores nothing
* A first restore unpacks the corpus and locks its version
* A range takes the newest version it admits
* A range below 1.0.0 admits the same minor version alone
* A repeat run writes nothing
* A lock the range admits is taken without asking the source
* A lock the range admits is kept when a newer version exists
* A move of the range re-resolves the lock
* A prerelease is restored where the range names it exactly
* A restore sends the registry token to an https source
* A restore sends no registry token over plain HTTP
* A restore refuses a shortcode the package does not carry
* A restore refuses a range no version in the source satisfies
* A restore refuses a range of any other form
* A restore refuses two entries claiming one shortcode
* A restore refuses two entries consuming one corpus
* A restore refuses a folder that no longer holds the locked version
* A restore refuses everything where one entry fails
* A restore refuses a package holding an entry past 16MB

## Unspecified

* A package past 256MB in all. A scenario would read that much on every run.
* What either command prints: the `wrote` line, the "consumes nothing" line, and the versions a refusal lists. A
  scenario cannot read the output.
* `pack` neither publishing nor asking whether a version is new. There is nothing a run leaves to observe.

## Where to go next

[Imports](../use/imports.md) follows a package from the producer's export to the citation a consumer resolves.

[Pack.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Pack.feature
[Restore.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Restore.feature
