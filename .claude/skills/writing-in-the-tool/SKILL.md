---
name: writing-in-the-tool
description: How to write inside `tooling/`. Covers C# comments, XML doc comments, test names, the feature documents, `tests/README.md` and the changelog. Load it after `technical-writing` whenever you write or change a comment, or any prose under `tooling/`.
---

# Writing in the tool

Load `technical-writing` first. Everything below either adds to it or says plainly which of its rules it overrides.

**The code says what it does. A comment says why it is that way.** A comment restating the line below it has spent a
reader's attention and given nothing back.

## What the floor does not reach here

Headings, bold labels, numbered lists and the "we" and "you" rule are for pages. They have no meaning in a comment, so
do not go looking for them.

The floor's label rule already covers a comment above a declaration. Read it there.

## What this overrides

* **"Write in the present tense. Keep 'will' for something that genuinely happens later."** A comment often describes
  something that does not exist yet, so "will" is ordinary here.
  Write: `// The plugin manifest as it will travel.`
* **"Gloss a precise term on first use."** A comment is addressed to whoever maintains this tool, so a term the trade
  knows needs no gloss. Write DOM, AST and idempotent plainly.
* **Nothing else.** Where the floor and this page appear to disagree anywhere below, the floor wins.

## Check the claim is still true

**A comment that misdescribes the code is worse than no comment, because it is believed.** Read the code under every
comment you touch, and fix what has drifted.

A sweep in #253 found sixteen of these. Two would have cost somebody real time: a field called unread that another
class refuses an export over, and a duplicate id said to replace the first where the code keeps it.

Every one of them was true when it was written.

## Keep it to a line where a line does

**Summarise in one line, or give the context the code cannot.** There is nothing in between worth the space.

**A comment pays a line where a page pays a word.** Its margin is 120 columns minus its indentation, so a repair that
costs a page three words costs a nested comment a whole line. Where two repairs are equally clear, take the shorter.
Clarity still outranks brevity, exactly as the floor says.

**No banners.** A title fenced in hyphens is decoration, and the fence usually wraps a shorter version of the comment
below it. A section heading inside a long file is not a banner and earns its place.

**No apologising.** Not `hacky`, `sorry`, `for now`. A constraint is worth naming and an apology for it is not.

## The words the tool prints

**A message opens lower case and closes with a full stop.** A reader meets it as the tail of the command they ran.
Write: `id 'adr-7' must start with 'adr-'.`

**The second sentence of a split message opens lower case too.** An interpolated path or id opening it keeps the case
of the value itself.
Write: `the index is stale. {path} changed after it was built.`

## Say it once

**Before writing a sentence you have written before, grep a phrase from it.** An explanation here tends to land in
three places at once: the comment, the feature document and the README.

* [`features/`](../../../tooling/features/) is the reference for what a command does.
* [`tests/README.md`](../../../tooling/tests/README.md) is the reference for what a scenario asserts.

Where the argument already sits in one of those, link it and stop.

**A sibling source file is a citation target too.** Name the file, or the method that holds the reasoning.
Write: `// Through the source generator rather than reflection. See Json.cs.`

**Let a test say what it proves, and nothing more.** Why the code is shaped that way belongs at the source. A test
repeating it gives a reader two places to keep in step and no reason to trust either.

## The changelog

**One entry per behaviour a reader can observe.** A refactor nobody can see from outside does not get an entry.

**Name the verb and the flag, not the class.** `kac export --json` is what a reader has; `Exporter.WriteJson` is not.
