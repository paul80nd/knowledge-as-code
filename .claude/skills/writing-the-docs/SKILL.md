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

`docs/design/discovery.md` is the model. Match its voice, its section length and its headings. Where a page and the
model differ, change the page.

<!-- Replace the model with the first page rewritten under the new floor, once one exists. -->

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

## Which page holds a fact

The site follows Diátaxis: a tutorial, a how-to, a reference and an explanation, each on its own page.

| The reader wants                                  | The page                                                |
|---------------------------------------------------|---------------------------------------------------------|
| to decide whether to install it at all            | `README.md` and `docs/index.md`                         |
| to run it once, having installed nothing          | `docs/getting-started.md`                               |
| to wire it into a pipeline                        | `docs/ci.md`                                            |
| to find the flags a command takes                 | `kac <command> --help`, and the page's generated block  |
| to learn what a command does and what it rejects  | `docs/cli/<verb>.md`                                    |
| to learn why it works the way it does             | `docs/design/`                                          |
| to learn what a type, a tier or a record is       | `docs/framework/`                                       |
| to change `kac` itself                            | `tooling/README.md`, which the site links out to        |

* A command page says what the command does. It links the design page for why. It does not argue the design.
* A second page is earned by a second reader. `cli/checks.md` is for somebody running the command. `design/checks.md`
  is for somebody adding a check. One reader who wants more detail gets a deeper heading, not a page.
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

## Headings and sections

* A heading names its topic: the command, the key, the file, the check. It is not a sentence and not a slogan. Test it
  on its own. A reader who has read nothing else on the page can tell whether their answer is under it.
* Keep a section under about 120 words, unless it is a list or a table.
* Use a numbered list for steps, a table for values, and bullets for parallel items.
* Write a list of three or more things as a list, not as a sentence.

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
* From a file under `tooling/`, cite a page by path, as `docs/cli/export.md`.
* From the overlay layer (`.schema/`, `knowledge-as-code/`, `.plugin/`), cite the site by URL. Those files reach every
  corpus and `docs/` reaches none.
* End each page with one link to read next, chosen for where the reader now is.

## Admonitions

* `!!! warning` marks a command that is not built yet.
* `!!! note` marks a step that must happen before the next command block.
* Use neither for emphasis.
