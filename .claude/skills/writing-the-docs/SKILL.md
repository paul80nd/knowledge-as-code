---
name: writing-the-docs
description: How to write the public face of this project. Covers the root `README.md`, `PACKAGE.md` as nuget.org renders it, and the documentation site. Load it after `technical-writing` whenever you write or change a page addressed to somebody who does not yet use this.
---

# Writing the docs

Load `technical-writing` first. Everything below either adds to it or says plainly which of its rules it overrides.

**The reader has no corpus, no checkout and nobody to ask.** They arrived from a package page, a search or the
repository, and they have installed nothing. Everything they need is on the page or one link from it.

**They decide in a paragraph whether to keep reading.** That paragraph says what the thing is, in words they already
have.

## What this overrides

* **"Gloss a precise term on first use."** The floor's rule stands here, and keeping it is the one thing this voice does
  that both the others undo. `writing-a-record` drops it because the corpus has a glossary. `writing-in-the-tool`
  drops it because the reader maintains the tool. This reader has neither. The framework carries its own vocabulary in
  [`knowledge-as-code.md`](../../../examples/library/glossary/knowledge-as-code.md): corpus, record, type, tier, layer,
  mechanism, drift, forked, synced, upstream and the rest. Gloss each on its first use **on every page**, because a
  reader arrives in the middle of the set rather than at the front of it. Write: "a corpus, meaning one repository of
  knowledge records."
* **"Write 'we' for us and 'you' for the reader."** Narrowed: a public page says **you**, and never **we**. No
  commitment is being made here and no organisation is making it. Name `kac`, or the framework, or the corpus, and let
  it act. Write: "`kac` builds each index from the records." Not: "We generate the indexes for you."
* **Nothing else.** Where the floor and this page appear to disagree anywhere below, the floor wins.

## Open on what the reader gets

**The opening paragraph says what the reader gets.** Two openings fail here, and both are tempting. A definition answers
a question nobody has asked yet. A problem statement, sitting directly under the project's name, reads as a description
of what the project hands you. Not: "A corpus is one repository of knowledge documents."
Not: "Engineering knowledge is spread thin."

**Say enough for a reader to place the thing.** Somebody who cannot tell whether this is a service, a library or a
command they run has not been told what they get. An opening that summarises is fine where the sentence after it lands
the nouns: the format, the tool's name, where the thing lives. Write: "`kac` reads a folder of Markdown records and
holds each one to the schema its type declares."

**Take the plain word in the opening, and introduce the term where the reader needs it.** The gloss rule says to define
a term on first use. It does not say to reach for the term first. A body of knowledge becomes a corpus a paragraph
later, once there is something to call by its name.

**A definition built on a contrast asks a reader to hold two ideas** when they have not got the first one yet, and three
contrasts in a row reads as rhythm and lands as nothing. That is where this project's prose goes wrong when it is trying
to sound confident. Cut: "An index is generated rather than maintained, a broken cross-reference fails CI rather than
rotting quietly, and an agent can be told where a thing goes instead of guessing."

**No page here is the model.** Where a page and this one disagree, the page is what changes, unless somebody decided
otherwise on purpose. The register is a pair of numbers rather than a file to copy: under four contrasts per thousand
words, in sentences averaging fourteen. Count yours the same way and read what you find, as the floor says. Measure
prose alone, because a code block and a table skew both numbers.

## Get the reader to a command

**A command appears before the explanation of why it exists.** Somebody deciding whether to install this wants to see
what running it looks like.

**A command works on a clean machine**, or the line above it names what has to be there first.

**A comment beside a command says what that command does.**
Write: `kac validate     # frontmatter, links, structure, clauses and the graph`

**A flag the reader meets while running the tool is documented where they meet it.** `--help` and the reference pages
carry that. A page somebody reads before installing carries what decides them, and nothing they can look up later.

## Check the claim before you write it

**A public page states facts about the tool, and the reader can check none of them.** They have no checkout. A sentence
that sounds right and is wrong costs them the hour they spend acting on it.

Read the code, the schema or the folder under every factual claim, including one you are rewording rather than
inventing. A rewrite drops a fact more easily than it drops a word, and the replacement reads just as fluently. Write:
"the corpus's own `.schema/` holds one file per type."
Not: "each type declares its fields and rules in a YAML file beside them."

