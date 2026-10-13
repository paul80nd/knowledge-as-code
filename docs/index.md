# knowledge-as-code

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

## A type declares its own schema

A **type** is a category such as a policy, a runbook or a glossary. Its **schema** is the machine-readable statement of
what a record of that type has in it. The schema travels inside the corpus, in `.schema/`, so adding a type is adding a
YAML file rather than changing the tool.

## What this site is

The reference for KaC: what a corpus holds, and what `kac` does to one.

* **[Getting started](getting-started.md)** installs the tool and runs it against a corpus.
* **[The framework](framework/index.md)** is the ideas the tool serves: what a type and a tier are, the types that ship,
  what a record has in it, and how knowledge is contributed.
* **[Running it in CI](ci.md)** wires the two commands that answer for a corpus into a pull request, on GitHub Actions
  or Azure Pipelines.
* **[CLI reference](cli/index.md)** gives a page to each command, saying what it does, what it refuses and what it
  leaves alone. The overview charts the order the commands run in, and lists the exit codes and the options every
  command takes.
* **[Troubleshooting](troubleshooting.md)** is what `kac` prints when something is wrong, and what to do about it.
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

This site is built from `main`, so it can describe a `kac` no release includes yet. Ask your own copy what it is with
`kac --version`, and read the [changelog](changelog.md) for what each published version brought. Where the two disagree,
the site is ahead.

Every page here describes what exists today. The
[issue tracker](https://github.com/paul80nd/knowledge-as-code/issues) says what has been considered and not yet built.
