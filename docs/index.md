# knowledge-as-code

`kac` reads a folder of Markdown documents and holds each one to the schema its type declares. It reports every fault
against the file that caused it. It writes the indexes and tables nobody should be keeping by hand. It also writes the
whole folder out as data an agent can read.

That folder is a **corpus**, meaning one repository of knowledge documents kept in git and reviewed by pull request. A
document filed under a type is a **record**. A **type** is a category such as a policy, a runbook or a glossary. Each
type declares a **schema**: the machine-readable statement of what a record of that type carries. The schema travels
inside the corpus, in `.schema/`, so adding a type is adding a YAML file rather than changing this tool.

```bash
dotnet tool install --global KnowledgeAsCode.Tool

cd path/to/your/corpus
kac validate     # frontmatter, links, structure, clauses and the graph
```

A corpus has two readers and one set of files. A person reads the rendered wiki. An agent reads an **export**, meaning
the corpus written out as data, built from the same frontmatter. Nothing has to be kept true twice, and a broken
cross-reference fails CI rather than rotting quietly.

## What this site is

The reference for `kac`: what each command does, what it refuses, and what it leaves alone.

* **[Getting started](getting-started.md)** installs the tool and runs it against a corpus.
* **[CLI reference](cli/validate.md)** gives a page to each command, saying what it is for and how it behaves.
* **[The corpus descriptor](corpus-descriptor.md)** covers `.corpus.yaml`, the one file a corpus writes for itself.
* **[Checks](checks.md)** says where a check comes from, and how a rule declared in YAML reaches the validator.

## What lives elsewhere

Three documents sit outside this site, each written for a different reader.

**The argument for working this way** is
[`knowledge-as-code.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/template/knowledge-as-code.md). Every
corpus receives its own copy of it and may then diverge.

**How to change the tool** is
[`tooling/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/README.md), beside the code. It
covers building `kac`, its three test layers, and what will bite you while editing any of it.

**How to author a schema** is
[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/template/.schema/README.md). It travels
with the schema it describes, so a corpus reads its own copy.

## Maturity

**Early.** The tool is real and tested. The version on
[nuget.org](https://www.nuget.org/packages/KnowledgeAsCode.Tool) sits below `1.0.0` because the commands may still
change shape.

Every page here describes what exists today. The
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds what has been considered and not yet built.
Two pages break that rule and say so at their head: [`new`](cli/new.md) and [`update`](cli/update.md) are
specifications, written before their commands.