## Choose the page by its reader

`README.md` and `PACKAGE.md` are one page each, so a fact either belongs there or it does not. The site adds the
question neither of them asks: which of its pages carries this one.

**Diátaxis is the map, and this site departs from it in one place.** Diátaxis splits documentation four ways, into a
tutorial, a how-to, a reference and an explanation, and puts each in its own page. Here the reasoning stays inline,
beside whatever it explains. A command page argues its case under *How it works*, and `--check`'s reason sits under
`--check`.

**A second page is earned by a second reader.** [`cli/checks.md`](../../../docs/cli/checks.md) is for somebody running
the command. [`checks.md`](../../../docs/checks.md) is for somebody adding one. Two readers, so two pages. One reader
who wants more detail is a `####` under the heading they are already on.

**Where two pages share a subject, neither argues the other's case.** The command page says what running the command
does and cites the other for why. A claim written on both goes out of step on the day one is edited.

## Put each fact where its reader meets it

| The reader wants                                       | The page                                               |
|--------------------------------------------------------|--------------------------------------------------------|
| to find the flags a command takes                      | `kac <command> --help`, and the page's generated block |
| to learn what a command does, refuses and leaves alone | `docs/cli/<verb>.md`                                   |
| to learn why it works the way it does                  | the concept page, or that command's *Decisions*        |
| to run it once, having installed nothing               | `docs/getting-started.md`                              |
| to wire it into a pipeline                             | `docs/ci.md`                                           |
| to decide whether to install it at all                 | `README.md` and `docs/index.md`                        |
| to change `kac` itself                                 | `tooling/README.md`, which the site links out to       |

**`README.md` and `docs/index.md` open on the same four paragraphs, byte-identical.** Both are read by somebody who has
installed nothing. Edit one and copy the change across, because nothing in CI holds the two together.

## Write a page somebody lands in the middle of

**A reader arrives at a heading from search**, rather than at the top of the page. So a heading says what is under it,
and a section assumes nothing from the section above it.

**Count the words between headings.** Most pages here sit near 80 words. Past about 120 a reader has to read a section
to find out whether it answers them, and `export` at 173 and `bundle` at 143 are the two that have not yet had that cut.
The count is a prompt to look, never the diagnosis.

**A command page carries five fixed sections, and grows deeper rather than wider.**
[`tooling/README.md`](../../../tooling/README.md#the-documentation-site) is the reference for that set and for the
generated usage block above it. A command with two halves keeps each half at `###` and steps it at `####`.

**A command page opens on its verb and what running it does, with no colon between them.** `CliReference.cs` parses that
heading to build the overview's table, so a form it cannot read fails `CliReferenceTests`. Write:
``# `checks` list every check the validator can report``

**An admonition carries what a reader would otherwise act on wrongly.** Two kinds are in use: a `!!! warning` for a page
describing a command that is not built yet, and a `!!! note` for a step that has to happen before the next command
block. It is not a way to emphasise a paragraph.

## Cite the site by URL, and `docs/` by path

**A file under `tooling/` cites a page by path**, as `docs/cli/export.md`. The two folders travel together in a fork of
the whole framework, and a link to this site would send that fork's reader back upstream.

**A file in the overlay layer cites the site by URL.** `.schema/`, `knowledge-as-code/` and `.plugin/` reach every
corpus word for word, and `docs/` reaches none of them, so a path written there resolves to nothing.
[`manifest.yaml`](../../../manifest.yaml) says which files this covers.

**A link inside `docs/` is a relative path to the `.md` file**, and `mkdocs build --strict` fails a dead one. A page the
nav does not list is a separate fault, caught by `NavigationTests`, because the build reports it at INFO and exits `0`.

## Leave the reader somewhere to go

**A page names what to read next.** One link, chosen for where the reader now is. A list of everything is the same as no
link at all. Write: "[Checks](../checks.md) is the page for adding a check, or for deciding whether the one you want
already exists."
Not: "[Checks](../checks.md) says where a check comes from, what the schema pass refuses, and why a rule is data."

**No page assumes another was read first**, unless it links that page in the sentence that needs it.
