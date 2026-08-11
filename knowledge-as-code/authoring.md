# Authoring

How knowledge is **written** here. [Style](style.md) covers the words themselves, in every document this repository
holds. [Contributing](contributing.md) covers the mechanics — where a document goes, which template to copy, how it is
reviewed. This page covers what a record's tier asks of its prose on top of those.

Two rules carry most of the weight, and both are easier to state than to follow:

* **A record holds the thing being governed, not information about it.** Ownership, lifecycle, relationships and
  alignment are frontmatter. Prose that restates them is prose that will be wrong before the frontmatter is.
* **How a document is written follows its tier, not its type.** A runbook and a process are read under different
  conditions and obey different rules. An ADR and a discovery are not long and short versions of the same thing.

Both serve one end. A corpus is read under pressure, by people who did not write it and by agents that cannot ask what a
sentence meant. Length is not thoroughness. A document that says less, and says it once, is read.

Some of this is enforced, though less than you would hope and never uniformly. Each text rule is declared on a single
type — `low-ceremony` on discoveries, `not-normative` on explanations, `no-hedged-ordering` on processes — and many
types carry none at all. Run `./kac checks` to see what applies to the type in front of you rather than assuming, and
expect `./kac validate` to come back clean: CI gates the branch, so a clean run is the baseline rather than a source of
findings. The rest is judgement, and the absence of a check is not permission.

Where a rule here contradicts the schema, **the schema is right and this page is wrong**. It is executable and this is
not. Report the contradiction; do not resolve it by editing records.

## The floor

[Style](style.md) is the floor, and it binds every document in this repository: name who does what, anchor what you
write to something real, say it once and say it short, leave nothing to reconstruct, use the precise word and explain it
once, and write what stays true. Read it before this page.

What follows is what a corpus record asks on top of it.

## By tier

The floor applies throughout. Each tier adds to it.

| Tier            | Types                                                                     | Written as                                                                               |
|-----------------|---------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| **Procedural**  | processes, runbooks                                                       | One action per step, imperative, condition before action. No rationale in a step.        |
| **Normative**   | policies, standards, controls, nfrs, faqs                                 | One obligation per clause, testable, keyword leading. Rationale lives in Purpose.        |
| **Descriptive** | services, capabilities, tools, integrations, data, glossary, explanations | Indicative, present tense. State what is. No future tense — a plan is not a description. |
| **Decided**     | adrs, postmortems                                                         | Prose earns its keep. Constrain structure and length, not vocabulary.                    |
| **Observed**    | discoveries                                                               | Symptom, cause, why it might matter. Three sentences beat three paragraphs.              |

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
* **No future tense.** A planned service is not a service. Say so plainly and mark it — see
  [below](#what-is-not-built-yet).
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

Capture must stay nearly free or it does not happen. A discovery that takes ten minutes to write is a discovery nobody
writes.

* **Symptom, cause if known, why it might matter.** Nothing else is required, and `low-ceremony` warns past two hundred
  words — a discovery long enough to need structure has become something else and should be promoted, not padded.
* **Say what you do not know.** *Unconfirmed*, *seen once*, *may be specific to this branch* — hedging is honest here
  and is what the confidence level exists to carry.
* **Do not tidy it into authority.** A discovery that reads like an FAQ will be trusted like one. Promotion is where
  certainty is added.

## Intent, not administration

A record holds the knowledge. Information *about* the knowledge belongs in frontmatter, in generated blocks, or in git.

The test is repetition: **if a paragraph would appear in more than one record, it is not record content.** An
explanation of what `aligns-with` means, or of the difference between alignment and certification, is a corpus concern —
it belongs [here](metadata.md) and is written once. Repeated in every policy, it is twenty copies to keep in step and
twenty chances to be wrong.

Three consequences worth stating outright:

* **No review section.** `owner` and `review-by` are frontmatter, and the history is in git. A prose paragraph restating
  them is a copy, not a source.
* **No placeholder prose.** *No implementing standard exists yet* is a fact about the graph, not about the policy. An
  absent edge is absent; it does not need a sentence saying so.
* **No frontmatter restated in the body.** If the reader needs it rendered, that is the generator's job.

## What is not built yet

This framework documents where it is going as well as where it is. That is deliberate — the direction is the reminder of
what is being built — but a reader must never have to guess which they are reading.

**Unmarked prose describes what exists today.** Anything else carries a marker, on its own line, immediately below the
heading it governs, or leading the sentence it governs:

> **Planned.** Agreed direction, not yet built. An issue exists.

> **Aspirational.** Direction of travel. Neither agreed nor scoped.

A marker governs everything up to the next heading at the same or a higher level. Two rules keep it honest: a marked
section states what would have to be true for the marker to be removed, and **Planned** is not used without an issue to
point at. A marker that never comes off is a wish wearing a plan's clothes.

## Where these rules come from

The tier rules are informed by ASD-STE100 Simplified Technical English, the controlled language used for aerospace
maintenance documentation, and by the plain-language principles of ISO 24495-1. The debt is real and the influence is
direct, particularly on the procedural tier.

Both constrain how they may be quoted, so what we cite, what we learned from and what we do not claim is recorded in
[lineage](lineage.md#language).
