---
name: writing-a-record
description: How to write inside a knowledge corpus. Covers a record of any tier, a type root page, the framework pages under `knowledge-as-code/`, and the schema's `description:` and `notes:` values. Load it after `technical-writing` whenever you write or change any prose a corpus holds.
---

# Writing a record

Load `technical-writing` first. Everything below either adds to it or says plainly which of its rules it overrides.

**Tier decides how a record is written, and type does not.** A runbook and a process are read under different
conditions and answer to different rules. An ADR and a discovery are not long and short versions of one thing. The
frontmatter carries `tier`, so read that row of the table below and leave the other four alone.

**A record holds the thing being governed, not information about it.** Ownership, lifecycle, relationships and
alignment are frontmatter. Prose restating any of them goes wrong before the frontmatter does.

## What this overrides

* **"Write in the present tense."** A postmortem records what happened, so its timeline and its account of the
  incident are past tense. A Decided record's consequences have not happened yet, so a consequence takes the future
  and the mechanism producing it stays present.
  Write: "**CI will become a gate on documentation.** A malformed document fails the build."
  Whatever either record says about the estate today stays present.
* **"They do not reach a clause whose wording is the record of an obligation."** That exemption covers the words and
  not the marks. Repair a clause's punctuation, and keep every word carrying obligation: a citation quotes the row, so
  dropping a qualifier changes what `pol-DEVI.OWNER` says to whoever cites it. The author accepts that diff in review,
  which is where a meaning that moved is caught. A capital the new mark forces is not a word change:
  `— see [pol-SCRT]` becomes `. See [pol-SCRT]`.
  **Inside a clause row this beats every other rule, on this page and on the floor.** A passive, an elided verb, a
  compound obligation, and a rationale the tier row would send to Purpose all stay, because each is wording a citation
  quotes. Report them and leave them. The tier row governs a clause you are writing, and this governs one you are
  editing.
* **"Gloss a precise term on first use."** The corpus has a glossary, and a term it defines is defined once. Link the
  entry rather than restating it.
* **Nothing else.** Where the floor and this page appear to disagree anywhere below, the floor wins.

## What outranks this page

**The schema is executable and this page is not.** Where the two disagree, the schema is right. Report the
contradiction rather than editing records to match this.

**Run `kac checks` for the type in front of you.** Each text rule is declared on a single type, and many types carry
none at all. The absence of a check is not permission.

**Run `kac generate` before you finish.** A record's own words are copied into generated output: the H1 lands in the
type's index, and frontmatter values land in the tables beside it. Change either and a generated file goes stale, so
regenerating is the last thing a turn does.

**Where you cannot run it, leave the H1 and the frontmatter exactly as they are.** A stale generated file fails
`generate --check` for whoever arrives next, and no repunctuated title is worth that. Say which rule you left behind.

## Use the corpus's own words

**Check the glossary before treating two words as interchangeable.** Where it distinguishes near-synonyms the
distinction is load-bearing. Otherwise pick one word and keep it.

**These three read as precise and are not.**

| Instead of              | Write                         |
|-------------------------|-------------------------------|
| blast radius            | how far a breach reaches      |
| escape hatch            | the exception a policy allows |
| single point of failure | the only person who knows     |

## Stay warm without softening the obligation

Write as a team that expects people to do the right thing and says why.

* **Give the reason, not the suspicion.** Where a rule exists because something went wrong, name what went wrong.
* **Skip the escalation.** A rule that shouts is no clearer than one that states. Cut *at all times*, *under no
  circumstances* and *strictly prohibited*.
* **State an exception early.** An exception written up front says the hard case was thought about.

None of that softens an obligation. A clause can be unambiguous and unhedged and still be written by somebody who likes
their colleagues.

## Keep the administration out

Information *about* the knowledge belongs in frontmatter, in a generated block, or in git.

