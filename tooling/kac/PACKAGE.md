# kac: knowledge as code

`kac` is the command-line tool for Knowledge as Code (KaC), which keeps your engineering knowledge in git, where people
and AI coding sessions can both find it. Standards, decisions, runbooks, known fixes and the glossary each become a
**record**: a Markdown document with a declared type, reviewed by pull request like any other change. A repository of
records, with the schema it runs, is a **corpus**.

`kac` checks every record against the schema its type declares, and reports each fault against the file that caused
it. A broken cross-reference fails CI. `kac` writes the indexes nobody should keep by hand. It also packages the
records as a Claude Code **plugin**, so an agent can look up a standard before it writes code.

## Why use it

* **Knowledge gets written down.** A record has a type and an id, so anybody can find it.
* **You can tell whether to trust a record.** Each one has an owner, a status and a review date.
* **Agents build to your rules.** A session looks up the standard that applies and cites the rule it followed.
* **Teams share one set of rules.** A team's corpus can consume an organisation's corpus, so a standard written once
  applies in every repository that takes it.
* **The schema is yours.** `kac` has no rules about any particular type. You add a knowledge type by writing a YAML
  file.

## Get started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and git.

```bash
dotnet tool install --global KnowledgeAsCode.Tool
mkdir my-corpus && cd my-corpus
kac new         # asks what the corpus is called and which types it adopts, each with a default
kac validate    # checks every record against the schema its type declares
```

`kac new` turns the folder into a corpus. It writes the framework files and a `.corpus.yaml` that names your corpus. It
runs `generate` and `validate`, then stages what it wrote. The first commit is yours to make.

## The commands

`kac` finds a corpus by walking up from the working directory for a `.corpus.yaml`, so run it from anywhere inside one.

```bash
kac validate     # frontmatter, links, structure, clauses and the graph
kac generate     # regenerate the indexes and the generated blocks in each type page
kac update       # take a newer framework from the template this corpus was made from
kac restore      # fetch the corpora this one consumes into .imports/
kac export       # write the corpus to .dist/export/ as data a consumer reads instead of cloning
kac bundle       # assemble that export and .plugin/ into an installable plugin
kac pack         # zip that export into a versioned package a registry can hold
kac report       # print a report over this corpus and what it imports
kac checks       # list every check the validator can report
```

`kac update` writes into the working tree and commits nothing, so `git diff` is where you read what arrived.
`kac update --check` reports the same and writes nothing, which is the form a pipeline runs.

`kac --help` lists the commands, and every command takes its own `--help`.

## Exit codes

| Code | Meaning                                                                                 |
|------|-----------------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                        |
| `1`  | A corpus error, or a bad invocation.                                                    |
| `2`  | A command found no corpus. `new`, `--version` and `--help` need none and answer anyway. |

## Documentation

* [Getting started](https://paul80nd.github.io/knowledge-as-code/use/getting-started/) walks through a first corpus,
  from install to the first commit.
* [Compared with alternatives](https://paul80nd.github.io/knowledge-as-code/compared/) says what KaC adds to a
  `CLAUDE.md`, an organisation's managed instructions, a documentation server or a wiki, and when one of those is
  enough.
* [The CLI reference](https://paul80nd.github.io/knowledge-as-code/use/cli/) has a page for every command.
* [The GitHub repository](https://github.com/paul80nd/knowledge-as-code) has the source, and worked corpora you can read
  before you write your own.

Released under the MIT licence.
