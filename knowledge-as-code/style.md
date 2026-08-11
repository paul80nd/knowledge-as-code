# Style

How we write — every document, every comment, every clause. [Authoring](authoring.md) adds what a record's tier asks on
top of this page.

**One idea sits underneath all of it.** Clear writing and warm writing are the same discipline. We make writing hard to
read when we perform — when we reach for an abstraction, an epigram, or a passive that hides who acts. That puts
distance between us and the reader, and we do it when we write *at* someone rather than *to* them. When we name who does
what, in a short sentence, with an ordinary word, we trust the reader to follow.

So there is no trade-off to manage. The warm choice and the precise choice are nearly always the same sentence.

## Scope

Everything we write down: corpus records, README files, code comments, commit messages, pull request descriptions and
review comments. Where a rule applies to only one of those, it says so.

The rules bind, and the judgement stays yours. A rule that makes a particular sentence worse is a rule to break, and to
say why in review.

## Anchor what you write to something real

An abstraction works when it sits beside something concrete. It fails when it floats.

**A line anchored to a physical thing lands on first reading. A line built out of abstractions does not.**

> A backup that has never been restored is not a backup.

> Undocumented knowledge is a single point of failure that no amount of redundancy elsewhere compensates for.

The first names something you can point at. The second stacks four abstractions and applies a hardware metaphor to a
person. Both are reaching for something true. Only one arrives.

The same move works on any claim. **Replace a category with the things inside it.**

> Work produced by an AI agent — code, configuration, documentation, or an observation about how something behaves —
> enters our estate as a proposal.

> An agent can write code, change configuration, draft documentation, or report something it noticed about a system.

The second names things a reader can picture. We cut nothing and simplified nothing. We swapped a class name for its
members.

### The test for a memorable line

The backup example above takes a shape we reach for often: *an X that fails some condition is not an X*. The shape is
fine. Most instances of it are not, and the two look identical on the page.

**Ask what the line points at outside itself.** "A backup that has never been restored" points at an operation you can
go and run this afternoon, and the reader finishes the sentence knowing what to do. Compare:

> A taxonomy that can only grow by changing code is a taxonomy that stops growing.

That points at nothing but its own premise. It restates "hard to extend" as "will not be extended" and adds a verdict.
The reader learns our opinion of the situation and nothing about the situation.

Three failing shapes, each of which reads as insight:

* **The restatement.** *A discovery that takes ten minutes to write is a discovery nobody writes.*
* **The bare reversal.** *Documentation is a view over the graph. The graph is the thing.*
* **The verdict.** *A check that warns but does not block will eventually be ignored.* This may well be true. It is a
  claim about the future dressed as a definition.

**One such line per document, and only where it survives the test.** This bites hardest on the line you are pleased
with. The difference is easier to see the morning after.

## Name who does what

The most common way our writing goes wrong is by deleting the actor.

> Attribution is what makes everything else enforceable.

Nothing does anything in that sentence. "Attribution" stands in for people writing their names against changes. The
construction — *X is what makes Y possible* — lifts an abstraction into the subject slot. It reads as insight. It
carries less than it appears to.

> If we cannot tell who did something, we cannot hold anyone to any rule below.

Same claim, and the reader has it on the first pass.

**Write subject, verb, object, with a person or a system as the subject.** Use the active voice. Passive is fine where
the actor is genuinely unknown or genuinely irrelevant, and nowhere else.

That cleft is one shape of the defect. Any abstract noun in the subject slot does the same work — *acceptance*,
*authority*, *provenance*, *proportion*. You can cut every cleft and leave the defect in place. A document can have
every sentence under 25 words and still read cryptically.

Test each sentence by naming its subject. Where the subject is a noun made from a verb, rewrite until it is a person, a
system or a document.

Plain-English guidance allows one more use of the passive: it softens an unwelcome sentence by removing the person from
it. That works in a letter to a customer, and **we decline it here.** An obligation that softens is one a reader cannot
pin down, and the kind thing in a rule is to say who is bound.

## Vary the shape across a section

Every rule above works on one sentence. This one works on a page, and our own writing breaks it more than any other.

A sentence can obey every rule here and still be the fortieth in the same shape. Nothing inside a single sentence tells
you that. The reader feels it as monotony, stops hearing the argument, and starts hearing the writer.

**Definition by contrast is our house tic.** *X, not Y.* *X rather than Y.* *Not X. Y.* It is a good device, and it
earns its place where a reader would otherwise land on the wrong reading:

> The id names the deployable, not the repository.

Someone really will reach for the repository name. The contrast does work. Now compare:

> The split is load-bearing rather than tidy.

Nobody thought it was tidy. That contrast costs the reader a beat spent discarding an alternative they never held.

**State the point plainly first. Add a contrast only where a reader would otherwise get it wrong, and at most once per
section.** Two contrasts in adjacent paragraphs means the second is decoration. Where you want emphasis and the reader
is in no danger of the wrong reading, an ordinary declarative carries it: *The id names the deployable.*

