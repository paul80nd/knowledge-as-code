---
name: writing-a-record
description: The shape of what a corpus holds. Covers a record of any tier, a type root page, the framework pages under `knowledge-as-code/`, a `_template.md`, and the schema's `description:` and `notes:` values. Load it after `technical-writing` whenever you write or change any prose in a corpus.
---

# Writing a record

Load `technical-writing` first. This page adds the shape of a record. The floor wins where the two disagree, except
where this page says it overrides the floor.

## The tier decides how a record is written

Read `tier` in the frontmatter. Find its row below and follow that section. Ignore the other four.

| Tier            | Write it as                                                                              |
|-----------------|------------------------------------------------------------------------------------------|
| **Procedural**  | One action per step. Imperative. Condition before the action. No reason inside a step.   |
| **Normative**   | One obligation per clause. Testable. Keyword first. The reason goes in Purpose.          |
| **Descriptive** | Indicative, present tense. State what is.                                                |
| **Decided**     | Prose. Constrain the structure and the length, not the words.                            |
| **Observed**    | Symptom, cause, why it might matter. Three sentences.                                    |

### Procedural: runbooks and processes

The reader is under pressure and may be new to the system. They skim.

* Write one action per step. A step containing "and" is usually two steps.
* Put the condition before the action. Write: "If the queue depth exceeds 1000, restart the consumer."
* State the outcome of each step, so the reader can tell it worked.
* Put a warning before the step it guards.
* Do not hedge an order. Cut "typically", "usually" and "normally". If the order depends on something, say what.
  `no-hedged-ordering` warns on this in a process.
* Open a runbook with Symptoms. That is how a reader finds the runbook they need. `symptoms-first` is an error.

### Normative: policies, standards, controls, NFRs and fixes

The reader acts on a clause without checking it. Make it checkable.

* Start each clause of a standard with an RFC 2119 keyword in capitals. Lower-case "must" is prose.
* Put a policy's modals in the clause table only. Write Purpose and Scope as a commitment. `clause-modal` is an error.
* Purpose states the position and stops. It does not repeat a clause's threshold, list or exception.
* Write one obligation per clause. A clause you can fail in two ways is two clauses. `clause-order` and
  `clause-compound` warn.
* Make each clause testable. "Services read secrets from a managed vault" can be checked. "Services are secure"
  cannot.
* State the whole obligation in the clause. A cross-reference points at the other side of a shared obligation. It does
  not finish the sentence.
* Use no contractions in a clause. Purpose and Scope may use them.
* Do not hedge the commitment. Do name a risk as a risk. Write: "a mistake in development risks becoming an incident
  in production."
* Never claim a compliance posture in a policy. Certification and audit status belong in `frameworks.md`.
  `posture-belongs-to-frameworks` warns.
* A control, an NFR and a fix carry no clause table, no Purpose and no keywords. State what the reader will act on.

### Descriptive: services, capabilities, explanations and glossaries

* Use the indicative mood. Write: "The API authenticates with workload identity." Not: "should authenticate".
* Use no future tense. Describe what runs today.
* Use no promotion. Cut "world-class" and "best-in-class".
* Use no RFC 2119 keyword. `not-normative` warns.
* A capability and an explanation are hubs. Link out instead of restating. `hub-not-specification` and
  `links-rather-than-restates` warn at about forty words per outbound link.

### Decided: ADRs and postmortems

* Show that the alternatives were weighed. An option dismissed in one clause was not considered.
* Include the unwelcome consequences.
* Attribute a cause to a system, not to a person.
* Contractions are fine in Context and Alternatives. Not in the decision sentence or a consequence.

### Observed: discoveries

* Write the symptom, the cause if known, and why it might matter. `low-ceremony` warns past 200 words.
* Say what you do not know. "Unconfirmed" and "seen once" are honest here.
* Do not tidy a discovery into authority. Promotion is where certainty is added.

## What this page overrides in the floor

* **Present tense.** A postmortem's timeline is past tense. A Decided record's consequences may use "will", because
  they have not happened yet. Facts about the estate today stay in the present.
* **Gloss a term on first use.** The corpus has a glossary. Link the entry instead of restating it.
* **Clause wording.** You may repair punctuation inside a clause. You may not change a word, because a citation quotes
  the row. Report a passive or a compound obligation you find there, and leave it.

## What outranks this page

