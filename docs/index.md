# knowledge-as-code

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

## A type declares its own schema

A **type** is a category such as a policy, a runbook or a glossary. Its **schema** is the machine-readable statement of
what a record of that type carries. The schema travels inside the corpus, in `.schema/`, so adding a type is adding a
YAML file rather than changing the tool.

## What this site is

The reference for KaC: what a corpus holds, and what `kac` does to one.

* **[Getting started](getting-started.md)** installs the tool and runs it against a corpus.
* **[The framework](framework/index.md)** is the ideas the tool serves: what a type and a tier are, the types that ship,
  what a record carries, and how knowledge is contributed.
* **[Running it in CI](ci.md)** wires the two commands that answer for a corpus into a pull request, on GitHub Actions
  or Azure Pipelines.
* **[CLI reference](cli/index.md)** gives a page to each command, saying what it does, what it refuses and what it
  leaves alone. The overview carries the exit codes and the options every command takes.
* **[Troubleshooting](troubleshooting.md)** is what `kac` prints when something is wrong, and what to do about each one.
* **[The corpus descriptor](corpus-descriptor.md)** covers `.corpus.yaml`, the one file a corpus writes for itself.
* **[Design](design/index.md)** is why `kac` works the way it does: where a check comes from, what a rule expression may
  say, what the schema itself is held to, and the contract an export answers to.

## What lives elsewhere

Two documents sit outside this site, each written for a different reader. A corpus also receives its own
[`knowledge-as-code.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/template/knowledge-as-code.md), which
is its way in to everything here.

**How to change the tool** is
[`tooling/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/README.md), beside the code. It
covers building `kac`, its three test layers, and what bites you while editing any of it.

## Maturity

**Early.** The tool is real and tested. The version on
[nuget.org](https://www.nuget.org/packages/KnowledgeAsCode.Tool) sits below `1.0.0` because the commands may still
change shape.

This site is built from `main`, so it can describe a `kac` no release carries yet. Ask your own copy what it is with
`kac --version`, and read the [changelog](changelog.md) for what each published version brought. Where the two disagree,
the site is ahead.

Every page here describes what exists today. The
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) holds what has been considered and not yet built.
