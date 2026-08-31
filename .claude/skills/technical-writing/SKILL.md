---
name: technical-writing
description: The writing floor for every word in this repository. Records, README files, code comments, schema descriptions, documentation, commit messages and pull request descriptions all answer to it. Load it before writing or editing any prose, and before writing a commit message or a pull request body.
---

# Technical writing

Whatever sent you here adds the rules for your audience on top of these.

**These rules reach prose, and stop where prose stops.** They do not reach a heading you may not rename, an identifier,
a clause whose wording is the record of an obligation, a fixed form such as an ADR's decision sentence, or a transcript
quoting what a program printed. Leave any of those as they are, and say which rule you left behind.

**A comment inside a fenced code block is prose.** The command, its flags and its output are not.

**The words a program prints are prose.** A message in a string literal, or a schema `message:` value, is written for a
reader. A transcript quoting that message back is a fact.

**Clarity outranks brevity.** Where two rules pull against each other, the one leaving the reader fewer readings wins.
Where they tie, either answer is right: pick one and move on.

Examples read `Write: ... Not: ...`. The first is the one to copy.

## Build the sentence

* **Say who does what, where the text already claims who.** Put that person or system in the subject, in the active
  voice. Where the text claims nobody, keep the passive: the actor may be unknown, beside the point, or named somewhere
  this sentence should not name it. A CDN serves an image another service resized, so "the CDN resizes it" is active and
  false. A passive that softens an obligation is always wrong. Write: "We cannot hold anyone to a rule if we cannot tell
  who acted."
  Not: "Attribution is what makes this enforceable."
  Write: "Released under the MIT licence." Not: "We release this under the MIT licence."
* **Write in the present tense.** Keep "will" for something that genuinely happens later. Write: "The validator reports
  a missing id." Not: "The validator will report a missing id."
* **Write an instruction as a command.**
  Write: "Remove the packet from the box." Not: "The packet should be removed from the box."
* **Write "we" for us and "you" for the reader.** A commitment says "we". Anything addressed to somebody says "you".
* **Put the common case first** and the exception after it.
* **Make a heading carry the point, in sentence case.** Write: "Pick the mode first". Not: "Modes".

## Carry one thing at a time

* **One idea per sentence, and one instruction.** A sentence carrying an obligation and its justification is two
  sentences. Two steps joined by "and" are two sentences.
* **Keep an instruction under about 20 words and other prose under about 25.** An interrupting aside does not count
  toward the length. A 30-word sentence holding a 9-word aside is a 21-word sentence, and stays whole.
* **Put the condition before the step it guards.**
  Write: "To delete the document, click Delete." Not: "Click Delete to remove the document."
* **Keep the articles.** Write: "Remove the backup file." Not: "Remove backup file."
* **Keep a paragraph under about six sentences.**
* **Vary the length on purpose.** A short sentence lands a point. A longer one carries a fact with its condition. One
  thought per sentence does not mean one length per sentence.

## Leave one reading, not two

* **Put "only" and "not" against the word they govern.**
  "Only the owner can merge a policy" and "The owner can merge only a policy" say different things.
* **Point a pronoun at a noun.** Repeat the noun where the referent is not obvious. Where it is obvious, leave the
  pronoun alone: the repair costs a manufactured subject, and a run of them reads as a tic. Write: "...drifted from the
  catalogue. That exit is what keeps the table honest."
  Not: "...drifted from the catalogue, which is what keeps that table honest."
  **"one", "this", "that" and "it" are pronouns here.** A word standing in for a noun answers to the same rule, and a
  definition is where it slips: the reader meets the claim before the thing it is about. Write: "A policy that hedges is
  not a policy." Not: "A policy that hedges is not one."
* **Give every clause its own verb.**
  Write: "Phase 1 moves the converters and Phase 2 moves the runtime."
  Not: "Phase 1 moves the converters and Phase 2 the runtime."
* **Break a noun string longer than three words.**
  Write: "the script that checks the proto-import budget." Not: "the proto import budget check script."
* **Say which parts an "and" or an "or" joins** when the sentence can group two ways. "Both...and" and "either...or"
  cost nothing.

## Punctuation

* **No em dash survives.** A dash separates without saying how the two parts relate, so the reader does that work. Take
  the mark that states the relation, and often the sentence reads best with the mark simply gone. Where the dash follows
  a link label opening a line, a colon deletes the line. Give the label a verb instead, and see the label rule below.
