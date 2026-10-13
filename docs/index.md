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

## Where to start

This site has one section for each reader. A type is one kind of record, such as a runbook. Its schema is a YAML file
under `.schema/` that says what a record of that type has in it.

* **I want to use `kac`.** [Use it](use/index.md) shows you how to install the tool, run it over a corpus, wire it into
  CI and build the plugin. [Getting started](use/getting-started.md) is its first page.
* **I want to understand the framework.** [The framework](framework/index.md) explains how the types are laid out,
  where they came from, and how to write a record.
* **I own a schema.** [Schema reference](reference/index.md) covers the keys a type file takes, the rule expression
  grammar, the checks and the export contract.
* **I want to work on `kac`.** [Maintaining `kac`](maintaining/index.md) explains how to add a check in C# or a report,
  and how the plugin bundle is laid out.

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
