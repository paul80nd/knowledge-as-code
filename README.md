# knowledge-as-code

A structured, validated knowledge corpus that people and AI sessions both read from and contribute to, and the tool
that holds it to its shape.

Plain markdown in git, reviewed by PR, published as a wiki. What makes it more than a folder of documents is that
**every document has a type, and every type has a schema** — so an index is generated rather than maintained, a broken
cross-reference fails CI rather than rotting quietly, and an agent can be told where a thing goes instead of guessing.

The argument for building it this way is in [`example/knowledge-as-code.md`](example/knowledge-as-code.md) and the
documents beneath it.

## What is here

**[`tooling/`](tooling/)** — `kac`, the validator and generator, and the three test layers that prove it. A .NET 10
file-based entrypoint over a `kac.core` library, plus the fixtures, feature specs and unit tests it is held to.
[`tooling/README.md`](tooling/README.md) is how to build and test it.

**[`template/`](template/)** — what a corpus is made of, authored once: the machine-readable schema, the framework's
own documentation, the plugin tree, and the pages and templates a corpus starts from.
[`template/manifest.yaml`](template/manifest.yaml) says which of those a corpus receives once and owns afterwards, and
which it receives again whenever it takes a newer framework.

**[`example/`](example/)** — a complete corpus that took that template, run through the tool built beside it on every
commit. It holds its own copy of everything the template overlays, plus a set of illustrative records about a
fictional library consortium. [`example/README.md`](example/README.md) is the way in.

No folder contains another. `kac` finds a corpus by walking up for a `.schema/`, so it reads whichever corpus it is
run from, and the one in this repository is what proves the tool over real content rather than over fixtures alone.

## Getting started

Requires the **.NET 10 SDK**. `kac` runs through `dotnet run`, so there is no build step to manage.

```bash
git clone https://github.com/paul80nd/knowledge-as-code.git
cd knowledge-as-code/example

./kac validate     # frontmatter, links, structure, clauses and the graph
./kac index        # regenerate the indexes and generated blocks
./kac checks       # list every check the validator implements
```

`./kac` (Windows: `kac.cmd`) sits at the corpus root and wraps `dotnet run ../tooling/kac.cs`. Add that folder to your
`PATH` to drop the `./`, or use the explicit form, which is what CI runs.

Every command, one document apiece, is in [`tooling/features/`](tooling/features/).
[`tooling/README.md`](tooling/README.md) maps them and carries the test commands.

## Starting a corpus of your own

Copy `example/`, delete the records in the types you keep, rewrite the type pages' examples in your own domain, and
start writing. `.schema/` comes with it and is the half you want to keep receiving changes to.

**A copy carries no tool today.** `kac` lives in `tooling/`, which is not part of the corpus, so a copy is run against
a checkout of this repository beside it. `kac mechanism` cannot answer for a copy either: it reads a manifest the
corpus no longer holds. Packaging the tool so a corpus installs and pins a version of it is the work that closes
both, and it is tracked in the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) rather than described here.

## Maturity

**Early.** The tool is real and tested: it validates the schema, frontmatter, identity, structure, clauses, links, the
graph and the type setup, and generates the indexes and reference tables from the same pass. Three test layers stand
behind it, and a round-trip that installs what was built and asks it questions.

The taxonomy is the half that is only partly proven.
[`example/README.md`](example/README.md#maturity) records which types have met real content and which are still
drafts.

Every document here describes what exists today, and the
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds everything agreed and unbuilt.
[Write what exists](example/knowledge-as-code/authoring.md#write-what-exists) is the rule, and says why.

## Opinions

Stated openly, because they are load-bearing:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order`
  drives navigation, and `/`-rooted links resolve from the corpus root. Everything degrades to plain markdown
  elsewhere, but the sharp edges were filed against ADO.
* **Seventeen types is a lot.** It is the most likely thing to be wrong here, and the mitigation is a decision table
  plus a standing willingness to merge types that are not earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content, it is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.
* **This framework is copied, not depended on.** An organisation adopting it gets its own cut, free to diverge, with
  nothing to remove if they later want to go their own way.

## Provenance

Developed against a real engineering wiki and extracted once the mechanism was separable from the content. History
starts fresh here by design — the original commits are interleaved with a client's decisions and belong with them.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.
