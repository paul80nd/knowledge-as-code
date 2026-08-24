# knowledge-as-code

[![kac][ci-badge]][ci] [![NuGet][nuget-badge]][nuget] [![Licence: MIT][licence-badge]][licence]

Knowledge as Code (KaC) is a framework for knowledge that people and AI sessions both read from and contribute to. It's
plain Markdown in git, reviewed by pull request. But every document carries a type, and every type declares a schema.

`kac` is the tool that holds each document to the schema its type declares. It reports every fault against the file that
caused it. A broken cross-reference fails CI rather than rotting quietly. `kac` writes the indexes and tables nobody
should be keeping by hand. It also writes the documents out as an **export**: data an agent can read.

A repository of those documents, with the schema it runs, is a **corpus**. A document filed under a type is a
**record**. A corpus has two readers and one set of files. A person reads the rendered wiki. An agent reads the export,
built from the same documents. Nothing has to be kept true twice.

An agent can therefore find the standard it needs before it writes code, and leave what it learns where a reviewer will
see it.

Skills, which are instructions an agent loads when it needs them, say how to read a corpus and how to add to it. The
argument for building it this way is in [`template/knowledge-as-code.md`](template/knowledge-as-code.md).

## A type declares its own schema

A **type** is a category such as a policy, a runbook or a glossary. Its **schema** is the machine-readable statement of
what a record of that type carries. The schema travels inside the corpus, in `.schema/`, so adding a type is adding a
YAML file rather than changing the tool.

## What is here

**[`tooling/`](tooling/README.md)** holds `kac`, the validator and generator, and the three test layers that prove it. A
.NET 10 entrypoint over a `kac.core` library, packed as the dotnet tool `KnowledgeAsCode.Tool`, plus the fixtures,
feature specs and unit tests it is held to.

**[`manifest.yaml`](manifest.yaml)** says which files a corpus is made of. A template is not a folder here: it is the
set of files these rules name, read from wherever they are authored, and `to:` on a rule says where each one lands in a
corpus.

**[`.schema/`](.schema/README.md)** is the machine-readable statement of what a record of each type carries, authored
once at the root and read by both corpora below. A corpus of your own carries its own copy, at its own root.

**[`template/`](template/README.md)** holds the rest: the framework's own documentation, the plugin tree, the pages and
templates a corpus starts from, and the repository-shaped files it cannot work without. Some a corpus receives once and
owns afterwards, and some it receives again whenever it takes a newer framework.

**[`example/`](example/README.md)** is a complete corpus that took that template, run through the tool built beside it
on every commit. It holds its own copy of every file the template shares, plus a set of illustrative records about a
fictional library consortium.

