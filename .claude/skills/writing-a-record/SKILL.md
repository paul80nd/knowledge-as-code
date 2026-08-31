---
name: writing-a-record
description: How to write inside a knowledge corpus. Covers a record of any tier, a type root page, the framework pages under `knowledge-as-code/`, and the schema's `description:` and `notes:` values. Load it after `technical-writing` whenever you write or change any prose a corpus holds.
---

# Writing a record

Load `technical-writing` first. Everything below either adds to it or says plainly which of its rules it overrides.

**Tier decides how a record is written, and type does not.** A runbook and a process are read under different conditions
and answer to different rules. An ADR and a discovery are not long and short versions of one thing. The frontmatter
carries `tier`, so read that row of the table below and leave the other four alone.

**A record holds the thing being governed, not information about it.** Ownership, lifecycle, relationships and alignment
are frontmatter. Prose restating any of them goes wrong before the frontmatter does.

## What this overrides

* **"Write in the present tense."** A postmortem records what happened, so its timeline and its account of the incident
  are past tense. A Decided record's consequences have not happened yet, so a consequence takes the future and the
  mechanism producing it stays present. Write: "**CI will become a gate on documentation.** A malformed document fails
  the build."
  Whatever either record says about the estate today stays present.
* **"They do not reach a clause whose wording is the record of an obligation."** That exemption covers the words and not
  the marks. Repair a clause's punctuation, and keep every word carrying obligation: a citation quotes the row, so
  dropping a qualifier changes what `pol-DEVI.OWNER` says to whoever cites it. The author accepts that diff in review,
  which is where a meaning that moved is caught. A capital the new mark forces is not a word change:
  `— see [pol-SCRT]` becomes `. See [pol-SCRT]`. **Inside a clause row this beats every other rule, on this page and on
  the floor.** A passive, an elided verb, a compound obligation, and a rationale the tier row would send to Purpose all
  stay, because each is wording a citation quotes. Report them and leave them. The tier row governs a clause you are
  writing, and this governs one you are editing.
* **"Gloss a precise term on first use."** The corpus has a glossary, and a term it defines is defined once. Link the
  entry rather than restating it.
* **Nothing else.** Where the floor and this page appear to disagree anywhere below, the floor wins.

## What outranks this page

**The schema is executable and this page is not.** Where the two disagree, the schema is right. Report the contradiction
rather than editing records to match this.

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

## Fill the frontmatter first

**Quote every date.** YAML reads unquoted `2026-06-12` as a datetime, and renders it with a locale format and a timezone
shift. `"2026-06-12"` renders as written.

**Enum values are lower-case and hyphenated.** They are grep targets first and prose second.

**A list is a block sequence, one entry per line.** An entry stays individually reviewable in a diff, and a finding can
point at the entry that caused it rather than at the field.

**Two things send a list to the compact flow form, and either is enough.**

* **The field is secondary metadata.** `tags: [ a, b ]` says how a record is found rather than what it says, and a block
  sequence gives the least interesting field in the block the most lines.
* **The entries are short and there are many of them.** A standard's `implements:` puts a line per clause between the
  reader and the document, and thirteen clauses of `pol-AUTV` is thirteen lines. The `clauses:` inside a policy's
  `aligns-with` entry is a run of framework references under a framework already named on the line above.

Everything else stays a block sequence: a handful of entries, or entries a reviewer weighs one at a time.

Nothing enforces either form, so this is the house style rather than a rule. A corpus taking the framework writes
whichever valid YAML it prefers.

**A list of objects names each entry on its first key.** `aligns-with` writes the framework, then the references reached
inside it. The naming key comes first because the tool sorts on it and an index column renders it.

```yaml
aligns-with:
  - framework: ISO 27001:2022
    clauses: [ A.5.17, A.8.24 ]
```

It carries the frameworks that bind, which `frameworks.md` files under **Obliged** or **Self-obligated**. A clause may
cite a framework filed under **Inspiration** to say where an idea came from, and that citation stays out of the
roll-up: the generated index is read as a coverage table, and a framework binding nothing dilutes it.

**A list reads alphabetically.** No list field's sequence carries meaning, so alphabetical is the order that scan-reads
and the one two authors agree on without discussion. Numbers inside an entry compare as numbers, so `A.8.7` comes
before `A.8.29`.

