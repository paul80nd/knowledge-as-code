A corpus that consumes another, with the import restored beside it.

The [`imports`](../imports/README.md) fixture is the other half of the pair. It declares the same thing with nothing
restored, and `validate` reports that. This one declares it, holds it, and exports.

`.imports/eng/` is committed here, which no real corpus does: a restore is fetched rather than kept, and each corpus's
own `.gitignore` says so. A fixture has to hold one, because what an export makes of an import is the thing being
pinned.

## What the export has to get right

**One file per type, whoever wrote the records.** `glossary/terms.jsonl` holds this corpus's term and both of the ones
it inherited. A consumer greps once for every term that reaches it.

**A record file is filed under the corpus that wrote it.** `glossary/eng/gls-shared.json` sits beside
`glossary/gls-local.json`. Two corpora can name one record, so a directory is what keeps them apart. A line needs no
directory, because it says whose it is.

**A bare id is this corpus's own.** `gls-local.shelf` carries no prefix and no `shortcode`, which is the rule a
citation already follows. `eng:gls-shared.record` carries both, and its `seeAlso` is stamped too: left bare it would
point at whatever this corpus happens to call the same thing, and resolve to the wrong record rather than to none.

**A grandparent is labelled once.** `gp:gls-old.store` arrived inside eng's export already naming `gp`, and keeps that
name. Its `shortcode` stays `gp` rather than becoming `eng`, and `sources` carries `gp` alongside `eng`, each at its own
commit. That is the whole of what makes a chain of any depth resolve, and it costs no code beyond carrying the list
forward.

**A type this corpus never adopted travels whole.** `policies/` is eng's alone. It arrives because a record here may
cite a clause of it, and an address resolves only where the thing it names travelled too.

**`sources` is the one thing a merge cannot merge.** Each entry holds its producer's own publishing block. A record of
`eng` is read at eng's commit in eng's repository, and this corpus's own template gets both wrong.

## What is asserted where

The expectations [the suite README](../../README.md) describes for an `export` scenario, and it says what each one
carries.

`expected-dist/` is the export itself, committed file for file. A diff under it is a change to a published contract.
Regenerate it with `dotnet run tooling/kac-tests.cs -- --update export-inherited`, read what moved, and say in the
commit message why it moved.