* **An interrupting aside takes parentheses**, where the aside holds its own commas and a comma pair cannot mark it.
  Parentheses say the reader may skip what they hold, which is what an aside is. A dash reads as drama it rarely earns.
  Write: *The bibliographic description of a title (author, edition, subject headings) held once and shared by every
  branch.*
  Use them sparingly. A second aside in one paragraph is usually a sentence of its own.
* **A colon points forward: what follows completes what precedes.** A list, an example, a reason, a consequence, a count
  or a definition all complete. Either half may be the reason for the other. A colon standing in for a verb or a
  conjunction completes nothing. Write: "The index is stale: three records changed." Write: "Three records changed: the
  index is stale."
  Not: "If you are coming from automation: you describe conditions."
* **An en dash is for a range or a pair.** `A.7.1–A.7.14`, `35–45 words`, `client–server`. A hyphen there is a different
  mark meaning a different thing.
* **The full stop is the default repair.** Take it where a semicolon offers itself, where the sentence already carries a
  colon, and where the container reserves the mark you wanted. A YAML plain scalar cannot hold `": "`, so a definition
  written there becomes two sentences rather than a quoted value. A colon inside quoted data, a path or an identifier
  belongs to that value rather than to the sentence, so it does not count. One semicolon survives: the pair joining a
  statement to its exact negation, sharing the verb it negates. That test decides a semicolon you are writing as well as
  one you found. Keep: *Its four operational obligations are deviable; those three prohibitions are not.*
* **Write the alternative out.** Write: "a, b, or both." Not: "and/or", "read/write".
* **Make anything in parentheses a whole grammatical unit.** A plural written "(s)" is never one.
* **Write the contraction where speech would use one.** "It's" and "cannot" both read naturally, and the document
  decides which fits. Whatever sent you here says how formal this one is.
* **Use straight quotes.**

## Use the real words

* **Write the real symbol, path, flag or command name.** The codebase is the word list, and a synonym is a second name
  for something that has one.
* **Swap a category for the things inside it.** Nothing gets cut or simplified. A class name becomes its members. Write:
  "An agent can write code, change configuration, draft documentation, or report something it noticed."
  Not: "Work produced by an AI agent enters our estate as a proposal."
* **A figure of speech earns its place when it says something the plain word does not.**
  Keep: "a broken cross-reference fails CI rather than rotting quietly." *Rotting* says silent **and** gradual.
  *Unnoticed* says only silent. Cut the figure that dresses a plain word: *substrate* for base, *vector* for way, *wedge
  in* for add, *north star*
  for goal, *flywheel* for what builds on itself. A word naming something real is not a figure at all, so a harness in a
  filename is that file's name.
* **Use the short everyday word.** "use" not "utilize", "help" not "facilitate", "do" not "perform". A long word has to
  buy its length with precision.
* **Call one thing by one name, everywhere.** Two names read as two things. Elegant variation is a defect here.
* **Gloss a precise term on first use.** "idempotent", "failure domain" and "trust boundary" earn their place once
  glossed.

## Cut

* **Cut any word the sentence survives without.** "in order to" is "to". "It is important to note that" is nothing. This
  reaches words that add nothing, and stops at words that disambiguate.
* **Cut these on sight:** simply, of course, seamless, robust, comprehensive, leverage, delve into, a tapestry of,
  pivotal, cutting-edge, serves as, it is worth mentioning, I hope this helps. A hedge stack and an adverb propping up a
  weak verb are the same fault. Write: "may." Not: "could potentially be argued that it might."
  Write: "is fast", or the measurement. Not: "runs quickly."
  An adverb carrying its own meaning stays: "an id that quietly means something new".
* **Give a claim one reason, and stop.** A second clause explaining the first explains nothing the first did not. Write:
  "Building it in costs a fraction of fixing it later."
  Not: "Building it in costs a fraction of fixing it later, because the expensive failures are structural, and a late
  fix does not reach them."
* **Say what you found.** Write: "three broken links." Not: "a number of issues."
* **Cut a sentence that could appear unchanged in another project's documentation.** It says nothing about this one.
  Write: "A column rename fails the build." Not: "Schema changes can cause issues."

## Watch the shape

* **Build a sentence on a contrast only where the reader would otherwise take the wrong reading.**
  Keep: "Copy `template/`, not `examples/`." A reader would reach for the wrong folder. Cut: "The tool validates rather
  than generates." Say what it does. **Count the contrasts before you finish.** Every one passes on its own. The
  fortieth in one shape fails. Count `rather than`, `, not ` and `, never ` over what you wrote, and aim under 5 per
  1000 words. A bulleted rulebook scores higher by construction, since each bullet is its own reading unit.
