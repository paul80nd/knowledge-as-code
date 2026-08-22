---
name: kb-review
description: Review knowledge records against the corpus authoring rules and propose rewrites in clearer, tighter language. Use when asked to review, tidy, simplify, shorten or improve the wording of one or more records, a type folder, or the corpus as a whole — for example "review the runbooks for clarity", "these policies read as verbose", "tidy up services/", "apply the authoring rules to the discoveries". Do not use for placement questions (which type a document belongs to) or for schema and validation failures.
---

# Reviewing a knowledge record

You are reviewing existing records against this corpus's authoring rules and proposing rewrites. You are not adding
knowledge, not correcting facts, and not moving documents between types.

## What outranks what

Three sources of rules, in this order:

1. **The schema and the validator.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report. These are the
   authority on anything mechanical: required sections, clause modals, id and filename formats, link forms, and which
   text rules a type declares. The type's `rules:` block is the wider of the two — it carries rules the tool does not
   implement, which bind an author even though nothing fails.
2. **The type's own pages.** `<type>.md` for what the type is meant to hold, `<type>/_template.md` for the sections it
   must have.
3. **[`knowledge-as-code/contributing.md`](../../../knowledge-as-code/contributing.md).** The link and template
   conventions CI enforces, and the same precedence list stated in full. Read it before you touch a link.
4. **The `technical-writing` and `writing-a-record` skills.** The prose rules: the floor for the words, which is the
   same everywhere, and the corpus voice for what a record's tier adds. Load both, every time. This skill is the
   procedure for applying them and does not restate them.

**Where a prose rule contradicts the schema, that is a finding to report — never an instruction to act on.**
`writing-a-record` says the same of itself. Report which of the two is wrong and leave both alone. A reviewer
resolving the contradiction instead breaks the build while claiming the rulebook's authority. Read literally, one
wrong bullet would have stripped **MUST** from twenty-two normative documents that `clause-modal` requires it in.

## The one rule that stops this going wrong

**Language follows tier, not type.** Read the record's `tier:` frontmatter before you read its prose, and apply that
tier's section of `writing-a-record`. A rewrite applying procedural terseness to an ADR has destroyed the document while
appearing to improve it.

## Never do these

* **Never rewrite the substance of a settled Decided-tier record** — an [ADR](../../../adrs.md) at `accepted`, a
  [postmortem](../../../postmortems.md) at `published`. The status field is the test, not the tier:
  `immutable-after-accepted` and `immutable-after-published` are written against it. You may fix a typo or a broken
  link. Everything else, including "tightening", is prohibited: to change a decision, a new one supersedes it. If a
  settled record genuinely reads badly, report it and stop. Before that point the record is under review and its wording
  is in scope — but the decision itself, and the weighing behind it, remain the author's.
* **Never edit between `<!-- BEGIN GENERATED -->` and `<!-- END GENERATED -->`.** Change the frontmatter or the schema
  and regenerate.
* **Never change an identifier.** Document ids, clause ids and link reference labels are referenced from elsewhere in
  the graph and from other corpora. A renamed clause id is a broken edge.
* **Never change meaning to save words.** Losing a qualifier, an exception or a scope boundary is a defect, not a
  saving. When a sentence is long because the obligation is genuinely conditional, leave it long.
* **Never change frontmatter** other than to correct a demonstrable error in the record in front of you, and say so
  explicitly if you do. Where the same error runs across a folder, or the field is one CI groups or resolves on, it is a
  migration rather than a review edit: report it as separate work and change nothing.
* **Never delete a section the type's `_template.md` requires**, even if it is thin. An empty required section is a
  content gap to report, not a formatting problem to fix.
* **Never edit a `_template.md` or a `<type>.md`.** A defect either of them caused is still theirs — see below.

## What the validator gives you, and what it does not

**Run `kac validate` first and expect it to come back clean.** CI gates the branch and pushes to `main` are rejected, so
a clean corpus is the normal state rather than the lucky one. It is your regression baseline: run it again at the end
and compare. It is not a source of findings, and it will not hand you a starting list.

