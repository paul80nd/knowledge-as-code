---
name: technical-writing
description: The writing floor for every word in this repository. Records, README files, code comments, schema descriptions, documentation, commit messages and pull request descriptions all answer to it. Load it before writing or editing any prose, and before writing a commit message or a pull request body.
---

# Technical writing

The floor every writer in this repository stands on. Whatever sent you here adds the rules for your audience on top.

**These rules reach prose, and stop where prose stops.** They do not reach a heading you may not rename, an identifier,
a clause whose wording is the record of an obligation, a fixed form such as an ADR's decision sentence, or text that
reproduces what a program prints. Where a rule cannot be obeyed without changing one of those, leave it and say so.

**A comment inside a fenced code block is prose and follows these rules.** The command, its flags and its
output are not.

**Never trade clarity for word count.** Where two rules pull against each other, the one that leaves the reader fewer
readings wins.

Examples read `Not: ... Write: ...`. The second is the one to copy.

## Build the sentence

* **Say who does what.** Put a person or a system in the subject. Use the active voice.
  Not: "Attribution is what makes this enforceable."
  Write: "We cannot hold anyone to a rule if we cannot tell who acted."
* **Do not invent an actor.** Where the text makes no claim about who acts, adding one adds a claim.
  Not: "Released under the MIT licence" becoming "We release this under the MIT licence."
  Write: "Released under the MIT licence."
* **Keep the passive where the actor is known and naming it here would be wrong.** A CDN serves an image that another
  service resized. "The CDN resizes it" is active, and false.
* **Use the passive only when the actor is unknown or beside the point.** Never to soften an obligation.
* **Write in the present tense.** Use "will" only for something that genuinely happens later.
  Not: "The validator will report a missing id." Write: "The validator reports a missing id."
* **Write an instruction as a command.**
  Not: "The packet should be removed from the box." Write: "Remove the packet from the box."
* **Write "we" for us and "you" for the reader.** A commitment says "we". Anything addressed to somebody says "you".
* **Put the common case first** and the exception after it.
* **Make a heading carry the point, in sentence case.** Not: "Modes". Write: "Pick the mode first".

## Carry one thing at a time

* **One idea per sentence.** A sentence carrying an obligation and its justification is two sentences.
* **One instruction per sentence.** Never two steps joined by "and".
* **Keep an instruction under about 20 words and other prose under about 25.** Fix the subject before you shorten.
* **Put the condition before the step it guards.**
  Not: "Click Delete to remove the document." Write: "To delete the document, click Delete."
* **Keep the articles.** Not: "Remove backup file." Write: "Remove the backup file."
* **Keep a paragraph under about six sentences.**
* **Vary the length on purpose.** A short sentence lands a point. A longer one carries a fact with its condition. One
  thought per sentence does not mean one length per sentence.

## Leave one reading, not two

* **Put "only" and "not" against the word they govern.**
  "Only the owner can merge a policy" and "The owner can merge only a policy" say different things.
* **Give every "it", "they" and "this" one obvious thing to point at.** Repeat the noun when in doubt.
* **Never point "this", "that" or "which" at a whole clause.** Where the repair needs a manufactured subject, recast the
  sentence instead of bolting one on.
  Not: "...drifted from the catalogue, which is what keeps that table honest."
  Write: "...drifted from the catalogue. That exit is what keeps the table honest."
* **Do not drop a verb across a clause boundary.**
  Not: "Phase 1 moves the converters and Phase 2 the runtime." Write: "...and Phase 2 moves the runtime."
* **Break a noun string longer than three words.**
  Not: "the proto import budget check script". Write: "the script that checks the proto-import budget".
* **Say which parts an "and" or an "or" joins** when the sentence can group two ways. "Both...and" and "either...or"
  cost nothing.

## Punctuation

* **An em dash has to earn its place, and usually cannot.** It separates without saying how the two parts relate, so
  the reader does that work. Use the mark that states the relation: a full stop for a new point, a colon for a reason,
  a list or a definition, a comma for a short aside. Often the sentence reads best with the mark simply gone. Where the
  sentence already carries a colon, take the full stop rather than a second one. Parentheses are a last resort, for an
  aside a comma would garden-path.
  Not: "**Repository** — `thumbnailer`." Write: "**Repository**: `thumbnailer`."
* **A bold label takes a colon before a value and a full stop before sentences.**
  Write: "**Platform**: CDN custom domain." Write: "**Caching.** The edge ignores every query string but two."
  Where a third part qualifies the value, a comma carries it: "**Repository**: `infrastructure`, at
  `services/covers`."
* **One em dash survives: the interrupting aside that carries its own commas.** *A definite reference — the same, none,
  that, it — needs its antecedent in the sentence before.* Parentheses would hold it too, and they tell a reader to
  skip it, which is wrong where the aside is load-bearing. That is the whole of the exception. **A dash at the end of a
  sentence interrupts nothing** and takes a colon or a full stop.
