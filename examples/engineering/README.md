# `engineering` ... a tiered example corpus for engineering governance

> **Everything here is invented.** Example Engineering is a fictional engineering organisation. Its policies are written
> to be stack-agnostic, so they name no service and invent no estate, and the compliance posture standing behind them is
> made up.

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
| [ADR](adrs.md)           | decided     | An architecturally significant decision affecting more than one repository, and the reasoning behind it.        |
| [Control](controls.md)   | normative   | How a standard's rules are verified: the mechanism, the frequency, and the evidence it leaves.                  |
| [Glossary](glossary.md)  | descriptive | The ubiquitous language. Terms whose meaning is specific to us, or which are easily confused.                   |
| [Policy](policies.md)    | normative   | A high-level engineering commitment: the what and the why, largely stack-agnostic and changing rarely.          |
| [Standard](standards.md) | normative   | The rulebook, imperative, RFC 2119, with concrete examples and a conformance checklist.                         |
| [Tool](tools.md)         | descriptive | The approved-software register. What is chosen, rejected or deprecated, and the version ranges we stand behind. |

**Where does a document go?** The [taxonomy](knowledge-as-code/taxonomy.md) has the decision table, what each type is
and is not, and the calls that are genuinely close.

<!-- END GENERATED: types-index -->

## Working in this corpus

Needs `kac` on your path. [`../../README.md`](../../README.md#running-the-tool) covers the ways to get one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and generated blocks
kac export       # write the corpus to .dist/export/ as data a consumer reads
kac bundle       # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks       # list every check the validator implements
```

While you are changing the tool, run `dotnet run --project ../../tooling/kac -- validate` instead. That reaches the
working tree, and an installed `kac` does not.

## The policies are the part worth reading

The clause model, the mnemonic ids, the per-clause alignment and the gap analysis that closed it were worked out on
these records rather than assumed. Policies alone forced the mnemonic id style, a category field, the identity line, the
clause table and the checks that hold it.

They are principle-level and stack-agnostic by design, so they name no service and invent no domain. That is why they
would survive adoption with only the specifics rewritten, where a service catalogue would not.

## What the corpus claims about its organisation

Showing how framework alignment works requires something to align with, so this corpus takes a position: ISO/IEC 27001
registered, obliged by the UK GDPR, running on Azure, a public-sector body bound by accessibility duties, and part of a
management system whose other halves belong to facilities, HR and IT. None of it describes anyone. It is there so that
[`frameworks.md`](frameworks.md) and the policies' `Alignment` columns have something to point at. Rewrite it on the way
through.

The Azure assumption is the demonstration content's alone: the Azure Well-Architected entry in
[`frameworks.md`](frameworks.md) and the pillars the policies cite from it. AWS and Google publish near-identical
pillars, so changing it is a relabelling rather than a re-mapping. No clause's wording names a provider, only the
references beside it.

## Maturity

**Policies and standards are the proven types here.** The other four schemas have met little real content, and two of
them hold none at all. That is the honest limit: a schema is wrong in ways only real content reveals.

## What this corpus declares about itself

[`.corpus.yaml`](.corpus.yaml) says what this corpus is, which of the framework's types it has adopted, where its
published form is served from, and any deviation from the shared baseline it has deliberately accepted. Three versions
live in it, each named for what it versions, and the file's own comments say what each one is for.

Declaring `types:` states a decision rather than the shape the folders happen to have. `validate` then holds the corpus
to standing up everything it declared, and every generated list of types is written from that declaration. A corpus that
declares nothing still works: the tool reads adoption off the folders instead.

## What it publishes

`kac export` writes this corpus to `.dist/export/` as data a consumer reads, and `kac bundle` assembles that export and
the [`.plugin/`](.plugin/) tree into an installable Claude Code plugin. Only the glossary carries an `export:` block
today, so what a consumer takes from here is a vocabulary and not the policies. Exporting a clause is
[#277](https://github.com/paul80nd/knowledge-as-code/issues/277).

Nothing publishes this corpus's plugin. `../library/` is the one this repository pushes to its marketplace branch, and
its README walks that path.