**The same ceiling applies to any device you like.** One replacement applied everywhere becomes the next mannerism. Swap
every contrast for the same conditional and you have moved the tic, not removed it. It now sits in the paragraph the
contrast used to occupy. Three positions are worth watching:

* **The section closer.** A short, quotable line at the end of every section is a rhythm, and a reader meets it as
  style. Let most sections end on an ordinary sentence.
* **The em dash.** It bolts a qualification onto a sentence you did not want to commit to. Roughly one per paragraph is
  plenty. Past that, some of them are full stops.
* **The opening reversal.** *Repositories and folders are storage. The relationships are the knowledge.* Once a page at
  most.

Read a finished section and mark the shape of each sentence. Where four in a row share one, rewrite three.

## Say it once, and say it short

* **One idea per sentence.** A sentence carrying an obligation and its justification is two sentences.
* **Aim for an average of 15 to 20 words, and treat 25 as a ceiling.** Both are diagnostics rather than targets, and
  length is a symptom. A 25-word sentence with a person as its subject reads on one pass; a 14-word sentence built on
  two abstractions does not. **When a sentence is hard, fix the subject before you shorten it.** Shorten one whose
  subject is an abstraction and you get a short sentence that is still hard.
* **Six sentences to a paragraph.** Past it, a paragraph is usually carrying a second point that deserves its own.
* **Prefer the short word, and the same word.** Two names for one thing read as two things. Elegant variation is a
  defect here. Where the glossary distinguishes near-synonyms, the distinction is load-bearing.
* **Cut filler.** *Simply*, *obviously*, *of course*, *it is important to note*, *in order to*. Delete the word and read
  the sentence again. If nothing was lost, it was filler.
* **Cut any sentence that only announces the next one.**
* **Make every item in a list finish the line that introduces it.** A list breaks most often when one item no longer
  completes the stem. Prefer bullets to numbers unless the order carries meaning.
* **Cite rather than restate.** If the reasoning lives somewhere else, link it. Nobody updates a copy.

### Rules you can ignore

Split an infinitive where it reads better. Open a sentence with *and*, *but* or *so*. End one with a preposition. Use
the same word twice rather than reaching for a synonym you like less.

Each is a half-remembered school rule. A sentence contorted to obey one is worse for it.

## Leave nothing to reconstruct

A reader should be able to move forwards. Each of these sends them back.

* **A pointer needs something to point at.** *The acceptance gate has none* — has no what? The answer sat in the section
  heading rather than the sentence. A definite reference — *the same*, *none*, *that*, *it* — needs its antecedent in
  the sentence before. Make it a noun the reader can point at.
* **Do not drop a verb across a clause boundary.** *…and no recorded deviation can.* Can what? The reader rebuilds the
  verb from a clause they have already left.
* **Repeat the noun rather than chain pronouns.** Three sentences of *It… It… It* pointing back at one subject give the
  reader three chances to lose it. Naming it once mid-chain costs a word.
* **Do not define a word only by its opposite.** *That is proportion, not exception.* An X-not-Y frame works where the
  reader already holds the distinction and fails where they do not. Where they might not, state the mechanism first: *a
  person who accepts agent work becomes its author, not its approver.* The frame survives once there is a person inside
  it.

## Use the precise word, and explain it once

Jargon is not the enemy of clarity. Vagueness is. *Idempotent*, *failure domain*, *trust boundary* and *shed load* each
carry something no plain phrase carries as tightly, and replacing them costs precision.

**Keep the precise term and gloss it on first use in a document.** Replace one only where a plain phrase says the same
thing in the same space.

**Noun clusters run to three words.** *Recovery point objective* is legible. *Corpus dependency resolution failure mode*
is not — break it with a preposition or a verb.

Words we have reached for and should not, each found in our own writing:

| Instead of              | Write                                                     |
|-------------------------|-----------------------------------------------------------|
| blast radius            | how far a breach reaches                                  |
| lateral traffic         | traffic between our own systems                           |
| correlatable            | able to be lined up across systems                        |
| resolvable inventory    | an inventory naming each component and the version in use |
| escape hatch            | the exception a policy allows                             |
| mainline, main branch   | the default branch                                        |
| single point of failure | the only person who knows                                 |

Ask who is reading. An auditor, a new joiner and an agent all read what we write, and none of them arrives with our
context.

## Write what stays true

Someone will read this in a year, in a repository you have not seen, having missed the conversation that produced it.

* **Describe what is, not what changed**, and never as a correction of what was. A paragraph justifying a change reads
  as noise a month later. Change history belongs in the commit message and the changelog.
* **Do not count the corpus.** A measurement written into prose is stale on the next merge. A corpus that took this
  framework and grew its own content was never described by it at all. Where a number is load-bearing, generate it or
  point at the command that reports it.
* **A closed set may be counted.** Four categories, the tiers, an external framework's control count. Each moves by
  decision rather than by accumulation.

## Warmth, concretely

We write as a team that expects people to do the right thing and says why, not as one that assumes they will not.

* **Write "we" for us and "you" for the reader.** A policy is a commitment we hold, so it says "we". A README, a runbook
  or a review comment is addressed to somebody, so it says "you". Both appear on this page.
