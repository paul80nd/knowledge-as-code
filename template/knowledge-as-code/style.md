# Style

> How we write — every document, every comment, every commit message.

[Authoring](authoring.md) adds what a record's tier asks on top of this page, including how a clause is written.

Clear writing and warm writing are the same discipline. Writing gets hard to read when we perform — reaching for an
abstraction, a flourish, a passive that hides who acts. Name who does what, in a short sentence, with an ordinary word,
and the reader follows without effort. There is no trade-off to manage: the plain choice and the precise choice are
nearly always the same sentence.

## Scope

Everything we write down: corpus records, README files, code comments, commit messages, pull request descriptions and
review comments. Where a rule applies to only one of those, it says so.

The rules bind, and the judgement stays yours. A rule that makes a particular sentence worse is a rule to break, and to
say why in review.

## Keep one job per document

A document that drifts between explaining, deciding and instructing is three documents wearing one heading. What a
record is for is fixed by its tier — see [authoring](authoring.md) — so a description that starts giving orders, or a
decision that explains background instead of deciding, is not a style problem to smooth over. It is a sign the content
belongs in two documents, not one.

## Write for the reader

**Name who does what.** Write subject, verb, object, with a person or a system as the subject. Use the active voice.

> Attribution is what makes everything else enforceable.

Nothing does anything in that sentence. Compare:

> If we cannot tell who did something, we cannot hold anyone to any rule below.

Same claim, and the reader has it on the first pass. Passive is fine where the actor is genuinely unknown or genuinely
irrelevant, and nowhere else — including as a way to soften an unwelcome sentence. An obligation that softens is one a
reader cannot pin down.

**Give the instruction, not a sentence about it.**

> Remove the packet from the box.

not

> The packet should be removed from the box.

The second is longer, vaguer about who acts, and no gentler. Where a courtesy word helps, use it; where compliance is
not optional, leave it out. "Please" offers a choice the reader does not have.

**Write "we" for us and "you" for the reader.** A policy is a commitment we hold, so it says "we". A README, a runbook
or a review comment is addressed to somebody, so it says "you".

## Anchor the abstract in something real

A line built entirely from abstractions rarely lands. A line anchored to something the reader can picture does.

> Undocumented knowledge is a single point of failure that no amount of redundancy elsewhere compensates for.

> A backup that has never been restored is not a backup.

Both reach for something true. Only the second names something you could go and check this afternoon. The same move
works on any claim — replace a category with the things inside it:

> Work produced by an AI agent enters our estate as a proposal.

> An agent can write code, change configuration, draft documentation, or report something it noticed about a system.

We cut nothing and simplified nothing. We swapped a class name for its members.

## Say it once, and say it plainly

* **One idea per sentence.** A sentence carrying an obligation and its justification is two sentences.
* **Keep sentences short.** As a guide, an instruction reads best under 20 words and other prose under 25 — treat both
  as symptoms, not hard limits. A 25-word sentence with a person as its subject can read on one pass; a 12-word sentence
  built on two abstractions may not. Fix the subject before you shorten it.
* **Keep paragraphs short.** Past about six sentences, a paragraph is usually carrying a second point that deserves its
  own.
* **Use the same word for the same thing.** Two names for one thing read as two things. Check the glossary before
  treating two words as interchangeable — where it distinguishes near-synonyms, the distinction is load-bearing.
  Otherwise, pick one word and keep it.
* **Cite rather than restate.** If the reasoning lives somewhere else, link it. Nobody updates a copy.
* **Cut any sentence that only announces the next one**, and any word that adds nothing if removed: *simply*, *of
  course*, *it is important to note*, *in order to*.

### Rules you can ignore

Split an infinitive, open with *and* or *but*, end on a preposition, use the same word twice. Each is a half-remembered
school rule, and a sentence contorted to obey one is worse for it.

## Make it hold together

A reader should be able to move forward. Each of these sends them back.

* **Give a pronoun something to point at.** *The acceptance gate has none* — has no what? A definite reference — *the
  same*, *none*, *that*, *it* — needs its antecedent in the sentence before.
* **Don't drop a verb across a clause boundary.** *…and no recorded deviation can.* Can what? The reader has to rebuild
  the verb from a clause they already left.
* **Put a modifier next to what it modifies.** *Only the owner can merge a policy* and *The owner can merge only a
  policy* say different things. Put *only*, *not* and *every* directly against the word they govern.
* **Repeat the noun rather than chain pronouns.** Three sentences of *It… It… It* pointing at one subject give the
  reader three chances to lose it.
