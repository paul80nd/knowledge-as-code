---
name: writing-in-the-tool
description: The shape of prose inside `tooling/`. Covers C# comments, XML doc comments, test names, the feature documents, `tests/README.md`, the messages the tool prints, and the changelog. Load it after `technical-writing` whenever you write or change a comment or any prose under `tooling/`.
---

# Writing in the tool

Load `technical-writing` first. This page adds the shape of prose in the tool.

## What this page overrides in the floor

* **Present tense.** A comment often describes something that does not exist yet, so "will" is ordinary. Write:
  `// The plugin manifest as it will travel.`
* **Gloss a term on first use.** The reader maintains this tool. Write DOM, AST and idempotent plainly.
* **"we" and "you", headings and bold labels.** These are for pages. A comment has none of them.

## Comments

Write comments to John Ousterhout's *A Philosophy of Software Design*, chapters 12 to 16. You know it. The rules
below say where this repository differs.

* **Comment every constraint a maintainer could break by tidying the code.** Where deleting a line would still
  compile and still pass the tests, say why it is there. That is the comment most often missing, and the one that
  pays for itself.
* **State the constraint. Do not argue for it.** A comment that defends a choice against the alternatives is a
  design page in the wrong place. The reasoning goes under `docs/design/`, and the comment cites the path.
* **Keep a comment to one paragraph.** A longer one is a design page in the wrong place too. Move it and cite the
  path.
* **A comment at the same level as the code repeats it. Delete it.**
* **Describe the design as it stands.** A comment never says what the code used to do. The commit message holds
  that.
* **Read the code under every comment you touch.** Fix a comment that has drifted. A wrong comment is worse than
  none, because it is believed.
* **Use no banner and no `#region`.** A region collapses by default and hides structure. StyleCop bans it under
  `SA1124`. A file that wants section headings wants splitting.
* **Do not apologise.** Cut `hacky`, `sorry` and `for now`. Name the constraint instead.

## `///` and `//`

* `///` is read by a caller. The IDE shows it beside the call. Write what a caller must know to use the member
  correctly: what a null means, what it throws, an order two calls must keep, a cost. A member whose name and signature
  already say it needs none.
* `//` is read by a maintainer. Write the constraint the implementation answers to, inside a body or above a
  private member.
* Keep the two apart. A caller must not have to open the file to learn what a method promises.

## Messages the tool prints

* A message opens lower case and closes with a full stop. The reader meets it at the tail of the command they ran.
  Write: `id 'adr-7' must start with 'adr-'.`
* The second sentence of a split message opens lower case too. An interpolated path or id keeps its own case. Write:
  `the index is stale. {path} changed after it was built.`
* A message names a change to the corpus the reader holds. It does not name a template or a transform they never saw.
* Changing what a command prints makes its docs page stale. `docs/cli/<verb>.md` quotes real output. Re-read it,
  run the command, and paste what it printed now.

## Say it once

* Before you write a sentence you have written before, grep a phrase from it. An explanation lands in the comment, the
  feature document and the README at once.
* [`docs/`](../../../docs/) is the reference for what a command does.
  [`tests/README.md`](../../../tooling/tests/README.md) is the reference for what a scenario asserts. Where the
  explanation is already in one of those, link it and stop.
* Name the path. Do not summarise what it says. `CommentCitationTests` fails a `.md` path no file answers to, so a
  bare path breaks where you can see it. A summary goes stale where you cannot.
* A sibling source file is a citation too. Write: `// Through the source generator, not reflection. See Json.cs.`
* A test says what it proves. Why the code is shaped that way belongs at the source.

## The changelog

* Write one entry per behaviour a reader can observe. A refactor nobody can see from outside gets no entry.
* Name the verb and the flag, not the class. Write `kac export --json`. Not `Exporter.WriteJson`.
* Write the entry under `## Unreleased`. Moving the version is a separate decision.