* **Use the honest number.** Two examples where two will do, four where four is true.
* **End a section on a fact, and a paragraph too.** A crafted line closing either one reads as style rather than
  substance, and the paragraph is where the habit starts. Where the last sentence restates the paragraph in a
  better-sounding shape, cut it and let the paragraph end on the point it made. Cut: "Without it, each of them becomes
  something we assert rather than something we can show."
* **A bold lead-in opens a list item or a paragraph, and new detail follows it.** A bold span closing inside a sentence
  is decoration. Write: "**Schema in TypeScript.** Tables live in one file." Not: "**Performance:** Performance
  improved."
* **A label names the thing beside it, and may be a verbless phrase.** Bold, a code span, a link label, a comment above
  a declaration, a field value next to its field name, a cell under its column heading. It takes a colon before a value
  and a full stop before sentences, and a comma carries a third part. Write: "**Platform**: CDN custom domain." Write: "
  **Repository**: `infrastructure`, at `services/covers`."
  Write: "**Retries.** The client backs off twice and then gives up."
  Write: "// One file, and the bytes it holds."
  A link label opening a line is the one that bites: `[pol-AGNT]: gate` is a link reference definition, and the line
  disappears from the page. A value of two words or more is safe, and a verb is safer still. Write: "[pol-AGNT] sets the
  acceptance gate."
* **A footnote closing a section is a whole line in italic, opening on a bold label.** Position tells the two forms
  apart: the label above sits in among the writing, and a footnote closes the section it is about. Wrap the line in
  underscores, which leaves the asterisks to mark a stressed word inside a sentence.
  Write: "_**Covers:** [pol-SCRT].EMBED, [pol-SCRT].LOGS_"
* **A bold lead-in may carry two steps.** Splitting it moves half the instruction outside the emphasis. Write: "**Retire
  the old policy and write a new one.**"
* **Bold the term, not the claim,** inside a paragraph. Where the claim is the misreading the paragraph exists to
  prevent, put that sentence first and let position carry it.
* **Italicise the word a speaker would stress.** One mark can carry what a clause of explanation was carrying, and the
  reader hears the sentence the way it was meant. Reach for it where the stress is the point, and leave it alone
  everywhere else: a page of emphasis emphasises nothing. Write it with asterisks, as the rest of the corpus does.
  Write: "software that serves *most* of them."
* **No emoji decorating prose.** A tick and a cross marking a good-and-bad pair are labels rather than decoration.
* **Use a numbered list only when the order carries meaning.** Introduce a list with a sentence saying what the items
  are, and keep the items parallel. Where the items speak for themselves, open the list without a stem.

## Write what stays true

* **Describe what is, not what changed**, and never as a correction of what was. Change history belongs to the commit
  message and the changelog, which are the two documents whose subject is change. A released changelog entry keeps the
  tense it shipped with, because the release it sits under fixes when it was true.
* **Name the command that reports a count.** A set fixed by decision rather than by accumulation may be counted. A
  number making a point rather than a tally is not a count: "correcting it alongside twenty others" is an argument about
  cost.
* **Cite rather than restate.** If the reasoning lives elsewhere, link it. Nobody updates a copy.

## Rewrite the block, not the diff

Adding a fact to prose that already exists means writing the whole block again with that fact in it. Prose grown one
edit at a time reads as a stack of additions, because that is what it is.

* **You should not be able to point at the sentence you added.** Where you can, you appended. Read the block whole,
  then write it again carrying everything it now has to say.
* **A block whose diff is only added lines was never re-read.** Nothing reworded and nothing dropped is the tell, and a
  pull request shows it.
* **A fact belonging to a list the block already carries joins that list.** It does not open a paragraph of its own.

## Rules you can ignore

Split an infinitive, open with *and* or *but*, end on a preposition, repeat a word rather than reach for a synonym. Each
is a half-remembered school rule, and a sentence contorted to obey one is worse for it.

## Commit messages and pull requests

* **The subject line says what changed.** Imperative, no full stop.
* **The body says why.** This is the one place where describing what used to be true is correct.
* **A pull request body carries the why and the evidence**, not a retelling of the diff.
* **Review the change, not the person.** Ask rather than assert where you might be wrong, and say what would change your
  mind.

## Before you finish

1. Read the longest sentence aloud. If you run out of breath, it is two sentences.
2. Read the last sentence of each section. Where more than one in a row is a crafted line, flatten the weaker.
3. Read the whole draft cold. Where you cannot tell which paragraph you wrote last, it is finished.