**The test is repetition.** A paragraph that would appear unchanged in a second record is a corpus concern rather than
record content. What `aligns-with` means is written once, in `metadata.md`.

* **No review section.** `owner` and `review-by` are frontmatter and the history is in git.
* **No placeholder prose.** *No implementing standard exists yet* is a fact about the graph. An absent edge is absent,
  and needs no sentence saying so.
* **No frontmatter restated in the body.** Where a reader needs it rendered, the generator renders it.

## Write what exists

**Agreed and unbuilt work goes to the issue tracker**, where somebody closes it on the day it lands. A sentence about it
in a record has nobody to close it, and a reader four months later cannot tell the plan from the fact.

**One unbuilt thing is written down, because the tooling already reports it.** A type may declare a rule nothing
implements, and the generated checks table renders it under *Declared, not yet enforced*. Prose about such a rule states
the declaration in the present tense: *`feature-file-orphans` is declared and does not run*. Never *CI checks the
paths*. Claiming an unbuilt check as working is a correctness defect, and it is the commonest one here.

Where that needs a marker, `**Declared.**` leads the sentence it governs and reaches to the next heading at the same
level or higher.

**The rule is about the estate rather than about a decision.** A record at `status: proposed` holds a decision that
exists as a decision. Its Decision section is written in the present, and none of it claims the estate already changed.

## Write to the tier

| Tier            | Written as                                                                    |
|-----------------|-------------------------------------------------------------------------------|
| **Procedural**  | One action per step, imperative, condition before action. No reason in a step. |
| **Normative**   | One obligation per clause, testable, keyword leading. Reason lives in Purpose. |
| **Descriptive** | Indicative, present tense. State what is.                                     |
| **Decided**     | Prose earns its keep. Constrain structure and length, not vocabulary.         |
| **Observed**    | Symptom, cause, why it might matter. Three sentences beat three paragraphs.   |

### Procedural

Read under pressure, often at 3am, often by somebody who has not done this before. Every rule here exists because a
reader in that state skims, and a skimmed instruction still has to be right. The floor's condition rule bites hardest
here. A reader who stops halfway through *Restart the consumer if the queue depth exceeds 1000* does the wrong thing.

* **One action per step.** A step holding *and* is usually two steps.
* **No reason inside a step.** Why the step exists goes in the section preamble or in an explanation.
* **State the outcome.** A step the reader cannot confirm succeeded is a step they repeat.
* **A warning precedes the step it guards**, and never follows it.
* **Do not hedge an order.** *Typically*, *usually* and *normally* tell a reader under pressure that the sequence is
  negotiable. Where it genuinely is, say what decides it. `no-hedged-ordering` warns on this in a process and nothing
  catches it in a runbook.
* **A runbook opens with Symptoms**, because that is how a reader who does not yet know which record they need finds
  it. `symptoms-first` is an error rather than a warning.

### Normative

The rule has to be checkable, and a reader has to tell obligation from commentary at a glance.

**The clause rules bind the types that carry clauses**, which are policies and standards. A control, an NFR and an FAQ
are normative for a different reason: a reader may act on them without checking. Those three hold no clause table, no
Purpose and no RFC 2119 keywords. Every normative type answers to one sentence: state the thing a reader will act on,
make it checkable, and keep the argument out of the part that gets quoted.

* **A standard leads each clause with an RFC 2119 keyword, in capitals.** The capitals are what make it normative, and
  lower-case *must* is prose.
* **A policy carries its modals in the clause table and nowhere else.** Purpose and Scope are written as commitment.
  `clause-modal` is an error, and it asks for the modal bold where it binds and plain where it does not. What separates
  a policy clause from a standard's is altitude: a policy clause stays true when the whole technology estate is
  replaced.
* **One obligation per clause.** A clause that cannot be failed in exactly one way splits. `clause-order` and
  `clause-compound` warn where a row carries more than one.
