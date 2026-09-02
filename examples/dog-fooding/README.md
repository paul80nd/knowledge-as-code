# `dog-fooding` ... a tiered example corpus defined against the KaC framework itself

> **Everything here is real.** Unlike the other example corpora this one describes the repository it sits in: the tool,
> the docs site, the marketplace branch, and the rules a change to any of them answers to. It's here to test the
> framework against itself as per 'eating your own dogfood'.

```text
                                              YOU ARE HERE
                                                   ▼
┌───────────────┐      ┌─────────────┐      ┌─────────────┐
│ kac framework │ ━━━► │ engineering │ ━┳━► │ dog-fooding │
└───────────────┘      └─────────────┘  ┃   └─────────────┘
        ╎                       ╎       ┃   ┌─────────────┐
   The framework defines how    ╎       ┗━► │ payments    │
   knowledge is structured      ╎           └─────────────┘
                                ╎               ╎
        Parent corpus defines cross-cutting     ╎
        engineering expectations and policies   ╎
                                                ╎
             Downstream corpora adopt those expectations
             and extend them within a specific domain
```

> [/examples/README.md](../README.md) provides the overview of the KaC example corpora - read it first.

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

**A corpus that stands up before it is full.** Most types here are declared, generated and validated with no record in
them, which is the state a corpus created this morning is in. `kac validate` holds a type to what it declared whether or
not anything has been filed under it.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted.

It declines `adrs`, because a decision about `kac` is recorded in `tooling/CLAUDE.md` and in the commit that made it,
and moving those here is a separate call. It declines `glossary`, because the framework's own vocabulary belongs to
`../engineering/` and this corpus cites it as `eng:`.
