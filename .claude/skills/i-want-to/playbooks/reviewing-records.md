### Reviewing records

You are reading existing records against the rules and proposing rewrites. You are not adding knowledge, correcting
facts, or moving documents between types.

**The prose rules live in the skills.** `technical-writing` is the floor and `writing-a-record` is the corpus voice and
the tier rules. This page is the procedure for applying them and restates none of them.

## What outranks what

1. **The schema and the validator.** `.schema/*.yaml`, and what `kac validate` and `kac checks` report. The authority on
   anything mechanical. A type's `rules:` block is the wider of the two, because it carries rules the tool does not
   implement, which bind an author even though nothing fails.
2. **The type's own pages.** `<type>.md` for what the type holds, `<type>/_template.md` for the sections it must have.
3. **[`contributing.md`](../../../../example/knowledge-as-code/contributing.md)**, for the link and template conventions
   CI enforces.
4. **`technical-writing`, then `writing-a-record`.** Load both, every time.

**Where a prose rule contradicts the schema, that is a finding to report and never an instruction to act on.** Report
which of the two is wrong and change neither. Read literally, one wrong bullet would have stripped **MUST** from
twenty-two documents that `clause-modal` requires it in.

## Never

* **Never rewrite the substance of a settled Decided record.** An ADR at `accepted`, a postmortem at `published`. The
  `status` field is the test rather than the tier. Fix a typo or a broken link; report anything else and stop.
* **Never edit between the generated markers.** Change the frontmatter or the schema, then regenerate.
* **Never change an identifier.** A document id, a clause id or a link label is cited from elsewhere in the graph and
  from other corpora. The validator cannot see a citation from outside this one.
* **Never change meaning to save words.** A lost qualifier, exception or scope boundary is a defect. Where a sentence is
  long because the obligation is genuinely conditional, leave it long.
* **Never change frontmatter**, beyond a demonstrable error in the record in front of you, and say so where you do. The
  same error across a folder is a migration to report rather than a review edit.
* **Never delete a required section.** An empty one is a content gap to report.
* **Never edit a `_template.md` or a `<type>.md`.** See *Template defects* below. Where the request is *about* one of
  those files rather than about the records, this is the wrong playbook: run
  [sweeping-prose](sweeping-prose.md), whose step 8 is the only one covering them.

## The procedure

1. **Establish scope, and count what is in it.** One record, a folder, or a named set. Asked to review "the corpus",
   propose an order and confirm it. Do not silently review a hundred documents. **A type folder here usually holds
   nothing.** Most hold no record at all, so a folder named in a request may have only its `_index.md` and
   `_template.md`, both of which the never-list puts out of reach. Say so, and say what the candidates are: where the
   request meant the type page or the template, it belongs to
   [sweeping-prose](sweeping-prose.md) rather than here. **Test the claim the request makes.** "These are verbose" and
   "there are semicolons everywhere" are counts, and a count that comes back small or empty is the answer rather than
   the start of one.
2. **Run `kac validate` and expect it clean.** CI gates the branch, so a clean corpus is the normal state. It is your
   regression baseline rather than a source of findings, and it hands you no starting list.
3. **Establish which text rules reach this type.** `kac checks` lists what the validator implements; the type's `rules:`
   block lists what the type declares. A declared rule with no code behind it binds you exactly as much as one that
   fails the build. Most types declare none, and for `services/` not one fires. **A rule can bind and appear in
   neither.** The prose rules live only in the skills, so a mark the floor governs is invisible to `kac checks` and to
   the schema. Read the floor before concluding that nothing reaches this type.
4. **Load `technical-writing` and `writing-a-record`**, then the type's root page and its template.
5. **Per record:** read the frontmatter for `tier` and `status`, then read the record whole before changing anything.
   Edit at the scale the findings interact at. What must be whole is the result, so a reader arriving cold cannot tell
   which paragraph is newest.
6. **Then read the set.** Siblings against each other: one fact in two records, one caveat worded three ways, two
   records that disagree. Nothing inside a single record shows these. Where two contradict on fact, report it and change
   neither.
7. **Walk the categories below once each, over the whole set.** A reviewer settles into whichever category the first
   record rewarded and finishes feeling finished. Naming them in turn is what makes two runs agree.
8. **Check the second tree.** `template/` holds its own copy of every type page and template. Those are `seed`, so
   `kac update --check` does not hold them equal and nothing catches drift. A record under `example/` has no twin.
9. **Run `kac validate` again**, and `kac generate` where any frontmatter changed.
10. **Propose rather than commit.** Run [opening-a-pull-request](opening-a-pull-request.md) only if asked.

## What to look for

The floor and the voice carry the sentence-level rules. These are the findings a review adds, ordered by what they cost
a reader.

1. **Not record content.** The highest-yield finding. A paragraph that would appear unchanged in a second record of the
   type is corpus guidance in the wrong place. So is commentary on the record's own editorial choices, an explanation of
   what a metadata field means, prose restating frontmatter, and placeholder text describing an absent relationship. Say
   where it belongs instead.
2. **Restated rather than cited.** Prose reproducing what an ADR, a standard, a type page or a glossary entry already
   says. Replace it with a link.
3. **Duplicated across records.** Two records in a folder carrying near-identical paragraphs. Flag both.
4. **Aspiration.** Content describing what is intended. This is a correctness finding rather than a style one, so flag
   it prominently. One exception: a schema rule the tool does not implement.
5. **Tier violation.** Read against the tier's section of `writing-a-record`, whose rules these are.
6. **Template defect.** Below.
7. **Rulebook contradiction.** Above.

## Records that exist to demonstrate their type

Most records here are examples, and a type page often says so. That pulls against *not record content*, because the
awkward case and the lecture about it arrive in one paragraph. Separate them.

**Keep the awkward case in full.** A `repo` field that under-answers, a bare `depends-on` on a service that depends on
plenty, a `critical` service depending on an `important` one. The fact is what makes the example worth shipping, and the
demonstration is the shape of the record.

**Cut the convention it restates, and cite the type page instead.** Nine copies is nine things to keep in step.

## Template defects

**A defect the template or the type page caused belongs to that file.** The test is causation rather than tally. Name
the line responsible and the defect is the template's, however few records carry it. Where you cannot, it is a record
defect however many do. An example in a template's frontmatter is the most contagious line of all.

Leave the records alone, and **fixing the template is out of scope for a review**. It changes every record of the type
and every corpus downstream. Propose it as separate work.

## When there is nothing to find

**"No findings" is a correct outcome**, and the likely one on prose worked over before. A padded report spends the
reader's attention on findings you did not believe in, and teaches them to discount the ones you did.

For each candidate, name the category and the reader it costs. If you cannot do both, it is not a finding. Where you are
unsure whether a cut loses meaning, do not make it: list it under *Judgement calls* and let a human decide.

**Reply:** per record, one line on what changed and the findings under the category names above. Then *Judgement calls*,
the findings you left and why. Then the numbers: how many records, how many findings by category, and the change in
length. Where the rewrite is on disk, `git diff` is the artefact, so do not reproduce the documents.