**[`docs/`](docs/)** is the documentation site, published to
[GitHub Pages](https://paul80nd.github.io/knowledge-as-code/) on every push to `main`. It is the reference for KaC and
for `kac`, and the one place a command's behaviour is written down. It documents the framework rather than the corpus in
this repository.

`kac` finds a corpus by walking up for a `.corpus.yaml`, so it reads whichever corpus it is run from, and then walks up
again for the `.schema/` to judge it against. That second walk is what lets one schema at this root serve both corpora
here. A corpus of your own holds both files at its own root, so both walks stop there. `example/` proves the tool over
real content rather than over fixtures alone.

## Running the tool

Requires the **.NET 10 SDK**. Each way below needs less of this repository than the one above it.

### From this repository

The development loop, and the way to try the corpus here. `dotnet run` builds as it goes, so there is no build step to
manage.

```bash
git clone https://github.com/paul80nd/knowledge-as-code.git
cd knowledge-as-code/example

dotnet run --project ../tooling/kac -- validate  # frontmatter, links, structure, clauses and the graph
dotnet run --project ../tooling/kac -- generate  # regenerate the indexes and generated blocks
dotnet run --project ../tooling/kac -- checks    # list every check the validator implements
```

That form runs the tool this checkout holds, and it is what CI runs. A `kac` already on your `PATH` is whichever version
you installed last, and it rewrites generated files with an older wording without saying so.

### As an installed tool, packed here

`kac` packs as the dotnet tool `KnowledgeAsCode.Tool`. Pack it yourself to try a change to the tool exactly as a corpus
will receive it, before the version carrying it is published.

```bash
dotnet pack tooling/kac/kac.csproj -o .dist/pack
dotnet tool install --tool-path .dist/tools --add-source .dist/pack KnowledgeAsCode.Tool

cd example
../.dist/tools/kac validate
```

`--tool-path` keeps the install inside this repository, where `.dist/` is untracked. `--global` puts `kac` on your
`PATH` instead. Either way it finds a corpus the way the local one does, and reads whichever corpus it is run from:
this one, or a corpus of your own anywhere on the machine.

### From nuget.org

How a corpus outside this repository takes it with no dependency on this repo.

```bash
dotnet tool install --global KnowledgeAsCode.Tool

cd path/to/your/corpus
kac validate
```

Install it into a [tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use) instead to pin
it, which is what a corpus with CI of its own wants. The version lands in `dotnet-tools.json` at the repository root and
travels with it. Every machine and every build then runs the version the corpus was written against.

```bash
dotnet new tool-manifest
dotnet tool install KnowledgeAsCode.Tool

dotnet tool run kac validate
```

A push to `main` publishes the tool whenever it carries a `<Version>`
[nuget.org](https://www.nuget.org/packages/KnowledgeAsCode.Tool) does not already hold. That publish tags the commit and
opens the release for it. [`tooling/README.md`](tooling/README.md#building) says how that version moves, and
[`tooling/kac/CHANGELOG.md`](tooling/kac/CHANGELOG.md) says what each version carried, and is published as the
site's [changelog](https://paul80nd.github.io/knowledge-as-code/changelog/).

Every command has a page at **<https://paul80nd.github.io/knowledge-as-code/>**, beside a getting-started guide and the
reference for `.corpus.yaml`. [`tooling/README.md`](tooling/README.md) is the other half: how to build the tool, and the
test commands.

## Starting a corpus of your own

**Copy [`template/`](template/) and [`.schema/`](.schema/), not `example/`.** Between them they are the corpus with the
content taken out: the schema, the framework's own documentation, a root page and a template for every type, and nothing
about anybody's estate. `example/` is a worked corpus to read for ideas, and copying it hands you a fictional library
consortium to delete.

```bash
cp -R template/ ../my-corpus
cp -R .schema ../my-corpus/         # authored at this root, and a corpus carries its own copy
cd ../my-corpus
rm README.md                        # the template describing itself, not a corpus's own

# edit .corpus.yaml: the one file no template can answer for you
git init && git add -A              # kac reads the git listing, so a corpus is a repository
                                    # the .gitignore you copied keeps .dist/ and _reports/ out of it

dotnet tool install --global KnowledgeAsCode.Tool
kac generate                        # write the indexes and generated blocks
kac validate                        # comes back clean on an empty corpus
```

[`.corpus.yaml`](example/.corpus.yaml) names the corpus and says where it publishes. The one you copied names the
template, so those are the two values to write first. It may also declare the types the corpus adopted. Leave that key
out and `kac` reads adoption off the folders it finds instead. `example/`'s is commented throughout and is the one to
read while writing yours.

You arrive with ignore rules, editor conventions, a wiki ordering and a starter pipeline for each of the two hosts. Keep
whichever pipeline you run on and delete the other. What you do not arrive with is a `README.md`, which is the one page
nobody else can write for you. A command that stands all of this up for you sits in the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues/242).

**A copy carries no tool, and takes one from outside.** `kac` lives in `tooling/`, which is not part of any corpus.
Install it from nuget.org as above and run it from the copy: it needs nothing but the `.schema/` the copy already
carries.

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
[Contributing](https://paul80nd.github.io/knowledge-as-code/framework/contributing/) names the skills that carry that
rule and say why.

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