**Then establish which text rules apply to the type in front of you.** `kac checks` lists what the validator implements,
across every type at once. `.schema/<type>.yaml`'s `rules:` block lists what your type declares. Read the second and use
the first to tell which of those rules actually run. Each text rule is declared on a single type: `low-ceremony` on
discoveries, `not-normative` on explanations, `symptoms-first` on runbooks. Most types declare none at all. For
`services/`, not one text rule fires. Do not report a check's findings as your own, and do not read the absence of a
check as permission: a declared rule with no code behind it binds you exactly as much as one that fails the build.

## What to look for

Ten categories, ordered by what they cost the reader. Use these names in the report; they are the category list.

1. **Not record content.** The highest-yield finding. A paragraph that would appear in more than one record of the type
   is corpus guidance in the wrong place. So is commentary about the record's own editorial choices, and any explanation
   of what a metadata field means. So is prose restating `owner`, `review-by`, `status` or `tags`, a review section
   duplicating frontmatter, and placeholder text describing the absence of a relationship. Say where the content belongs
   instead.
2. **Restated rather than cited.** Prose reproducing what an ADR, standard, type page or glossary entry already says.
   Replace it with a link.
3. **Duplicated across records.** Two records in the same folder carrying near-identical paragraphs. Flag both: one
   should hold it, or neither should.
4. **Aspiration.** Content describing what is intended rather than what exists. A record holds what exists today and the
   issue tracker holds the rest, so this is a correctness finding rather than a style one — flag it prominently. One
   exception: a schema rule the tool does not implement, where prose says the rule is declared and does not run.
5. **Tier violation.** Future tense in a descriptive record. Rationale inside a procedural step. A discovery written
   with the confidence of an FAQ. A condition trailing the action it guards in a runbook.
6. **Two ideas in one sentence.** The most common single defect. Usually a sentence over about 25 words, or a clause
   joined by *and* whose halves could each be failed independently.
7. **Filler.** Sentences that announce the next sentence. *It is important to note*, *in order to*, *simply*, *of
   course*. Adjectives carrying no information — *robust*, *seamless*, *comprehensive*.
8. **Inconsistent wording.** Two names for one thing, or one caveat worded three ways across sibling records. Elegant
   variation is a defect here.
9. **Template defect.** Below.
10. **Rulebook contradiction.** Above.

## Records that exist to demonstrate their type

Most of this repository's records are examples, and a type page will often say so — `services.md` says its estate is
there for "the awkward cases the schema was shaped by". That pulls against *not record content*, because the awkward
case and the lecture about it arrive in the same paragraph. Separate them:

* **Keep the awkward case, in full.** A `repo` field that under-answers. A bare `depends-on` on a service that depends
  on a great deal. A `critical` service depending on an `important` one. The fact is what makes the example worth
  shipping, and the demonstration is the **shape of the record**.
* **Cut the convention it restates.** Where the record goes on to explain the rule that makes the fact awkward, that
  text is already on the type page — often word for word. Nine copies is nine things to keep in step.
* **Cite the type page.** A link is how the reader gets the rule, and it costs one line.

## Template defects

**A defect the template or the type page caused belongs to that file, not to the records.** The test is causation rather
than tally. Name the line responsible and the defect is the template's, however few records carry it. Where you cannot,
it is a record defect however many do. `services/_template.md` tells authors that a consumers list "is maintained by
hand and will go stale. Say so", and the caveat duly appears in three records where *not record content* says it should
appear in none. An example in a template's frontmatter is the most contagious line of all, because every author copies
it.

Leave the records alone, and **fixing the template is out of scope for a review**. That holds in a `role: source`
repository as much as anywhere, because it changes every record of the type and every corpus downstream. Propose it as
separate work.

## When there is nothing to find

**"No findings" is a correct outcome**, and the likely one on prose that has been worked over before. A padded report is
worse than a short one: it spends the reader's attention on findings you did not believe in, and teaches them to
discount the ones you did.

For each candidate, name the category it falls in and the reader it costs. If you cannot do both, it is not a finding.
Where you are unsure whether a cut loses meaning, **do not make it** — list it under *Judgement calls* and let a human
decide. That list is worth more than a larger diff.