* **Make each list item finish the stem that introduces it.** A list breaks when one item no longer completes the
  sentence above it. Prefer bullets to numbers unless the order itself carries meaning.
* **Write out an alternative rather than slashing it.** *and/or*, *read/write* — a slash asks the reader to resolve an
  ambiguity we could have resolved for them.

## Cut what doesn't earn its place

Vagueness is the enemy, not a plain word — jargon that carries real precision (*idempotent*, *failure domain*, *trust
boundary*) earns its place if you gloss it on first use. What doesn't earn its place is language chosen to sound
considered rather than to say something:

| Instead of                    | Write                                           |
|-------------------------------|-------------------------------------------------|
| leverage, utilize             | use                                             |
| facilitate                    | help                                            |
| delve into                    | cover, look at                                  |
| a tapestry of, a landscape of | name the things directly                        |
| pivotal moment, cutting-edge  | say what changed, or cut it                     |
| not just X but Y              | pick the one that's true, or state both plainly |
| serves as                     | is                                              |
| blast radius                  | how far a breach reaches                        |
| escape hatch                  | the exception a policy allows                   |
| single point of failure       | the only person who knows                       |

A few more habits are worth naming. Each is easy to write without noticing:

* **Don't force ideas into groups of three** where two would do, or four would be honest.
* **Keep noun clusters to three words.** *Recovery point objective* is legible; break anything longer with a preposition
  or a verb.
* **Say what you actually found**, not that you found "a number of issues" or "several considerations." Name the number,
  or name the thing.
* **Skip the pleasantries and disclaimers that pad AI-generated prose** — "I hope this helps," "please note that,"
  "it's worth mentioning." If it isn't information, cut it.

## Watch the shape, not just the words

A sentence can be correct on its own and still be the fifth in a row with the same shape. The reader feels this as
monotony before they can say why.

* **Don't build every sentence around a contrast.** *X, not Y* is a good device exactly where a reader would otherwise
  land on the wrong reading — reach for it there, and let an ordinary declarative carry everything else.
* **Prefer a full stop to an em dash.** One a paragraph is plenty; past that, most of them are sentences that didn't
  commit.
* **Don't manufacture a punchline for every section.** Most sections should end on an ordinary sentence that states a
  fact. A short quotable line closing every section reads as style rather than substance.
* **Skip decorative formatting.** No emoji, no title-case headings, no bolding a whole sentence to make it sound more
  important than the one beside it — bold the term, not the claim.

## Write what stays true

Someone will read this in a year, having missed the conversation that produced it.

* **Describe what is, not what changed**, and never as a correction of what was. Change history belongs in the commit
  message and the changelog.
* **Don't count the corpus.** A number written into prose is stale on the next merge. Where a count is load-bearing,
  generate it or point at the command that reports it. A closed set — the tiers, an external framework's control count —
  may still be counted. It moves by decision rather than by accumulation.

## Warmth, without softening the obligation

We write as a team that expects people to do the right thing and says why, not as one that assumes they will not.

* **Give the reason, not the suspicion.** Where a rule exists because something went wrong, name what went wrong.
* **Skip the escalation.** *At all times*, *under no circumstances*, *strictly prohibited* — a rule that shouts is no
  clearer than one that states.
* **State exceptions early.** An exception written up front says we thought about the hard case.
* **Assume the reader wants to get it right.**

None of this softens an obligation. A clause can be unambiguous and unhedged and still be written by someone who likes
their colleagues.

## By medium

**Code comments.** The code says what it does; the comment says why it does it that way. A comment restating the line
beneath it is noise, and it will be wrong within a month. Do not apologise in a comment — *hacky*, *sorry*, *don't
ask* — say what the constraint was instead.

**Commit messages.** The subject line says what changed. The body says why — the one place where describing what used to
be true is correct.

**Pull requests and reviews.** [Contributing](contributing.md) covers the process. Review the change, not the person.
Ask rather than assert where you might be wrong, and say what would change your mind.

## Before you publish

Run these against what you actually wrote, as a pass over the finished draft:

1. **Name the subject of each sentence.** Where the answer is "nothing," rewrite it.
2. **Read the longest sentence aloud.** If you run out of breath, it is two sentences.
3. **Scan for the words in the table above**, and for any sentence that sounds more like a press release than a
   colleague.
4. **Read the last couple of sentences of each section.** If more than one in a row is a crafted one-liner, flatten the
   weaker.
5. **Read it as someone who was not in the conversation.** That reader is most of your audience.

This page's language, and the tier rules in [authoring](authoring.md), are informed by outside work on plain and
controlled language. The debt and its limits are recorded in [lineage](lineage.md#language).
