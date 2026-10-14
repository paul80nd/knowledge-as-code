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

* **Comment a constraint a maintainer could break by tidying the code.** Where deleting a line would still compile
  and still pass the tests, say what the line protects. One or two lines is enough.
* **State the constraint and stop.** [The documentation
  charter](../../../docs/maintaining/charter.md#what-a-comment-or-a-notes-contains) says what a comment may cite, and
  the argument goes in the commit message. A "because" or a "rather than" in a comment is usually the argument
  starting. Cut the clause.
* **Keep a comment to one paragraph of three lines.** `CommentLengthTests` fails a `//` block that runs longer. Where
  the reason needs more, it is an argument. Put it in the commit message.
* **A comment at the same level as the code repeats it. Delete it.**
* **Describe the design as it stands.** A comment never says what the code used to do. The commit message holds
  that.
* **Use no banner and no `#region`.** A region collapses by default and hides structure. StyleCop bans it under
  `SA1124`. A file that wants section headings wants splitting.
* **Do not apologise.** Cut `hacky`, `sorry` and `for now`. Name the constraint instead.

## `///` and `//`

Follow Microsoft's [comment style] for C# and its [recommended XML tags]. The rules below are what they mean here.

* **A comment on a type or a member is an XML doc comment, `///`, with a `<summary>`.** That includes a private member.
  The IDE shows it beside every call. Write what a caller must know: what a null means, what it throws, an order two
  calls must keep, a cost. A member whose name and signature already say it needs none.
* **Add `<param>`, `<returns>` or `<exception>` only where it says something the summary does not.**
* **A doc comment is XML.** Write code as `<c>`, and escape `<` as `&lt;` and `&` as `&amp;`.
* **`//` belongs inside a body**, on the line above the statement it explains.
* **Put a comment on a line of its own.** A label on an `[InlineData]` row is the one exception, because it annotates
  one case of a table.
* **Write a comment as a sentence.** Open it with a capital letter and close it with a full stop.
* `CommentPlacementTests` fails a `//` on a member, a comment trailing code, and a doc comment that is not well-formed
  XML.

[comment style]: https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions#comment-style
[recommended XML tags]: https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags

## Messages the tool prints

* A message opens lower case and closes with a full stop. The reader meets it at the tail of the command they ran.
  Write: `id 'adr-7' must start with 'adr-'.`
* Every sentence after the first opens with a capital. Only the first meets the tail of the command. Write:
  ``this fix's `review-by` has passed. Check the steps still clear the symptom.``
* An interpolated path or id keeps its own case wherever it falls.
* A message names a change to the corpus the reader holds. It does not name a template or a transform they never saw.
* Changing what a command prints makes its docs page stale. `docs/use/cli/<verb>.md` quotes real output. Re-read
  it, run the command, and paste what it printed now.

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
* [What a pull request owes](../../../docs/maintaining/contributing.md#what-a-pull-request-owes) says where the entry
  goes, and that `<Version>` stays as it is.
