# Authoring

What a record's tier asks of its prose. [Style](style.md) is the floor under this page and binds every document in this
repository. Read it first. [Contributing](contributing.md) covers the mechanics and says which page owns what.

Two rules carry most of the weight, and both are easier to state than to follow:

* **A record holds the thing being governed, not information about it.** Ownership, lifecycle, relationships and
  alignment are frontmatter. Prose that restates them is prose that will be wrong before the frontmatter is.
* **How a document is written follows its tier, not its type.** A runbook and a process are read under different
  conditions and obey different rules. An ADR and a discovery are not long and short versions of the same thing.

Both serve one end. A corpus is read under pressure, by people who did not write it and by agents that cannot ask what a
sentence meant. Length is not thoroughness. A document that says less, and says it once, is read.

Some of this is enforced, though less than you would hope and never uniformly. Each text rule is declared on a single
type — `low-ceremony` on discoveries, `not-normative` on explanations, `no-hedged-ordering` on processes — and many
types carry none at all. Run `kac checks` to see what applies to the type in front of you rather than assuming. Expect
`kac validate` to come back clean: CI gates the branch, so a clean run is the baseline rather than a source of findings.
The rest is judgement, and the absence of a check is not permission.

Where a rule here contradicts the schema, **the schema is right and this page is wrong**. It is executable and this is
not. Report the contradiction; do not resolve it by editing records.

## Type root pages

A `<type>.md` is a **collection** page. It carries no frontmatter and no `tier`, so the table below never reaches it:
the tier governs the records inside the folder, not the page describing them.

Hold a collection page to [style](style.md), and borrow each tier's **shape** where a section calls for it without
taking on its full rule set. A Scope section reads descriptively. The numbered "Adding a…" steps take the procedural
shape — one action to a step, condition before action — and may carry a short reason, which a procedural record may not.
Conventions sit close to normative without reaching for the keywords.

The generated block owns what a field means. The prose owns what an author does about it. Where the two say the same
thing, the prose is the copy to cut, because nobody updates it and the generator rewrites its own.

## By tier

