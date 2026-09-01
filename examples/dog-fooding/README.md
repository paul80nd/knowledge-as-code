# Example Dogfooding: this repository, held to its own framework

> **Everything here is real.** The corpora beside it describe invented estates. This one describes the repository it
> sits in: the tool, the site, the branch, and the rules a change to any of them answers to. A record here is wrong if
> the repository disagrees with it.

A corpus is plain markdown in git where every document has a type and every type has a schema. This one is a domain
corpus in the same shape as [`../payments/`](../payments/): it consumes [`../engineering/`](../engineering/) for its
governance and writes only what is its own. Its domain is the building and publishing of `kac`.
[`../README.md`](../README.md) sets it beside the others here and says what each one demonstrates.

**A new corpus starts from [`../../template/`](../../template/)**, which is the same corpus with the content taken out.

Why it is built this way is in [`knowledge-as-code.md`](knowledge-as-code.md) and the documents beneath it.

## The knowledge types

<!-- BEGIN GENERATED: types-index -->

| Type                     | Tier        | What it holds                                                                                                   |
|--------------------------|-------------|-----------------------------------------------------------------------------------------------------------------|
| [Control](controls.md)   | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                  |
| [Runbook](runbooks.md)   | procedural  | An incident-time procedure read under pressure: terse, imperative, structured as a decision tree.               |
| [Service](services.md)   | descriptive | One deployable component: purpose, repo, platform, environments, dependencies, data stores, owner.              |
| [Standard](standards.md) | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](tools.md)         | descriptive | The approved-software register. What is chosen, rejected or deprecated, and the version ranges we stand behind. |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../../README.md`](../../README.md#running-the-tool) covers the ways to get one.

```bash
kac restore      # fetch the corpora this one consumes into .imports/
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

`restore` comes first, and it needs a package to take. Run `kac export` and then `kac pack` in
[`../engineering/`](../engineering/) once, and this corpus's `source:` has a folder to read.

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## What it demonstrates

**A corpus about the repository holding it.** The corpora beside it prove that the framework holds for an invented
estate, which is a weaker claim than it sounds: nothing pushes back when the fiction is convenient. Here the estate
answers back. A service record naming the wrong workflow is caught by whoever next reads the workflow, and a standard
nobody follows is visible as a standard nobody follows.

**A corpus that stands up before it holds anything.** Every type here is declared, generated and validated with no
record in it, which is the state a corpus created this morning is in. `kac validate` holds it to the types it declared
either way.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted.

It declines `adrs`, because a decision about `kac` is recorded in `tooling/CLAUDE.md` and in the commit that made it,
and moving those here is a separate call. It declines `glossary`, because the framework's own vocabulary belongs to
`../engineering/` and this corpus cites it as `eng:`.

## Layout

```
<type>.md              # what the type is, why it exists, how to contribute. One per type
<type>/
  ├── _index.md        # GENERATED from frontmatter
  ├── _template.md     # what humans and agents copy
  └── <records>.md

frameworks.md          # the external frameworks this corpus refers to, and what each one obliges
knowledge-as-code.md   # the approach, and the way in to everything below
knowledge-as-code/     # the system's own documentation
  ├── taxonomy.md      # the types and where things go
  ├── metadata.md      # the frontmatter fields
  ├── contributing.md  # the way in for somebody adding to this corpus
  └── lineage.md       # where the taxonomy's names came from
.corpus.yaml           # what this corpus is, and where it publishes
.plugin/               # source for the plugin that carries this corpus's export to another repository
```

The machinery is dot-prefixed: `.corpus.yaml` and `.plugin/`. The markdown stays the visible half, so an Azure DevOps
wiki published from this tree shows knowledge rather than mechanism.

The `.schema/` this corpus is judged against sits at the repository root, one copy shared with every corpus here
and with `template/`. A corpus outside this repository carries its own at its own root, which is where `kac` looks
first.
