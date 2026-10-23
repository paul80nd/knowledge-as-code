# `restore`

What a first `kac restore` writes, asserted through the CLI. The consumer in `corpus/` declares one import, and its
`source:` is the folder `../producer/package`, the form the example corpora use for the corpus next door. The runner
lays `producer/` beside the consumer, so that path resolves inside the run's own temp folder.

[`tests/README.md`](../../README.md) says what the `restore` mode asserts.

## The package

`producer/package/example-libraries.2.3.4.nupkg` is the real output of `kac pack` over the
[`pack` fixture](../pack/README.md)'s corpus, committed as an input. Its `manifest.json` records the moment it was
packed, and every run restores those same bytes.

Remake it when the package format moves, and a restore refuses this one:

1. Copy `.schema/` from the repository root and the contents of `../pack/corpus/` into one empty folder.
2. From that folder, run `kac export` and then `kac pack`, with the `kac` built on your branch.
3. Replace `producer/package/` with the `.dist/package/` folder the pack wrote.
4. Run `dotnet run tooling/kac-tests.cs -- --update restore`, then read the diff.
