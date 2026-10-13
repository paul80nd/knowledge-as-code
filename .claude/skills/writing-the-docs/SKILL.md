---
name: writing-the-docs
description: The shape of the public documentation. Covers the root `README.md`, `PACKAGE.md` as nuget.org renders it, and the site under `docs/`. Load it after `technical-writing` whenever you write or change a page for somebody who has not installed the tool.
---

# Writing the docs

Load `technical-writing` first. This page adds the shape of the docs.

## The reader

They have installed nothing. They have no corpus and nobody to ask. They arrived from a package page, a search result
or the repository. They decide in the first paragraph whether to keep reading.

## Match the model page

`docs/use/discovery.md` is the model for voice and headings. Where a page and the model differ in either, change
the page. It is not the model for length. Keep each section to what its reader needs.

## What this page overrides in the floor

* **"we".** Never, on a public page. Say "you", or name `kac`, the framework or the corpus. Write: "`kac` builds each
  index from the records." Not: "We generate the indexes for you."
* **Gloss a framework term on first use.** On every page, not once per site. A reader arrives from search in the
  middle of the set. The vocabulary is in
  [`knowledge-as-code.md`](../../../examples/library/glossary/knowledge-as-code.md).

## The first paragraph

* Say what the thing is, what it does and where it lives. Write: "`kac` reads a folder of Markdown records and checks
  each one against the schema its type declares."
* Do not open on a definition. Do not open on a problem statement.
* Show a command before the explanation of why it exists.
* A command runs on a clean machine, or the line above it names what must be installed first.
* A comment beside a command says what the command does. Write: `kac validate     # frontmatter, links, structure`

## Which section holds a page

The site has four sections, one for each reader:

* Use it
* The framework
* Schema reference
* Maintaining `kac`

[The documentation charter](../../../docs/maintaining/charter.md) says which reader each section serves, which kinds of
page it may contain, and where an argument goes. Read it before you add a page, move one or argue anything on one.

* Maintaining `kac` covers building, testing, releasing and forking `kac`. `tooling/README.md` keeps what a reader
  needs only while changing the code, and links back to those pages.
* A command page says what the command does. It argues nothing.
* A second page is earned by a second reader. `use/cli/checks.md` is for somebody running the command.
  `reference/checks.md` is for somebody declaring a check, and `maintaining/checks.md` for somebody writing one in C#.
  One reader who wants more detail gets a deeper heading, not a page.
* `README.md` and `docs/index.md` open on the same four paragraphs, byte for byte. Edit one and copy the change. Nothing
  in CI checks it.

## The command page

The page has fixed sections in a fixed order.
[`tooling/README.md`](../../../tooling/README.md#the-documentation-site) is the reference for the set.

* The usage block at the top is generated. Do not edit it.
* The H1 is the verb and what running it does, with no colon: ``# `checks` list every check the validator can report``.
  `CliReference.cs` parses that form to build the overview table, and `CliReferenceTests` fails any other.
* **What it does** opens on the fact. The first sentence says what the command reads and what it writes.
* **Examples** sit at `###`, one per example, each with the output the command printed. Take the output from a run.
* **Known limits** states what the command does not do, and the exit code for each refusal.
* Put the rule before the reason in every section.
* An example heading is a noun phrase naming the case: "A first restore", "A refusal".

## Headings and sections

* A heading names its topic: the command, the key, the file, the check. A noun phrase or a question that names the
  topic passes: "What discovery excludes", "`from: sub-path`". A claim fails: "The fallback walk honours nothing".
  Test it on its own. A reader who has read nothing else on the page can tell whether their answer is under it.
* Keep a section under about 120 words, unless it is a list or a table.
* Use a numbered list for steps, a table for values, and bullets for parallel items.
* Write a list of three or more things as a list, not as a sentence.
* Renaming a heading breaks every link to its anchor. Search the whole repository before you rename one.

## Facts

* The reader can check nothing. Read the code, the schema or the folder under every claim, including one you are
  rewording.
* A count, or a claim that something is not yet built, needs something that fails when it changes. In order: let the
  generator write it, let a test hold it (`DocumentationCitationTests`, `DefaultTypesTests`), or name the command that
  prints it (`kac checks`, `kac --version`). Where none of the three applies, leave the claim out.
* Write a count once. Cite it from everywhere else.

## Links

* Inside `docs/`, link by relative path to the `.md` file. `mkdocs build --strict` fails a dead link. `NavigationTests`
  fails a page the nav does not list.
* From a file under `tooling/`, cite a page by path, as `docs/use/cli/export.md`.
* From the overlay layer (`.schema/`, `knowledge-as-code/`, `.plugin/`), cite the site by URL. Those files reach every
  corpus and `docs/` reaches none.
* End each page with one link to read next, chosen for where the reader now is.

## Admonitions

* `!!! warning` marks a command that is not built yet.
* `!!! note` marks a step that must happen before the next command block.
* Use neither for emphasis.
