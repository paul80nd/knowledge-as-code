---
id: std-PROSE
tier: normative
status: active
implements: [ eng:pol-KNOW.AGENTS ]
applies-to:
  - all
review-by: "2027-09-02"
owner: paul.law
tags: [ documentation, prose, writing ]
---

# Prose reads the same way whoever wrote it

`Standard: std-PROSE` `ACTIVE`

## Summary

Every word this repository publishes answers to one set of writing rules: records, README files, code comments, schema
descriptions, documentation pages, commit messages and pull request bodies. The rules say how a sentence reads. What a
particular surface adds on top is that surface's own business.

## Rules

### The rules reach prose and stop where prose stops

- Every word written for a reader **MUST** answer to this standard, whoever or whatever wrote it.
- You **MUST** leave alone a heading you may not rename, an identifier, a clause whose wording is the record of an
  obligation, a fixed form such as an ADR's decision sentence, and a transcript quoting what a program printed.
- You **MUST** name in your reply every rule you left behind on one of those grounds.
- A comment inside a fenced code block counts as prose, and you **MUST NOT** touch the command, its flags or its
  output.
- A message in a string literal or a schema `message:` value counts as prose, and a transcript quoting that message
  back **MUST NOT** count as prose.
- Where two rules pull against each other, the one leaving the reader fewer readings **MUST** win, and where they tie
  you **MAY** take either answer.

_**Covers:** `eng:pol-KNOW.AGENTS`_

### Build the sentence

- The person or system a sentence already claims acted **MUST** be its subject, in the active voice.
- You **MUST** keep the passive where the text claims nobody, because the actor may be unknown, beside the point, or
  named somewhere this sentence should not name it.
- A passive that softens an obligation **MUST NOT** stand.
- Prose **MUST** run in the present tense, and *will* **MUST** wait for something that genuinely happens later.
- An instruction **MUST** read as a command.
- A commitment **MUST** say *we*, and anything addressed to somebody **MUST** say *you*.
- The common case **MUST** come first and the exception after it.
- A heading **MUST** carry the point, in sentence case.

### Carry one thing at a time

- A sentence **MUST** carry one idea, and an instruction **MUST** carry one step.
- A sentence holding both an obligation and its justification **MUST** split in two, and two steps joined by *and*
  **MUST** split too.
- An instruction **SHOULD** stay under about 20 words and other prose under about 25.
- An interrupting aside **MUST NOT** count toward that length.
- A condition **MUST** come before the step it guards.
- Every article **MUST** stay.
- A paragraph **SHOULD** stay under about six sentences.
- Sentence length **SHOULD** vary on purpose, so a short sentence lands a point and a longer one carries a fact with
  its condition.

### Leave one reading, not two

- *Only* and *not* **MUST** sit against the word they govern.
- A pronoun **MUST** point at a noun, and you **MUST** repeat the noun where the referent is not obvious.
- You **MUST** leave a pronoun alone where its referent is obvious, because the repair costs a manufactured subject and
  a run of them reads as a tic.
- *One*, *this*, *that* and *it* count as pronouns under that rule, and a definition **MUST NOT** open on a pronoun.
- Every clause **MUST** carry its own verb.
- A noun string **MUST NOT** run longer than three words.
- A sentence an *and* or an *or* can group two ways **MUST** say which parts it joins, and *both...and* and
  *either...or* cost nothing.

### Take the mark that states the relation

- An em dash **MUST NOT** survive, because a dash separates without saying how the two parts relate.
- You **MUST** replace it with the mark that states the relation, and the sentence **MAY** read best with the mark
  simply gone.
- An interrupting aside holding its own commas **MUST** take parentheses, and you **SHOULD** use parentheses sparingly.
- A colon **MUST** point forward, so that what follows completes what precedes: a list, an example, a reason, a
  consequence, a count or a definition.
- A colon standing in for a verb or a conjunction **MUST NOT** stand.
- An en dash **MUST** mark a range or a pair, as in `35–45 words` and `client–server`.
- A full stop **MUST** replace a semicolon that offers itself, a second mark where the sentence already carries a
  colon, and any mark the container reserves.
- One semicolon **MAY** survive: the pair joining a statement to its exact negation, sharing the verb it negates.
- You **MUST** write an alternative out, so *a, b, or both* replaces *and/or*.
- Anything in parentheses **MUST** be a whole grammatical unit, and a plural written *(s)* never is.
- A contraction **SHOULD** appear where speech would use one, and the surface a document sits on decides how formal it
  is.
- Quotes **MUST** be straight.

### Use the real words

- You **MUST** write the real symbol, path, flag or command name, because the codebase is the word list.
- You **SHOULD** swap a category for the things inside it, cutting nothing and simplifying nothing.
- A figure of speech **MUST** say something the plain word does not, or go.
- The short everyday word **MUST** win, so *use* beats *utilize* and *help* beats *facilitate*.
- One thing **MUST** carry one name everywhere, because two names read as two things.
- A precise term such as *idempotent* or *trust boundary* **MUST** carry a gloss on first use.

### Cut what the sentence survives without

- You **MUST** cut any word the sentence survives without, and keep any word that disambiguates.
- *Simply*, *of course*, *seamless*, *robust*, *comprehensive*, *leverage*, *delve into*, *a tapestry of*, *pivotal*,
  *cutting-edge*, *serves as* and *it is worth mentioning* **MUST** go on sight.
- A hedge stack and an adverb propping up a weak verb **MUST** go, and an adverb carrying its own meaning **MUST** stay.
- A claim **MUST** carry one reason and no second clause explaining the first.
- You **MUST** say what you found, so *three broken links* beats *a number of issues*.
- A sentence that could appear unchanged in another project's documentation **MUST** go, because it says nothing about
  this one.

