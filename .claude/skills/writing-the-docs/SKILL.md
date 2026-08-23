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

* **"Gloss a precise term on first use."** The floor's rule stands here, and keeping it is the one thing this voice
  does that both the others undo. `writing-a-record` drops it because the corpus has a glossary. `writing-in-the-tool`
  drops it because the reader maintains the tool. This reader has neither.
  The framework carries its own vocabulary in
  [`knowledge-as-code.md`](../../../example/glossary/knowledge-as-code.md): corpus, record, type, tier, layer,
  mechanism, drift, forked, synced, upstream and the rest. Gloss each on its first use **on every page**, because a
  reader arrives in the middle of the set rather than at the front of it.
  Write: "a corpus, meaning one repository of knowledge records."
* **"Write 'we' for us and 'you' for the reader."** Narrowed: a public page says **you**, and never **we**. No
  commitment is being made here and no organisation is making it. Name `kac`, or the framework, or the corpus, and let
  it act.
  Write: "`kac` builds each index from the records." Not: "We generate the indexes for you."
* **Nothing else.** Where the floor and this page appear to disagree anywhere below, the floor wins.

## Open on what the reader gets

**The opening paragraph says what the reader gets.** Two openings fail here, and both are tempting. A definition
answers a question nobody has asked yet. A problem statement, sitting directly under the project's name, reads as a
description of what the project hands you.
Write: "A structured, validated body of knowledge that people and AI sessions both read from and contribute to."
Not: "A corpus is one repository of knowledge documents."
Not: "Engineering knowledge is spread thin."

**Take the plain word in the opening, and introduce the term where the reader needs it.** The gloss rule says to define
a term on first use. It does not say to reach for the term first. A body of knowledge becomes a corpus a paragraph
later, once there is something to call by its name.

**A definition built on a contrast asks a reader to hold two ideas** when they have not got the first one yet.
Write: "`kac` reads a folder of Markdown records and holds each one to the schema its type declares."

**Three contrasts in a row is the shape to watch for.** It reads as rhythm and lands as nothing, and it is where this
project's prose goes wrong when it is trying to sound confident.
Cut: "An index is generated rather than maintained, a broken cross-reference fails CI rather than rotting quietly, and
an agent can be told where a thing goes instead of guessing."

[`README.md`](../../../README.md) at the repository root carries the register to match. It runs at about four contrasts
per thousand words, in sentences averaging fourteen. Count yours the same way and read what you find, as the floor
says. Measure prose alone: a code block and a table skew both numbers.

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
inventing. A rewrite drops a fact more easily than it drops a word, and the replacement reads just as fluently.
Write: "the corpus's own `.schema/` holds one file per type."
Not: "each type declares its fields and rules in a YAML file beside them."

## Leave the reader somewhere to go

**A page names what to read next.** One link, chosen for where the reader now is. A list of everything is the same as
no link at all.

**No page assumes another was read first**, unless it links that page in the sentence that needs it.