* **An en dash is for a range or a pair.** `A.7.1–A.7.14`, `35–45 words`, `client–server`. A hyphen there is a
  different mark meaning a different thing.
* **A colon introduces a list, an example, a reason or a definition.** Never as a mid-sentence connector.
  Not: "If you are coming from automation: you describe conditions." Write: "Coming from automation, you describe
  conditions."
* **Use a full stop, not a semicolon.**
* **Write the alternative out.** Not: "and/or", "read/write". Write: "a, b, or both".
* **Make anything in parentheses a whole grammatical unit.** Never form a plural with "(s)".
* **Use straight quotes.**

## Use the real words

* **Write the real symbol, path, flag or command name.** The codebase is the word list. Never a synonym or a
  description of it.
* **A figure of speech earns its place when it says something the plain word does not.**
  Keep: "a broken cross-reference fails CI rather than rotting quietly". *Rotting* says silent **and** gradual.
  *Unnoticed* says only silent.
  Cut the figure that only dresses a plain word: *substrate* for base, *vector* for way, *wedge in* for add, *north
  star* for goal, *flywheel* for what builds on itself.
* **A word that is the real name of a thing here is not a metaphor.** `Harness.cs` is a filename.
* **Use the short everyday word.** "use" not "utilize", "help" not "facilitate", "do" not "perform". A long word has to
  buy its length with precision.
* **Call one thing by one name, everywhere.** Two names read as two things. Elegant variation is a defect here.
* **Gloss a precise term on first use.** "idempotent", "failure domain" and "trust boundary" earn their place once
  glossed.

## Cut

* **Cut any word the sentence survives without.** "in order to" is "to". "It is important to note that" is nothing.
  This is for words that add nothing, never for words that disambiguate.
* **Cut these on sight:** simply, of course, seamless, robust, comprehensive, leverage, delve into, a tapestry of,
  pivotal, cutting-edge, serves as, it is worth mentioning, I hope this helps.
* **Cut the hedge stack.** Not: "could potentially be argued that it might". Write: "may".
* **Replace an adverb with a stronger verb or a number.** Not: "runs quickly". Write: "is fast", or the measurement.
* **Say what you found.** Not: "a number of issues". Write: "three broken links".
* **Cut a sentence that could appear unchanged in another project's documentation.** It says nothing about this one.
  Not: "schema changes can cause issues." Write: "a column rename fails the build."

## Watch the shape

* **Build a sentence on a contrast only where the reader would otherwise take the wrong reading.**
  Keep: "Copy `template/`, not `example/`." A reader would reach for the wrong folder.
  Cut: "The tool validates rather than generates." Say what it does.
* **Do not force ideas into groups of three** where two would do or four would be honest.
* **Do not manufacture a punchline for every section.** Most sections end on a sentence that states a fact.
* **A bold lead-in is structure. A bold claim is decoration.** A bold label ending in a colon that restates the line is
  a tell. A bold lead-in that ends in a full stop, names the item, and is followed by new detail is right, and where it
  does a heading's job it stays.
  Not: "**Performance:** Performance improved." Write: "**Schema in TypeScript.** Tables live in one file."
* **Bold the term, not the claim,** inside a paragraph. Where the claim is the misreading the paragraph exists to
  prevent, put that sentence first and let position carry it.
* **No emoji, no title-case headings.**
* **Use a numbered list only when the order carries meaning.** Introduce a list with a sentence saying what the items
  are, and keep the items parallel. Where the items speak for themselves, no introduction is needed. Never write a stem
  whose only job is to say a list follows.

## Write what stays true

* **Describe what is, not what changed**, and never as a correction of what was. Change history belongs to the commit
  message and the changelog, which are the two documents whose subject is change.
* **Do not write a count you cannot regenerate.** Name the command that reports it. A set fixed by decision rather than
  by accumulation may be counted.
* **Cite rather than restate.** If the reasoning lives elsewhere, link it. Nobody updates a copy.

## Commit messages and pull requests

* **The subject line says what changed.** Imperative, no full stop.
* **The body says why.** This is the one place where describing what used to be true is correct.
* **A pull request body carries the why and the evidence**, not a retelling of the diff.
* **Review the change, not the person.** Ask rather than assert where you might be wrong, and say what would change
  your mind.

## Before you finish

1. Name the subject of every sentence. Where the answer is "nothing", rewrite it, unless naming one would add a claim.
2. Read the longest sentence aloud. If you run out of breath, it is two sentences.
3. Search the draft for the cut list above, and for "only", "this" and "it".
4. Read the last sentence of each section. If more than one in a row is a crafted line, flatten the weaker.
5. Name any rule you could not obey, and what stopped you.