Style applies throughout, and each tier adds to it. Your document's frontmatter carries its `tier`, so read the row that
matches it; [taxonomy](taxonomy.md#the-types) groups the corpus's types under the tier each belongs to.

| Tier            | Written as                                                                               |
|-----------------|------------------------------------------------------------------------------------------|
| **Procedural**  | One action per step, imperative, condition before action. No rationale in a step.        |
| **Normative**   | One obligation per clause, testable, keyword leading. Rationale lives in Purpose.        |
| **Descriptive** | Indicative, present tense. State what is. No future tense — a plan is not a description. |
| **Decided**     | Prose earns its keep. Constrain structure and length, not vocabulary.                    |
| **Observed**    | Symptom, cause, why it might matter. Three sentences beat three paragraphs.              |

### Procedural

Read under pressure, often at 3am, often by someone who has not done this before. Every rule here exists because a
reader in that state skims, and a skimmed instruction must still be right.

* **One action per step.** A step containing *and* is usually two steps.
* **The condition comes first.** `If the queue depth exceeds 1000, restart the consumer` — never `Restart the consumer
  if the queue depth exceeds 1000`. A reader who stops halfway through the second form does the wrong thing.
* **Twenty words to a step.** Tighter than the floor, because the reader is not reading carefully.
* **No rationale inside a step.** Why the step exists goes in the section preamble or an explanation.
* **State the outcome.** A step the reader cannot confirm succeeded is a step they will repeat.
* **Warnings precede the step they guard**, never follow it.
* **Do not hedge an order.** *Typically*, *usually* and *normally* in a step tell a reader under pressure that the
  sequence is negotiable. If it genuinely is, say what decides it. `no-hedged-ordering` warns on this in a process;
  nothing catches it in a runbook.
* **A runbook opens with Symptoms**, because that is how a reader who does not yet know which document they need finds
  it. `symptoms-first` is an error, not a warning.

### Normative

The rule must be checkable, and a reader must be able to tell obligation from commentary at a glance.

**The clause rules below bind the types that carry clauses**, which are policies and standards. A control, an NFR and an
FAQ are normative because a reader may act on them without checking. They hold no clause table, no Purpose and no RFC
2119 keywords. What binds every normative type is the first sentence above: state the thing a reader will act on, make
it checkable, and keep the argument out of the part that gets quoted.

* **Standards use RFC 2119 keywords, in capitals, leading the clause.** The capitals are what make them normative;
  lower-case *must* is prose. See [lineage](lineage.md#standard).
* **Policies carry modals in their clauses and nowhere else.** Purpose and Scope are written as commitment — what we
  hold to, and why. The clause table states the obligation and opens each row with its keyword;
  `clauses` is an error if a row does not. What separates a policy clause from a standard's is altitude, not grammar:
  a policy clause stays true when the whole technology estate is replaced.
* **One obligation per clause.** If a clause cannot be failed in exactly one way, split it —
  `clause-order / clause-compound` warns when a row carries more than one.
* **A clause is testable, or it is a wish.** *Services are secure* is untestable. *Services read secrets from a managed
  vault* can be checked.
* **Rationale lives in Purpose, not in the clause.** The clause table is scanned; the purpose is read once.
* **A policy never claims a compliance posture.** Alignment with an external framework is a clause-level reference;
  whether the organisation is certified, registered or audited belongs in [`frameworks.md`](/frameworks.md) alone.
  `posture-belongs-to-frameworks` warns when a policy reaches for that language.

### Descriptive

These must mirror reality, so anything that reads as intent will eventually read as a lie.

* **Indicative mood, present tense.** *The API authenticates with workload identity.* Not *will authenticate*, not
  *should authenticate*.
* **No future tense.** Describe the service that runs today. Where one is agreed and unbuilt, the issue tracker holds
  it — see [below](#write-what-exists).
* **No promotion.** *Robust*, *seamless*, *world-class*, *best-in-class* describe nothing and survive no audit.
* **Never normative.** A **MUST** in an explanation means the rule is now in two places and only one of them is owned.
  `not-normative` warns on RFC 2119 keywords appearing here.
* **Link rather than restate.** A capability and an explanation are hubs, and their length is judged against how much
  they link — `hub-not-specification` and `links-rather-than-restates` warn at roughly forty words per outbound link.
  One accumulating facts of its own has become a liability, which is what that ratio measures. The other descriptive
  types have no such rule and are held to it by judgement alone.

### Decided

The one tier where extended prose is the point. An ADR that cannot be argued with is an ADR that recorded nothing, and a
postmortem that reads comfortably has usually left something out.

The constraint here is structural rather than lexical:

* **Alternatives were genuinely weighed**, and the document shows the weighing. An option listed only to be dismissed in
  a clause was not considered.
* **Consequences include the unwelcome ones.** A consequences section containing only benefits is a sales document.
* **Attribute causes to systems, not to people.** In a postmortem this is not politeness; it is the condition under
  which the next account gets written honestly.
* **Length is earned per point, not per document.** Give each point the detail it needs and make it once.

### Observed

Capture must stay nearly free or it does not happen.

* **Symptom, cause if known, why it might matter.** Nothing else is required, and `low-ceremony` warns past two hundred
  words. A discovery long enough to need structure has become something else, and should be promoted rather than padded.
* **Say what you do not know.** *Unconfirmed*, *seen once*, *may be specific to this branch* — hedging is honest here
  and is what the confidence level exists to carry.
* **Do not tidy it into authority.** A discovery that reads like an FAQ will be trusted like one. Promotion is where
  certainty is added.

## Intent, not administration

A record holds the knowledge. Information *about* the knowledge belongs in frontmatter, in generated blocks, or in git.

The test is repetition: **if a paragraph would appear in more than one record, it is not record content.** An
explanation of what `aligns-with` means, or of the difference between alignment and certification, is a corpus concern.
It belongs [here](metadata.md) and is written once. Repeated in every policy, it is twenty copies to keep in step and
twenty chances to be wrong.

Three consequences worth stating outright:

* **No review section.** `owner` and `review-by` are frontmatter, and the history is in git. A prose paragraph restating
  them is a copy, not a source.
* **No placeholder prose.** *No implementing standard exists yet* is a fact about the graph, not about the policy. An
  absent edge is absent; it does not need a sentence saying so.
* **No frontmatter restated in the body.** If the reader needs it rendered, that is the generator's job.

## Write what exists

Every document in this repository describes the corpus and the tooling as they are today. Agreed and unbuilt work goes
to the issue tracker, where somebody closes it on the day it lands. A sentence about it in a record has nobody to close
it, and a reader four months later cannot tell the plan from the fact.

That reaches the framework's own pages. [Automation](automation.md) groups the checks the validator runs, and
[principles](principles.md) describes how the framework works today. A corpus running this framework has no use for the
framework's roadmap, and leaving the roadmap out is what keeps every copy of these pages identical.

**One unbuilt thing gets written down, because the tooling already reports it.** A type may declare a rule that nothing
implements, and the generated checks table renders it under *Declared, not yet enforced*. Prose about such a rule states
the declaration, in the present tense: *`feature-file-orphans` is declared and does not run*, never *CI checks the
paths*. Claiming an unbuilt check as working is a correctness defect, and it is the most common one in this corpus.

Where that needs a marker, `**Declared.**` leads the sentence it governs, and governs everything up to the next heading
at the same or a higher level.

## Where these rules come from

The tier rules are informed by ASD-STE100 Simplified Technical English, the controlled language used for aerospace
maintenance documentation, and by the plain-language principles of ISO 24495-1. The debt is real and the influence is
direct, particularly on the procedural tier.

Both constrain how they may be quoted, so what we cite, what we learned from and what we do not claim is recorded in
[lineage](lineage.md#language).
