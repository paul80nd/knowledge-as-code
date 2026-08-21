---
name: technical-writing
description: The writing floor for every word in this repository. Records, README files, code comments, schema descriptions, documentation, commit messages and pull request descriptions all answer to it. Load it before writing or editing any prose, and before writing a commit message or a pull request body.
---

# Technical writing

The floor every writer in this repository stands on. Whatever sent you here adds the rules for your audience on top.
Nothing below has an exception clause. Where a rule makes one sentence worse, fix that sentence another way and say so
in review.

Examples below are written as `Not: ... Write: ...`. The second is the one to copy.

## Build the sentence

* **Say who does what.** Put a person or a system in the subject. Use the active voice.
  Not: "Attribution is what makes this enforceable."
  Write: "We cannot hold anyone to a rule if we cannot tell who acted."
* **Use the passive only when the actor is unknown or beside the point.** Never to soften an obligation.
* **Write in the present tense.** Use "will" only for something that genuinely happens later.
  Not: "The validator will report a missing id." Write: "The validator reports a missing id."
* **Write an instruction as a command.**
  Not: "The packet should be removed from the box." Write: "Remove the packet from the box."
* **Write "we" for us and "you" for the reader.** A commitment says "we". Anything addressed to somebody says "you".
* **Put the common case first** and the exception after it.
* **Make a heading carry the point, in sentence case.**
  Not: "Modes". Write: "Pick the mode first".
* **Do not pre-announce.** Cut "we will soon support", "more on this below", and any sentence whose only job is to
  introduce the next one.

## Carry one thing at a time

* **One idea per sentence.** A sentence carrying an obligation and its justification is two sentences.
* **One instruction per sentence.** Never two steps joined by "and".
* **Keep an instruction under about 20 words and other prose under about 25.** Fix the subject before you shorten.
* **Put the condition before the step it guards.**
  Not: "Click Delete to remove the document." Write: "To delete the document, click Delete."
* **Keep the articles.**
  Not: "Remove backup file." Write: "Remove the backup file."
* **Keep a paragraph under about six sentences.**
* **Vary the length on purpose.** A short sentence lands a point. A longer one carries a fact with its condition. One
  thought per sentence does not mean one length per sentence.

## Leave one reading, not two

* **Put "only" and "not" against the word they govern.**
  "Only the owner can merge a policy" and "The owner can merge only a policy" say different things.
* **Give every "it", "they" and "this" one obvious thing to point at.** Repeat the noun when in doubt. Never point
  "this" or "which" at a whole clause.
* **Do not drop a verb across a clause boundary.**
  Not: "Phase 1 moves the converters and Phase 2 the runtime." Write: "...and Phase 2 moves the runtime."
* **Break a noun string longer than three words.**
  Not: "the proto import budget check script". Write: "the script that checks the proto-import budget".
* **Use a full stop.** Not a semicolon, and not an em dash. Where an em dash feels right, start a new sentence.
* **Write the alternative out.**
  Not: "and/or", "read/write". Write: "a, b, or both".
* **Make anything in parentheses a whole grammatical unit.** Never form a plural with "(s)".
* **Say which parts an "and" or an "or" joins** when the sentence can group two ways. "Both...and" and "either...or"
  cost nothing.

## Use the real words

* **Write the real symbol, path, flag or command name.** The codebase is the word list. Never a synonym or a
  description of it.
* **Do not invent jargon.** Use the words a developer would say out loud.
  Not: "evacuate the records", "ratchet the budget". Write: "move the records", "lower the budget".
* **Use the short everyday word.** "use" not "utilize", "help" not "facilitate", "do" not "perform". A long word has to
  buy its length with precision.
* **Call one thing by one name, everywhere.** Two names read as two things. Elegant variation is a defect here.
* **Gloss a precise term on first use.** "idempotent", "failure domain" and "trust boundary" earn their place once
  glossed.
* **Skip idioms, metaphors and Latin abbreviations.** A translator, a non-native reader and an agent all parse plain
  constructions best.

## Cut

* **Cut any word the sentence survives without.** "in order to" is "to". "It is important to note that" is nothing.
* **Cut these on sight:** simply, of course, seamless, robust, comprehensive, leverage, delve into, a tapestry of,
  pivotal, cutting-edge, serves as, it is worth mentioning, I hope this helps.
* **Say what you found.**
  Not: "a number of issues", "several considerations". Write: "three broken links".
* **Be specific rather than sterile.**
  Not: "schema changes can cause issues." Write: "a column rename fails the build."

## Watch the shape

* **Do not build every sentence around a contrast.** "X, not Y" is for the place a reader would otherwise take the
  wrong reading. Let an ordinary declarative carry the rest.
* **Do not force ideas into groups of three** where two would do or four would be honest.
* **Do not manufacture a punchline for every section.** Most sections end on a sentence that states a fact.
* **Skip decorative formatting.** No emoji, no title-case headings, no bolding a whole sentence. Bold the term, not the
  claim.
* **Use a numbered list only when the order carries meaning.** Bullets otherwise. Introduce a list with a complete
  sentence and keep the items parallel.

## Write what stays true

* **Describe what is, not what changed**, and never as a correction of what was. Change history belongs to the commit
  message.
* **Do not write a count that goes stale.** Generate it, or name the command that reports it. A closed set may be
  counted, because it moves by decision rather than by accumulation.
* **Cite rather than restate.** If the reasoning lives elsewhere, link it. Nobody updates a copy.

## Commit messages and pull requests

* **The subject line says what changed.** Imperative, no full stop.
* **The body says why.** This is the one place where describing what used to be true is correct.
* **A pull request body carries the why and the evidence**, not a retelling of the diff.
* **Review the change, not the person.** Ask rather than assert where you might be wrong, and say what would change
  your mind.

## Before you finish

1. Name the subject of every sentence. Where the answer is "nothing", rewrite it.
2. Read the longest sentence aloud. If you run out of breath, it is two sentences.
3. Search the draft for the cut list above, and for "only", "this" and "it".
4. Read the last sentence of each section. If more than one in a row is a crafted line, flatten the weaker.