* **The schema.** Where the schema and this page disagree, the schema is right. Report the contradiction. Do not edit
  records to match this page.
* **`kac checks`** for the type you are writing. Each text rule is declared on one type. An absent check is not
  permission.
* **`kac generate`**, run last. The H1 and the frontmatter are copied into generated files. If you cannot run it,
  leave the H1 and the frontmatter as they are and say so.

## Keep the administration out

Information about a record lives in the frontmatter, a generated block, or git. The body holds the knowledge.

* No review section. `owner` and `review-by` are frontmatter. The history is in git.
* No placeholder prose. "No implementing standard exists yet" describes an absent edge. Say nothing.
* No frontmatter restated in the body. The generator renders it where a reader needs it.
* No unbuilt work. Send it to the issue tracker. One exception: a rule the type declares that nothing implements.
  Write it as declared and not running. Write: "`feature-file-orphans` is declared and does not run." Never claim an
  unbuilt check works. That is the commonest defect here.

## Frontmatter

* Quote every date: `"2026-06-12"`. Unquoted, YAML reads it as a datetime and renders it with a timezone shift.
* Write enum values lower-case and hyphenated.
* Write a list as a block sequence, one entry per line, so a finding can point at the entry. Two fields take the flow
  form `[ a, b ]` instead: `tags`, and a long list of short entries such as a standard's `implements:` or the `clauses`
  inside an `aligns-with` entry.
* Put the naming key first in a list of objects. The tool sorts on it.

  ```yaml
  aligns-with:
    - framework: ISO 27001:2022
      clauses: [ A.5.17, A.8.24 ]
  ```

* Name in `aligns-with` only a framework that `frameworks.md` files under **Obliged** or **Self-obligated**. A
  framework under **Inspiration** may be cited in a clause, and stays out of the roll-up.
* Sort every list alphabetically. Numbers compare as numbers, so `A.8.7` comes before `A.8.29`.
* A tag is a word a reader searches for. It never repeats another field.

## The H1 and the identity line

The H1 is the title and nothing else: no id, no prefix, no type name. The identity line sits directly under it and
carries the type, the id exactly as the frontmatter spells it, and the status in upper case.

```markdown
# Software we build is usable by everyone

`Policy: pol-A11Y` `DRAFT`
```

## Links

* Reference a record by its id, as a shortcut reference link. The label is the id and the display text.

      New headers are governed by [adr-0013].

      [adr-0013]: 0013-http-custom-header-naming.md

* Name the part when you reference one, as `<id>.<part>`.

      A title in the catalogue is not the indexed field. See [gls-search.title].

      [gls-search.title]: search.md#title

* Where a type's parts have no anchor of their own, link the record and write the part outside the label. A policy's
  clauses are rows of one table, so a clause is cited this way:

      _**Covers:** [pol-AUTV].INTEG, [pol-AUTV].BLOCK_

      [pol-AUTV]: ../../policies/delivery/autv-automated-verification.md#clauses

* Spell the label exactly as the record spells its id: `adr-0013`, `pol-DEVI`, `svc-billing-api`. `label-canonical`
  checks this. Markdown matches a label case-insensitively, so nothing else would catch `[ADR-0013]`.
* Use an inline link where the display text is prose.
* Put the definitions at the foot of the document, sorted by label. A `## Related` section uses the same labels. A
  fenced or indented block is not read for links.
* A framework page (`knowledge-as-code.md` and the pages under it) names a type and never links to one. A corpus that
  did not adopt the type has no page to open. `framework-names-types` checks this. A link that must exist goes in a
  generated block.

## A type root page

`<type>.md` has no frontmatter and no tier. Hold it to the floor. The generated block says what a field means. The
prose says what an author does about it. Cut prose that repeats the block.

## A `_template.md`

* `{{placeholder}}` marks what the author supplies, and nothing else does: not `NNNN`, not `XXXX`. The casing shows the
  form: `pol-{{MNEM}}` in `{{mnem}}-kebab-slug.md`.
* A placeholder cannot sit in a flow sequence. Write `related:` as a block sequence.
* Quote a placeholder that opens a value: `review-by: "{{date}}"`. Unquoted, YAML reads it as a mapping and the field
  arrives empty.
* The text inside the braces, and the guidance between `DELETE FROM HERE` and `DELETE TO HERE`, is prose. The floor
  and the tier apply to it.