**A tag is an entry point, and never a grouping.** It is the word a reader arrives with, on a record that does not use
it. One record may be the only one carrying a tag, and often is. A tag must never restate another field, since the two
can only ever disagree. Where a value divides a type into groups worth browsing, the type declares a list field of its
own for it.

**The identity line carries three facts, directly beneath the H1**: the type, the id, then the status in upper case.

```markdown
# Software we build is usable by everyone

`Policy: pol-A11Y` `DRAFT`
```

You arrive at a record from a citation, so the top of the page answers what kind of record this is, which one it is, and
whether it is in force, before the prose starts. Frontmatter answers all three and is written for a machine. The id
appears exactly as the frontmatter carries it. The status is the exception: lower-case in frontmatter because a machine
reads it, and upper-case on the line because a person reads it as a stamp.

**The H1 is the title and nothing else**: no id, no prefix, no type name. The identity line carries the handle instead.
A title competing with a handle is a worse title, and a generated index would have to strip the id back off to fill a
column that already held it.

## Link rather than restate

**Reference another record by its id, with a shortcut reference link.** The label is the id and doubles as the display
text, so a path appears once per document and a rename is a one-line change.

    New headers are governed by [adr-0013].

    [adr-0013]: 0013-http-custom-header-naming.md

**Name the part when you reference one**, as `<id>.<part>`, which is the form a citation already takes. A clause of a
policy and a term of a glossary are both parts. The label carries it and the target lands on it.

    A title in the catalogue is not the indexed field. See [gls-search.title].

    [gls-search.title]: search.md#title

Linking to the file instead lands a reader at the top of the document to find the part themselves. It also loses the
reference: a tool reading the corpus carries what the link states, and a link naming no part states none.

**Where a type gives its parts no anchor of their own, link the record and write the part outside the label.** Every
part would resolve to one target, so a document citing six of them carries six definitions landing in the same place.
A policy's clauses are rows of a single table under `## Clauses`. The anchor reaches that table and the row's `Id`
column is what a reader scans to, so a clause is cited this way:

    _**Covers:** [pol-AUTV].INTEG, [pol-AUTV].BLOCK_

    [pol-AUTV]: ../../policies/delivery/autv-automated-verification.md#clauses

The part stays on the page for a reader. The link states the record alone, because the record is as far as the
anchors reach. Two clauses of one policy reuse the definition, which is what this form is for.

Where in a document a clause is cited is the type's business rather than this page's. A standard names its clauses on
the `Covers` line closing each rule, and its `_template.md` says so.

**The label is the id exactly as that record carries it**: `adr-0013`, `pol-DEVI`, `svc-billing-api`. The prefix is
always lower-case, and what follows takes the type's own form, so a mnemonic stays upper-case and a slug stays
lower-case. A part id is the record's own id, a dot, and the part as its type writes one: `pol-DEVI.TIMEBOX`,
`gls-search.title`. The label is its own display text, so a label that is not the id shows the reader an id that does
not exist. `label-canonical` is the check, and it matters because Markdown matches a reference to its definition
case-insensitively: `[ADR-0013]` resolves perfectly happily, and nothing else would ever catch it.

**Use an inline link where the display text is prose**, since it differs from the target.

    The rule lives in the [value-formats standard](../standards/public-api/value-formats.md).

**Definitions go at the very foot of the document**, after every prose section, sorted by label. A `## Related` section
uses the same shortcut labels. An undefined label and an unused definition both fail, and a fenced or indented block is
read as code rather than as links.

**A framework page names a type and never links to one.** That rule reaches `knowledge-as-code.md` and the documents
beneath it, and nothing else. Every corpus holds the same copy of those files, and a corpus that never adopted standards
has no `/standards` page to open. A link into a type's folder is worse again, because the records it points at are the
first thing a corpus deletes. Where a link genuinely belongs there, put it in a generated block: `kac`
writes those from the types the corpus adopted, so they can only name pages that exist. `framework-names-types` holds
you to this.

**Everywhere else, naming a type is not linking to one.** A record may say a discovery becomes an FAQ where the corpus
adopted neither. Those are the ordinary nouns, and naming one promises no page. What adoption governs is the link. A
corpus that never adopted standards has no `standards.md` to open, so name the type and leave the label off.

## Write to the tier