### Watch the shape

- A sentence **MUST** rest on a contrast only where the reader would otherwise take the wrong reading.
- You **MUST** count the contrasts before you finish, over `rather than`, `, not ` and `, never `, and they **SHOULD**
  come under 5 per 1000 words of prose.
- Examples **MUST** run to the honest number: two where two will do, four where four is true.
- A section **MUST** end on a fact, and a paragraph **MUST** too.
- A last sentence restating the paragraph in a better-sounding shape **MUST** go.
- A bold lead-in **MUST** open a list item or a paragraph with new detail following it, and a bold span **MUST NOT**
  close inside a sentence.
- A label **MAY** be a verbless phrase, and **MUST** take a colon before a value and a full stop before sentences.
- A link label opening a line **MUST** carry a value of two words or more, because Markdown reads `[pol-AGNT]: gate` as
  a link reference definition and the line disappears from the page.
- A footnote closing a section **MUST** be a whole line in italic opening on a bold label, wrapped in underscores.
- A bold lead-in **MAY** carry two steps, because splitting it moves half the instruction outside the emphasis.
- Bold inside a paragraph **MUST** fall on the term and **MUST NOT** fall on the claim.
- Italics **MAY** mark the word a speaker would stress, written with asterisks, and a page of emphasis emphasises
  nothing.
- An emoji **MUST NOT** decorate prose, and a tick or a cross marking a good-and-bad pair counts as a label.
- A numbered list **MUST** appear only where the order carries meaning.
- A list **SHOULD** open on a sentence saying what the items are, with the items parallel, and **MAY** open without a
  stem where the items speak for themselves.

### Write what stays true

- Prose **MUST** describe what is, and **MUST NOT** read as a correction of what was.
- Change history **MUST** stay in the commit message and the changelog, which are the two documents whose subject is
  change.
- A released changelog entry **MUST** keep the tense it shipped with.
- A count **MUST** name the command that reports it, unless decision rather than accumulation fixes the set.
- A number making an argument about cost **MUST NOT** read as a count.
- Reasoning that lives elsewhere **MUST** carry a citation and not a restatement, because nobody updates a copy.

### Rewrite the block, not the diff

- You **MUST** carry a fact into prose that already exists by writing the whole block again with that fact in it.
- A reader **MUST NOT** be able to point at the sentence you added, because a sentence anyone can point at was appended.
- You **MUST** read a block whole and write it again where its diff holds only added lines.
- A fact belonging to a list the block already carries **MUST** join that list, and **MUST NOT** open a paragraph of
  its own.

### Commit messages and pull requests

- A subject line **MUST** say what changed, imperative and with no full stop.
- The body **MUST** say why, and it is the one place where describing what used to be true is correct.
- A pull request body **MUST** carry the why and the evidence, and **MUST NOT** retell the diff.
- A review **MUST** address the change and not the person, **SHOULD** ask rather than assert where the reviewer might
  be wrong, and **SHOULD** say what would change their mind.

### Rules that bind nothing here

- You **MAY** split an infinitive.
- A sentence **MAY** open with *and* or *but*, and **MAY** end on a preposition.
- A word **MAY** repeat rather than give way to a synonym.

## Examples

```
✅ Good
We cannot hold anyone to a rule if we cannot tell who acted.
The validator reports a missing id.
Remove the packet from the box.
To delete the document, click Delete.
The index is stale: three records changed.
An agent can write code, change configuration, draft documentation, or report something it noticed.
Building it in costs a fraction of fixing it later.
**Schema in TypeScript.** Tables live in one file.
[pol-AGNT] sets the acceptance gate.

❌ Avoid
Attribution is what makes this enforceable.
The validator will report a missing id.
The packet should be removed from the box.
Click Delete to remove the document.
If you are coming from automation: you describe conditions.
Work produced by an AI agent enters our estate as a proposal.
Building it in costs a fraction of fixing it later, because the expensive failures are structural.
**Performance:** Performance improved.
[pol-AGNT]: gate
```

The avoided lines each cost the reader a second pass. The passive hides who acted, the future tense describes a
validator that runs today, the trailing condition is read after the step it guards, the colon completes nothing, the
category names no members, the second reason explains what the first already did, the bold label repeats itself, and
the last line is a link reference definition that disappears from the page.

## Conformance checklist

- [ ] The longest sentence can be read aloud in one breath.
- [ ] No more than one crafted line closes two sections in a row.
- [ ] Reading the draft cold, you cannot tell which paragraph was written last.
- [ ] No em dash survives, and every semicolon joins a statement to its exact negation.
- [ ] Contrasts counted over `rather than`, `, not ` and `, never ` come under 5 per 1000 words of prose.
- [ ] Every instruction is a command, under about 20 words, with its condition first.
- [ ] Every count names the command that reports it.
- [ ] Every rule left behind on a prose-stops-here ground is named in the reply.

## Rationale and provenance

Markdown outnumbers code in this repository, and `git ls-files` run against `.md` and against `.cs` reports by how
much. How a sentence reads is therefore a property of the product rather than a matter of taste.

The rules an agent follows here reach it through the writing skills under `.claude/skills/`, which stand on their own
and name no record. This standard states the same rules as a record, so the estate can see what those skills were
written against and cite it the way it cites any other rule. The two are kept independent on purpose: a skill has to
work in a checkout that holds no corpus.

## Sources and further reading

- **Normative.** [RFC 8174] gives an RFC 2119 keyword its meaning only in capitals, so a lower-case *must* above binds
  nobody.

## Changelog

- 2026-09-02: initial version.

[RFC 8174]: https://www.rfc-editor.org/rfc/rfc8174