* **Give the reason, not the suspicion.** Where a rule exists because something went wrong, name what went wrong. Do not
  write as though the reader is the one who will do it next.
* **Skip the escalation.** *At all times*, *under no circumstances*, *strictly prohibited*. Emphasis of that kind tells
  a reader we expect to be ignored, and a rule that shouts is no clearer than one that states.
* **State exceptions early and plainly.** An exception written up front says we thought about the hard case. An
  exception discovered later says we did not.
* **Assume the reader wants to get it right**, and is reading because they do.

None of this softens an obligation. A clause can be unambiguous and unhedged and still be written by someone who likes
their colleagues.

## Give the instruction

Where you want someone to do something, write the instruction. Not a sentence about the instruction.

> Remove the packet from the box.

> The packet should be removed from the box.

The second is longer, vaguer about who acts, and no gentler. We avoid commands because we think they sound harsh. What
we reach for instead is harder to follow at the moment someone is following it.

**Where a courtesy word helps, use it. Where compliance is not optional, leave it out.** "Please" offers a choice the
reader does not have, and offering one that is not real is unkinder than the plain instruction.

This matters most in a runbook read at 3am, and in a clause read in an audit. Those are the two places our writing is
under real pressure.

## Writing that binds

People read clauses, rules and controls under different conditions from everything else: scanned, cited on their own,
quoted back in an audit. They carry the whole weight of precision, and they should carry none of the argument.

* **The clause states the obligation. The Purpose earns it.** Warmth lives in the Purpose. A reader who understands why
  something matters will want to do it, and that carries further than a requirement. A clause with a reason attached is
  one an auditor cannot parse, because they cannot tell where the obligation ends.
* **One obligation per clause.** If it needs an *and* joining two actions, it is two clauses.
* **A clause is testable, or it is a wish.** Before writing one, ask what an auditor would ask to *see*. If you cannot
  answer, rewrite it.
* **A clause states its obligation without help.** A cross-reference points at the other side of a shared obligation, or
  at the policy that owns it. It does not finish the sentence for you.
* **Write it so it cannot be read two ways.** Then read it again, deliberately, looking for the second way.

Standards and checks catch us when we fall. They are not why we do this.

## A worked section

The rules above land on sentences. This is what they look like held together over a page. The subject is a fictional
incident record; read it for shape.

> ## Why we use them
>
> An incident record is the account of one outage: what customers saw, when we noticed, what we changed, and when it
> ended. One record per incident, written while people still remember it.
>
> Two audiences read these. Someone on call next month wants to know whether this has happened before and what worked.
> Someone planning next quarter's work wants to know which parts of the estate keep producing outages. Both questions
> need the same facts, so we keep one account and not two.
>
> ## Scope
>
> An incident is any unplanned loss of service a customer could notice. A failed deployment nobody outside the team saw
> is not one; record it as a discovery.
>
> The record covers the outage. It does not cover the fix. Where the outage produced work, that work is a normal change
> and goes through the normal route, and the record links to it.

Four things are happening, and none of them is a rule you can apply to a single sentence.

1. **The first sentence names the thing and its parts.** No preamble, and no definition of "incident" in the abstract.
2. **The sentence shapes vary.** A definition, then two parallel sentences about readers, then a conditional. The one
   contrast in the passage — *not two* — arrives after the reason for it.
3. **The scope section says what is excluded and where it goes instead.** A boundary with nowhere to send the reader
   gets ignored.
4. **Neither section closes on an epigram.** The last sentence of each states a fact.

## By medium

**Corpus records.** [Authoring](authoring.md) holds the rules that follow a document's tier. Everything on this page
applies underneath them.

**Code comments.** The code says what it does. The comment says why it does it that way. A comment restating the line
beneath it is noise, and it will be wrong within a month. Where a comment explains a workaround, name what is being
worked around and what would let it go. Do not apologise in a comment — *hacky*, *sorry*, *don't ask* — say what the
constraint was.

**Commit messages.** The subject line says what changed. The body says why. Change history belongs here, so this is the
one place where describing what used to be true is correct.

**Pull requests and reviews.** [Contributing](contributing.md) covers the process. On the words: review the change, not
the person. Ask rather than assert where you might be wrong, and say what would change your mind. A review comment that
is right and lands badly has cost more than it bought.

## Before you publish

Run these against what you actually wrote. This is a pass over the draft, not a list to keep in mind while writing.

1. **Name the subject of each sentence.** Where the answer is "nothing", rewrite it.
2. **Read the longest sentence aloud.** If you run out of breath, it is two sentences.
3. **Mark the shape of each sentence in your longest section.** Four alike in a row means rewrite three.
4. **Count the contrasts in each section.** More than one, and the rest are decoration.
5. **Read the last sentence of each section.** Where more than one is a short quotable line, rewrite them.
6. **Find the line you are proudest of.** Ask what it points at outside itself. Cut it if the answer is nothing.
7. **Read it as someone who was not in the conversation.** That reader is most of your audience.

Our debts are recorded in [lineage](lineage.md#language).
