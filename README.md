# knowledge-as-code

[![kac][ci-badge]][ci] [![NuGet][nuget-badge]][nuget] [![Licence: MIT][licence-badge]][licence]

Knowledge as Code (KaC) keeps your engineering knowledge in git, where people and AI coding sessions can both find it.
Standards, decisions, runbooks, known fixes and the glossary each become a **record**: a Markdown document with a
declared type, reviewed by pull request like any other change.

`kac` is the command-line tool that checks every record against the schema its type declares. It reports each fault
against the file that caused it, and a broken cross-reference fails CI. It writes the indexes nobody should keep by
hand. It also packages the records as a Claude Code **plugin**, so an agent can look up a standard before it writes
code.

A repository of records, with the schema it runs, is a **corpus**. People read it as a wiki. Agents read the plugin,
built from the same files, so nothing has to be kept true twice.

An agent writes back by filing an issue on your tracker. A person decides whether what it found becomes a record.

## Why use it

* **Knowledge gets written down.** What one person remembers, or an old chat thread says, becomes a record anybody can
  find by its type and its id.
* **You can tell whether to trust a record.** Each one has an owner, a status and a review date, so you can see who
  is responsible for it and whether it is current.
* **Agents build to your rules.** A session looks up the standard that applies and cites the rule it followed, such as
  `std-SECRET.a-secret-comes-from-the-store`.
* **Teams share one set of rules.** A team's corpus can consume an organisation's corpus, so a standard written once
  applies in every repository that takes it.

## A record, and what `kac validate` reports

This is a standard from [`examples/engineering`](examples/engineering/standards/common/secret-handling.md), cut short.
`kac` checks the frontmatter against the schema for `standard`. Each heading under `## Rules` becomes one rule an agent
can search and cite.

```markdown
---
id: std-SECRET
type: standard
tier: normative
status: draft
implements: [ pol-SCRT.STORE, pol-SCRT.EMBED ]
applies-to:
  - all
review-by: "2027-08-26"
owner: human:paul.law
---

# A secret is read from the store at run time

## Rules

### A secret comes from the store

- A service **MUST** read every secret from the managed store, through an identity granted to the workload.
```

A **type**, such as a standard or a runbook, declares its fields and rules in a YAML file under `.schema/`. Adding a
type to your corpus means adding a YAML file. `kac` itself does not change.

When a record breaks its schema, `kac validate` reports the file, the check and the line, and exits `1`:

```text
services/catalogue-web.md
  warning  [dependency-criticality]  'depends-on' cites 'svc-search', graded 'important' where this record is 'critical'. Regrade one of the two, or write down what degrades when it is gone.  (services/catalogue-web.md:13)
  error    [link-resolves]           link target 'missing-runbook.md' does not resolve.  (services/catalogue-web.md:79)

validated 27 document(s) and 9 template(s), skipped 0 without frontmatter. 1 error(s), 1 warning(s)
```

## Get started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and git.

```bash
dotnet tool install --global KnowledgeAsCode.Tool

mkdir my-corpus && cd my-corpus
kac new         # asks what the corpus is called and which types it adopts, each with a default
kac validate    # checks every record against the schema its type declares
```

`kac new` writes the framework files and a `.corpus.yaml` that names your corpus. It runs `generate` and `validate`,
then stages what it wrote. The first commit is yours to make.

To give the corpus to an agent, build the plugin:

```bash
kac export      # writes the records out as data under .dist/export/
kac bundle      # packages that export as a Claude Code plugin under .dist/plugin/
```

`bundle` ends by printing the command that adds the plugin to Claude Code.

Read next:

