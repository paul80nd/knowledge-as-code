---
name: technical-writing
description: The writing floor for every word in this repository. Records, README files, code comments, schema descriptions, documentation pages, commit messages and pull request descriptions all follow it. Load it before you write or edit any prose.
---

# Technical writing

Write in the style of the Microsoft Writing Style Guide. You know it. Apply it to every sentence here. The rules below
say what that means in practice, and where this repository differs from Microsoft.

## Five habits

These do most of the work.

* **Lead with the point.** State the rule or the fact first. Put the reason after it, or leave the reason out.
* **Use the plain verb.** A command *lists*, *checks*, *writes*, *reports*, *creates*, *rejects*. It does not *hold
  to*, *carry*, *name*, *answer*, *reach*, *stand up* or *seal*. "holds" has four plain replacements: *has*,
  *contains*, *keeps*, *checks*. "carries" has four more: *has*, *contains*, *includes*, *states*. Pick by sense.
  The ban is on one of these standing in for a plain verb, not on the word. *Name* a thing, *cite* a record, and let a
  service *return* what a caller asked for.
* **Talk to the reader.** Say "you". Write an instruction as a command. Use the active voice and the present tense.
* **One idea per sentence.** Keep a sentence under about 20 words. A sentence joining two steps with "and" is two
  sentences.
* **Make it scannable.** Keep paragraphs short. Use a list for parallel items. Use a table when three or more things
  each have the same two or three facts: a key and its meaning, an option and its effect, a value and what it selects.
  A heading names its topic, so a reader arriving from search knows what is under it.

## Where this repository differs from Microsoft

Microsoft writes US English about Microsoft products. Apply these changes.

* **Use British spelling.** licence, colour, organisation, behaviour.
* **Use no em dash.** Use a full stop, a comma or a colon.
* **Wrap prose at 120 columns.** A table row and a link definition do not wrap.
* **Say "we" only in a commitment**, in a policy or a standard. Everywhere else say "you", or name `kac`, the framework
  or the corpus.
* **Gloss a framework term the first time it appears on a page.** The terms are corpus, record, type, tier, layer,
  export and plugin. Write: "a corpus (one repository of knowledge records)".
* **Use no figure of speech.** Microsoft allows a light touch. Here, none. Write "fails CI". Not "rots quietly".
* **Give a claim one reason, then stop.** A second clause explaining the first adds nothing.
* **Name one thing one way.** Two names read as two things. Repeat the word.

## What you must not change

* An identifier, a path, a flag, a command, or the output a command printed.
* A heading you may not rename, and the H1 of a record.
* A clause id, and any word of a clause that carries the obligation. `writing-a-record` states the test.
* Anything between a `BEGIN GENERATED` marker and its `END GENERATED`. Change the source and run `kac generate`.

Say in your reply which of these you left alone, and where.

## Before and after

| Write                                                                               | Not                                                                    |
|-------------------------------------------------------------------------------------|------------------------------------------------------------------------|
| `validate` reports a missing marker.                                                | `validate` holds a corpus to carrying the markers this writes between. |
| Only an error fails the build.                                                      | Failing rather than warning is the whole of the trade.                 |
| `pack` zips `.dist/export/` into a `.nupkg` under `.dist/package/`.                 | A directory is not something another repository can depend on.         |
| Two separators extend an id. `.` selects a part. `:` selects a corpus.              | Two separators reach past an id, each with one job.                    |
| If you adopt only some types, `new` removes links to the types you skipped.         | A corpus adopting a subset is sent nothing it cannot follow.           |
| `update` fetches `ref`. It writes `commit` for your information and never reads it. | `ref` is followed and `commit` is never read back.                     |

## Rewrite the block

When you add a fact to existing prose, rewrite the whole paragraph with the fact in it. A paragraph that only gained a
sentence was appended to, and reads that way. If the fact belongs in a list the paragraph already has, add it to the
list.

## Commit messages and pull requests

* The subject line says what changed. Imperative, no full stop.
* The body says why. This is the one place to describe what used to be true.
* A pull request body carries the reason and the evidence. It does not retell the diff.

## Before you finish

1. Search for "holds", "carries", "names", "answers", "reaches", "seals". Replace each one standing in for a plain
   verb. Leave the ones that are the plain verb.
2. Search for "rather than" and "instead of". Keep the contrast only where the reader already holds the wrong idea.
   Delete it everywhere else. Inside a clause it stays: it names what the clause forbids, so cutting it drops an
   obligation.
3. Search for "the two", "the three", "the four". Name the things, unless the sentence already named them.
4. Read each heading on its own. It names a topic.
5. Read the first sentence of each section. It is the rule, not the reason.
6. Read the longest sentence aloud. If you run out of breath, split it.
