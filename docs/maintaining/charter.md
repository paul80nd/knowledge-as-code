# The documentation charter

This page says which reader each section of the site serves and which kinds of page each section may contain. It also
says where the argument behind a change is written. Check a page against it before you add, move or argue anything on
one. The writing rules in the repository cite this page for the readers and the page kinds, and do not restate either.

A corpus is one repository of knowledge records kept in git. A record is one Markdown document in it, and a type is one
kind of record.

## The readers

Four readers each own a section. The plugin is a corpus packaged for Claude Code. The export is the set of files a
corpus publishes for another corpus to consume.

| Reader               | What they want                                                              | Section           |
|----------------------|-----------------------------------------------------------------------------|-------------------|
| The `kac` user       | install the tool, run a corpus, wire CI, build and install the plugin       | Use it            |
| The framework reader | how the types are laid out, where they came from, and how to write a record | The framework     |
| The schema owner     | the keys, the expression grammar, the checks and the export contract        | Schema reference  |
| The maintainer       | how `kac` is built and released, and what it is specified to do             | Maintaining `kac` |

One more reader, the evaluator, owns no section. They are deciding whether to adopt `kac` at all. They read the root
`README.md` and the opening of [the introduction](../index.md). A page written for them beyond that sits in Use it.

## The page kinds

Each section may contain only the kinds of page in its row.

| Section           | Page kinds                                                                                                      |
|-------------------|-----------------------------------------------------------------------------------------------------------------|
| Use it            | A tutorial, a how-to, and reference for what a user types or writes, such as a command or the corpus descriptor |
| The framework     | Explanation, which may say why                                                                                  |
| Schema reference  | Reference for what a schema owner declares                                                                      |
| Maintaining `kac` | Specification, the maintainer's how-tos for building, releasing and forking `kac`, and this charter             |

The first four kinds are [Diátaxis](https://diataxis.fr/)'s. A specification page states what `kac` is built to do,
and each behaviour on it links the Reqnroll scenario in
[`tooling/kac.features`](https://github.com/paul80nd/knowledge-as-code/tree/main/tooling/kac.features) that proves it.

## Which page owns a behaviour

When two sections describe one behaviour, the reader-facing page owns the full statement. The specification page:

1. Lists the behaviour in one line.
2. Links the scenario that proves it.
3. Links the page that owns it.

## Where an argument goes

An argument is the reasoning behind a choice: why it was made, and why not the alternative.

* A change of behaviour updates its specification page and the feature that proves it.
* The argument behind the change goes in the commit message and the pull request.
* A choice a contributor would plausibly propose reversing earns one "Why not" paragraph on its specification page.
* Use it and Schema reference argue nothing.

The commit keeps the argument beside the change it explains, where a reader of the site never has to read past it.

## What a comment or a `notes:` contains

A comment in the C# source, or a `notes:` in a `.schema/` file, states its constraint and stops. The argument goes in
the commit message. Either may cite the page that owns the behaviour.

| Written in               | May cite                                                      |
|--------------------------|---------------------------------------------------------------|
| A C# comment             | A specification page in Maintaining `kac`                     |
| A `notes:` in `.schema/` | A Schema reference page, or a type's section in The framework |

## Who applies it

No test checks a page against this charter. Which reader a page serves is a judgement, so the writing rules and review
apply it.

## Where to go next

[`writing-the-docs`](https://github.com/paul80nd/knowledge-as-code/blob/main/.claude/skills/writing-the-docs/SKILL.md)
gives the shape of a command page and the voice every page here uses.