| Tier            | Written as                                                                     |
|-----------------|--------------------------------------------------------------------------------|
| **Procedural**  | One action per step, imperative, condition before action. No reason in a step. |
| **Normative**   | One obligation per clause, testable, keyword leading. Reason lives in Purpose. |
| **Descriptive** | Indicative, present tense. State what is.                                      |
| **Decided**     | Prose earns its keep. Constrain structure and length, not vocabulary.          |
| **Observed**    | Symptom, cause, why it might matter. Three sentences beat three paragraphs.    |

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
* **A runbook opens with Symptoms**, because that is how a reader who does not yet know which record they need finds it.
  `symptoms-first` is an error rather than a warning.

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
* **Purpose does not preview the clauses.** A sentence there a reader could re-read as an obligation belongs in the
  table, where a citation can reach it. Purpose states the position and earns it, and the table is what binds. Cut from
  a Purpose: "We do not change it under its consumers without a version and reasonable notice."
* **One obligation per clause.** A clause that cannot be failed in exactly one way splits. `clause-order` and
  `clause-compound` warn where a row carries more than one.
* **A clause is testable, or it is a wish.** *Services are secure* is untestable. *Services read secrets from a managed
  vault* can be checked.
* **A clause states its obligation without help.** A cross-reference points at the other side of a shared obligation, or
  at the policy that owns it. It does not finish the sentence.
* **Write out every contraction in a clause.** A clause is quoted and acted on, and the formal register is what marks it
  as binding. Purpose and Scope are prose and take the contraction speech would use.
* **Do not hedge the commitment, and do state a risk as a risk.** A policy that hedges what it binds you to is not a
  policy. Naming what may follow from a departure is not hedging, and flattening it into a certainty makes the policy
  claim something untrue. Write: "a mistake in development risks becoming an incident in production."
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

* **Alternatives were genuinely weighed**, and the record shows the weighing. An option listed only to be dismissed in a
  clause was not considered.
* **Consequences include the unwelcome ones.** A consequences section holding only benefits is a sales document.
* **Attribute a cause to a system, not to a person.** In a postmortem that is the condition under which the next account
  gets written honestly.
* **Length is earned per point rather than per record.** Give each point the detail it needs, once.
* **A contraction belongs here where speech would use one.** Context and Alternatives are an account of what a team
  found and how it decided, so they read as a team talking. The decision sentence and the clauses do not.

### Observed

Capture stays nearly free, or it does not happen.

* **Symptom, cause where known, why it might matter.** Nothing else is required, and `low-ceremony` warns past two
  hundred words. A discovery long enough to need structure has become something else, and is promoted rather than
  padded.
* **Say what you do not know.** *Unconfirmed*, *seen once* and *may be specific to this branch* are honest here, and are
  what the confidence level carries.
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

**`{{…}}` marks everything the author supplies, and nothing else does**: not `NNNN`, not `XXXX`, not a slug called
`example`. One mark, so a template teaches exactly what the tool recognises. The casing carries what a sentence would
otherwise have to: `pol-{{MNEM}}` in `{{mnem}}-kebab-slug.md` says a mnemonic is upper-case in the id and lower-case in
the filename. `{{a}}` and `{{b}}` stand for *another* record, and `{{a}}.md` is its whole filename.

**The mark has to survive YAML**, and in two places it does not.

* **A placeholder cannot sit in a flow sequence.** `related: [ adr-{{a}} ]` is a parse error, because a plain scalar in
  flow context may not contain a brace. Write the list as a block sequence.
* **A placeholder opening a value has to be quoted.** YAML reads `review-by: {{date}}` as a flow mapping rather than as
  text, so the field arrives holding nothing. Write `review-by: "{{date}}"`. A placeholder that follows something needs
  no quotes, as in `svc-{{slug}}`.

**A `{{placeholder}}`'s braces are the mark, and what sits inside them is prose.** `{{Stop the bleeding.}}` is a figure
dressing a plain instruction, and the tier's rules reach it.

**The guidance between `DELETE FROM HERE` and `DELETE TO HERE` is prose as well**, written for whoever fills the
template in. It answers to the floor, and to the tier the template's records carry.

---

**Markdown formatting.** Wrap prose at 120 columns. A table and a link definition are exempt, because a URL cannot be
broken. `.editorconfig` says so and no check enforces it.
