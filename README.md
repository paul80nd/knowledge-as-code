# knowledge-as-code

[![kac][ci-badge]][ci] [![NuGet][nuget-badge]][nuget] [![Licence: MIT][licence-badge]][licence]

A structured, validated knowledge corpus that people and AI sessions both read from and contribute to, and the tool that
holds it to its shape.

Plain markdown in git, reviewed by PR, published as a wiki. What makes it more than a folder of documents is that
**every document has a type, and every type has a schema**. The tool builds each index from the records. A broken
cross-reference fails CI rather than rotting quietly. Skills help an agent use and contribute to the corpus.

The argument for building it this way is in [`template/knowledge-as-code.md`](template/knowledge-as-code.md).

## What is here

**[`tooling/`](tooling/README.md)** ... `kac`, the validator and generator, and the three test layers that prove it. A
.NET 10 entrypoint over a `kac.core` library, packed as the dotnet tool `KnowledgeAsCode.Tool`, plus the fixtures,
feature specs and unit tests it is held to.

**[`template/`](template/README.md)** ... what a corpus is made of, authored once: the machine-readable schema, the
framework's own documentation, the plugin tree, and the pages and templates a corpus starts from.
[`template/manifest.yaml`](template/manifest.yaml) sorts them: some a corpus receives once and owns afterwards, and some
it receives again whenever it takes a newer framework.

**[`example/`](example/README.md)** ... a complete corpus that took that template, run through the tool built beside it
on every commit. It holds its own copy of everything the template overlays, plus a set of illustrative records about a
fictional library consortium.

No folder contains another. `kac` finds a corpus by walking up for a `.schema/`, so it reads whichever corpus it is run
from. The one in this repository proves the tool over real content rather than over fixtures alone.

## Running the tool

Requires the **.NET 10 SDK**. Each way below needs less of this repository than the one above it.

### From this repository

The development loop, and the way to try the corpus here. `dotnet run` builds as it goes, so there is no build step to
manage.

```bash
git clone https://github.com/paul80nd/knowledge-as-code.git
cd knowledge-as-code/example

./kac validate     # frontmatter, links, structure, clauses and the graph
./kac index        # regenerate the indexes and generated blocks
./kac checks       # list every check the validator implements
```

`./kac` (Windows: `kac.cmd`) sits at the corpus root and wraps `dotnet run --project ../tooling/kac`. Add that folder to
your `PATH` to drop the `./`. The explicit form works the same way, and is what CI runs.

### As an installed tool, packed here

`kac` packs as the dotnet tool `KnowledgeAsCode.Tool`. Packing it yourself is how to try a change to the tool exactly as
a corpus will receive it, before the version carrying it is published.

```bash
dotnet pack tooling/kac/kac.csproj -o .dist/pack
dotnet tool install --tool-path .dist/tools --add-source .dist/pack KnowledgeAsCode.Tool

cd example
../.dist/tools/kac validate
```

`--tool-path` keeps the install inside this repository, where `.dist/` is untracked. `--global` puts `kac` on your
`PATH` instead. Either way it finds a corpus by walking up for a `.schema/`, the way the local one does. It reads
whichever corpus it is run from: this one, or a corpus of your own anywhere on the machine.

### From nuget.org

How a corpus outside this repository takes it with no dependency on this repo.

```bash
dotnet tool install --global KnowledgeAsCode.Tool

cd path/to/your/corpus
kac validate
```

Install it into a [tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use) instead to pin
it, which is what a corpus with CI of its own wants. The version lands in `.config/dotnet-tools.json` and travels with
the repository. Every machine and every build then runs the version the corpus was written against.

```bash
dotnet new tool-manifest
dotnet tool install KnowledgeAsCode.Tool

dotnet tool run kac validate
```

A push to `main` publishes the tool whenever it carries a `<Version>`
[nuget.org](https://www.nuget.org/packages/KnowledgeAsCode.Tool) does not already hold.
[`tooling/README.md`](tooling/README.md#building) says how that version moves.

Every command, one document apiece, is in [`tooling/features/`](tooling/features/).
[`tooling/README.md`](tooling/README.md) maps them and carries the test commands.

## Starting a corpus of your own

Copy `example/`, delete the records in the types you keep, rewrite the type pages' examples in your own domain, and
start writing. `.schema/` comes with it and is the half you want to keep receiving changes to.

**A copy carries no tool, and takes one from outside.** `kac` lives in `tooling/`, which is not part of the corpus.
Install it from nuget.org as above and run it from the copy. It needs nothing but the `.schema/` the copy already
carries, and a corpus outside this repository validates and generates exactly as `example/` does.

**What a copy cannot yet do is take a newer framework.** `kac mechanism` wants a reference corpus, through `--against`
or an `upstream.url`. Past that it reads a manifest at `tooling/manifest.yaml` that no corpus holds. A command that
updates the schema beneath a corpus sits in the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues), and is not described here.

## Maturity

**Early.** The tool is real and tested. It validates the schema, frontmatter, identity, structure, clauses, links, the
graph and the type setup, and generates the indexes and reference tables from the same pass. Three test layers stand
behind it, and a round-trip that installs what was built and asks it questions.

The taxonomy is the half that is only partly proven.
[`example/README.md`](example/README.md#maturity) records which types have met real content and which are still drafts.

Every document here describes what exists today, and the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds everything considered but, as yet, unbuilt.
[Write what exists](template/knowledge-as-code/authoring.md#write-what-exists) is the rule, and says why.

## Opinions

Stated openly, because they are load-bearing:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order`
  drives navigation, and `/`-rooted links resolve from the corpus root. Everything degrades to plain markdown elsewhere,
  but the sharp edges were filed against ADO.
* **Seventeen types is a lot.** It is the most likely thing to be wrong here. The mitigation is a decision table, and a
  standing willingness to merge types that are not earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content, it is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.
* **You own the content, and install the tool.** An organisation adopting this gets its own cut of the schema and the
  documentation, free to diverge. `kac` arrives from nuget.org as a version they pin.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.

[ci]: https://github.com/paul80nd/knowledge-as-code/actions/workflows/kac.yml
[ci-badge]: https://github.com/paul80nd/knowledge-as-code/actions/workflows/kac.yml/badge.svg
[nuget]: https://www.nuget.org/packages/KnowledgeAsCode.Tool
[nuget-badge]: https://img.shields.io/nuget/v/KnowledgeAsCode.Tool
[licence]: LICENSE
[licence-badge]: https://img.shields.io/badge/licence-MIT-blue