* **A clause is testable, or it is a wish.** *Services are secure* is untestable. *Services read secrets from a managed
  vault* can be checked.
* **A clause states its obligation without help.** A cross-reference points at the other side of a shared obligation,
  or at the policy that owns it. It does not finish the sentence.
* **Write out every contraction.** A clause is quoted and acted on, and the formal register is what marks it as
  binding.
* **Read the clause again, deliberately, looking for the second reading.**
* **A policy never claims a compliance posture.** Alignment with an external framework is a clause-level reference.
  Whether the organisation is certified, registered or audited belongs in `frameworks.md` alone, and
  `posture-belongs-to-frameworks` warns where a policy reaches for that language.

### Descriptive

These mirror reality, so anything reading as intent eventually reads as a lie.

* **Indicative mood.** *The API authenticates with workload identity.* Not *should authenticate*.
* **No future tense.** Describe the service that runs today.
* **No promotion.** *World-class* and *best-in-class* describe nothing and survive no audit. The floor's word list
  carries the rest.
* **Never normative.** A **MUST** in an explanation puts the rule in two places and owns one of them.
  `not-normative` warns on RFC 2119 keywords here.
* **Link rather than restate.** A capability and an explanation are hubs, and length is judged against how much they
  link. `hub-not-specification` and `links-rather-than-restates` warn at roughly forty words per outbound link. A hub
  accumulating facts of its own has become a liability, and that ratio is what measures it. The other descriptive types
  are held to the same idea by judgement alone.

### Decided

The one tier where extended prose is the point. An ADR that cannot be argued with recorded nothing, and a postmortem
that reads comfortably has usually left something out. The constraint is structural rather than lexical.

* **Alternatives were genuinely weighed**, and the record shows the weighing. An option listed only to be dismissed in
  a clause was not considered.
* **Consequences include the unwelcome ones.** A consequences section holding only benefits is a sales document.
* **Attribute a cause to a system, not to a person.** In a postmortem that is the condition under which the next
  account gets written honestly.
* **Length is earned per point rather than per record.** Give each point the detail it needs, once.
* **A contraction belongs here where speech would use one.** Context and Alternatives are an account of what a
  team found and how it decided, so they read as a team talking. The decision sentence and the clauses do not.

### Observed

Capture stays nearly free, or it does not happen.

* **Symptom, cause where known, why it might matter.** Nothing else is required, and `low-ceremony` warns past two
  hundred words. A discovery long enough to need structure has become something else, and is promoted rather than
  padded.
* **Say what you do not know.** *Unconfirmed*, *seen once* and *may be specific to this branch* are honest here, and
  are what the confidence level carries.
* **Do not tidy a discovery into authority.** A discovery reading like an FAQ is trusted like one. Promotion is where
  certainty is added.

## A type root page

A `<type>.md` is a collection page. It carries no frontmatter and no `tier`, so the table above never reaches it. The
tier governs the records in the folder rather than the page describing them.

Hold it to the floor, and borrow a tier's **shape** where a section calls for it without taking on that tier's rules. A
Scope section reads descriptively. The numbered "Adding a…" steps take the procedural shape and may carry a short
reason, which a procedural record may not. Conventions sit close to normative without reaching for the keywords.

**The generated block owns what a field means, and the prose owns what an author does about it.** Where the two say the
same thing, cut the prose. Nobody updates it, and the generator rewrites its own.

## A `_template.md`

**A `{{placeholder}}`'s braces are the mark, and what sits inside them is prose.** `{{Stop the bleeding.}}` is a figure
dressing a plain instruction, and the tier's rules reach it.

**The guidance between `DELETE FROM HERE` and `DELETE TO HERE` is prose as well**, written for whoever fills the
template in. It answers to the floor, and to the tier the template's records carry.

---

**Markdown formatting.** Wrap prose at 120 columns. A table and a link definition are exempt, because a URL cannot be
broken. `.editorconfig` says so and no check enforces it.