* [Use it](https://paul80nd.github.io/knowledge-as-code/use/) shows you how to install the tool, run it over a corpus,
  wire it into CI and build the plugin.
  [Getting started](https://paul80nd.github.io/knowledge-as-code/use/getting-started/) is its first page.
* [The framework](https://paul80nd.github.io/knowledge-as-code/framework/) explains how the types are laid out, where
  they came from, and how to write a record.
* [Schema reference](https://paul80nd.github.io/knowledge-as-code/reference/) covers the keys a type file takes, the
  rule expression grammar, the checks and the export contract.
* [`examples/`](examples/README.md) has worked corpora. Read them to see real records, and start your own with
  `kac new`.

Later, [`kac update`](https://paul80nd.github.io/knowledge-as-code/use/cli/update/) takes a newer framework. It leaves
every change in the working tree, so `git diff` is your review.

## Maturity

**Early.** The tool is tested and published on [nuget.org](https://www.nuget.org/packages/KnowledgeAsCode.Tool), below
`1.0.0` because its commands may still change shape.

The set of types is the part least proven. [`examples/library`](examples/library/README.md#maturity) and
[`examples/engineering`](examples/engineering/README.md#maturity) record which types have met real content.

Every document here describes what exists today. The
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) lists everything considered and not yet built.

## Opinions

Stated openly, because each one shapes the design:

* **Azure DevOps wiki is the primary publishing target.** Frontmatter renders as a metadata table there, `.order`
  drives navigation, and `/`-rooted links resolve from the corpus root. Everything degrades to plain markdown elsewhere,
  but the sharp edges were filed against ADO.
* **The taxonomy may be too large.** It is the most likely thing to be wrong here. The mitigation is a decision table,
  and a standing willingness to merge types that are not earning their place.
* **Trust matters more than coverage.** The failure mode of a wiki is not too little content. It is content nobody
  believes. Generated indexes, validated links and immutable decisions all serve that.
* **You own the content, and install the tool.** An organisation adopting this gets its own copy of the schema and the
  documentation, free to diverge. `kac` arrives from nuget.org as a version they pin.

## Contributing

This repository is not open to contributions of code. Raise an issue instead. A pull request opened without a prior
conversation is closed unmerged.
[Contributing to `kac`](https://paul80nd.github.io/knowledge-as-code/maintaining/contributing/) says what a pull request
owes and which checks gate it. To publish a version of your own,
[Forking `kac`](https://paul80nd.github.io/knowledge-as-code/maintaining/forking/) lists what to replace.

This repository contains the tool, the framework a corpus receives, the corpora that prove both, and the trials that
measure its documentation.

| Path                                     | What it contains                                                         |
|------------------------------------------|--------------------------------------------------------------------------|
| [`tooling/`](tooling/README.md)          | `kac`, and the tests behind it                                           |
| [`.schema/`](.schema/README.md)          | the schema for every type, read by every corpus in this repository       |
| [`template/`](template/README.md)        | the framework documentation, plugin tree and pages a corpus starts from  |
| [`manifest.yaml`](manifest.yaml)         | which files a corpus receives, and where each one lands                  |
| [`examples/`](examples/README.md)        | the worked corpora, which CI validates on every commit                   |
| [`docs/`](docs/)                         | the [documentation site](https://paul80nd.github.io/knowledge-as-code/)  |
| [`trials/`](trials/README.md)            | repeatable experiments on the documentation, with every run kept         |

Run the tool this checkout contains from inside one of the example corpora:

```bash
cd examples/library
dotnet run --project ../../tooling/kac -- validate
```

To change `kac`, read:

* [Maintaining `kac`](https://paul80nd.github.io/knowledge-as-code/maintaining/) on the documentation site, for how
  `kac` is put together, built, tested and released, and how to add a check in C# or a report.
* [`tooling/README.md`](tooling/README.md) for what to know while changing the code.
* [`tooling/kac/CHANGELOG.md`](tooling/kac/CHANGELOG.md) for what each version brought.

## Licence

Released under the [MIT licence](LICENSE), so that any organisation adopting this keeps an unencumbered copy.

[ci]: https://github.com/paul80nd/knowledge-as-code/actions/workflows/kac.yml
[ci-badge]: https://github.com/paul80nd/knowledge-as-code/actions/workflows/kac.yml/badge.svg
[nuget]: https://www.nuget.org/packages/KnowledgeAsCode.Tool
[nuget-badge]: https://img.shields.io/nuget/v/KnowledgeAsCode.Tool
[licence]: LICENSE
[licence-badge]: https://img.shields.io/badge/licence-MIT-blue