## Procedure

1. **Establish scope.** One record, a type folder, or a named set. If asked to review "the corpus", propose an order —
   by tier, worst offenders first — and confirm before starting. Do not silently review a hundred documents.
2. **Run `kac validate` and `kac checks`, and read the type's `rules:` block.** Baseline, what the tool enforces, and
   what the type declares.
3. **Load `technical-writing` and `writing-a-record`.** Then the type's `<type>.md` for what the type is meant to
   contain, and its `_template.md` for the sections it must have.
4. **For each record**, in this order:
   a. Read the frontmatter. Note the `tier`, and the `status` — whether a Decided record has settled. b. Read the record
   whole before changing anything. c. Identify findings against the floor, then against the tier's rules, then against
   `writing-a-record`'s *Keep the administration out*. d. **Edit at the
   scale the findings interact at** — the sentence where a finding is local, the section as a unit where several
   findings argue the same point across it, the whole document only where most of its paragraphs are in scope. What must
   be whole is the result: a reader arriving cold must not be able to tell which paragraph is newest.
5. **Then read the set.** Sibling records against each other: the same fact stated in two, one caveat worded three ways,
   two records that disagree outright. Nothing inside a single record shows these, and they are the findings a reader of
   the folder feels most. Where two records contradict each other on fact, report it and change neither — deciding which
   is true is the owner's.
6. **Walk the ten categories once each, over the whole set.** A reviewer settles into whichever category the first
   record rewarded and finishes feeling finished. Naming the ten in turn is what makes two runs over the same folder
   agree, and agreement is worth more than any single run's perceptiveness.
7. **Check what you produced** against the checklist below.
8. **Run `kac validate`** again and, if any frontmatter changed, `kac generate`. A rewrite that fails validation is not
   a rewrite.
9. **Report** in the shape given below. Propose; do not commit. Open a PR if asked — pushes to `main` are rejected.

## Before you hand it back

The validator checks these. Read its output rather than checking them by eye:

* Every internal link resolves, every shortcut reference has a definition, and every definition is used.
* Required sections are all present, and the identity line agrees with the frontmatter.
* Both generated-block markers survive.
* Clause ids are present, unique and correctly formed, and every `pol-XXXX.CLAUSE` citation still names a clause.

Nothing checks these. They are yours:

* **Every identifier unchanged.** The validator catches a rename that breaks a link inside this corpus; it cannot see a
  citation from another one.
* **Link form and definition order**, per [contributing](../../../knowledge-as-code/contributing.md#links).
  `unused-definition` and `undefined-label` fire; the ordering does not.
* **Prose wrapped at 120 columns**, tables and link definitions exempt. `.editorconfig` says so and no check enforces
  it. A line carrying an inline link still wraps — before the link, since only the URL itself cannot be broken, and only
  a definition whose URL is longer than the margin is genuinely exempt.
* **Nothing changed inside a generated block.** The validator checks that the markers survive, not the content between
  them.
* **Nothing added that was not in the original** — with one carve-out: **a link that replaces restated content is not an
  addition.** Swapping a paragraph of convention for `[Services](/services)` is the fix, not an expansion. A link from a
  record to a type page resolves and validates; `link-resolves` accepts every link form and the `.md` is optional.
* **Read the result cold.** If you cannot tell which paragraphs you changed, it is right.

## Reporting

**Report the findings and the judgement calls. Do not reproduce the documents.**

Where the rewrite is applied to the working tree, `git diff` is the artefact — say so, and let the reader read it.
Reproducing nine records at four hundred words each duplicates the corpus into a report that is stale the moment anyone
edits a file. The first rule here is *say less, once*. Reproduce a whole document only where nothing has been written to
disk.

For each record, keep it short:

**`<id>` — <what changed, in one line>.** Then the findings as a list, each naming its category and the rule it
breaches.

Then, separately, **Judgement calls**: the findings you did not act on, and why each was left.

Finish with the numbers, because they are the point: how many records, how many findings by category using the names
above, and the change in total length. If length went up, explain why — occasionally it should, when a record was terse
because it was incomplete.
