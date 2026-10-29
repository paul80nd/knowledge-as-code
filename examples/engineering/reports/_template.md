---
id: rpt-{{slug}}
type: report
tier: descriptive
status: draft
owner:
generated:
sources:
verified:
---

# {{Title}}

`Report: rpt-{{slug}}` `DRAFT`

<!-- DELETE FROM HERE: guidance for whoever fills this in, not part of the document ----------------------------- -->

**Start with [contributing](../knowledge-as-code/contributing.md).** Below is only what a report adds to it. Add an
optional field from [the type page](../reports.md#metadata) where you have a value for it.

**Everything under the H1 comes from `kac report <name>`.** The name is the question the report answers, not this
record's id, and `generated.report` states the one this record came from. Run it, paste what it printed, and answer
the judgement cells it left open. Keep the section it prints about its own limits: a reader meets the numbers
without the command beside them.

**The four verdicts.** Every row takes exactly one. `kac report` prints `covered` and `uncovered` and stops, because
it cannot tell a rule nobody has got to from a rule about something this organisation does not have. Those are the
cells you answer.

| Verdict               | When                                                                    |
|-----------------------|-------------------------------------------------------------------------|
| `Covered`             | The `Covered by` column names something. Copy the verdict, not the ids. |
| `Covered by its pair` | The `Pair candidate` is covered, and the two state one obligation.      |
| `Gap`                 | Nothing covers it, and the thing it governs exists here.                |
| `Out of scope`        | Nothing covers it, and the thing it governs does not exist here.        |

Answer every judgement cell. A cell you cannot answer is a question for whoever owns the area, not a blank. A gap is a
fact rather than a task. A `Note` says what the verdict does not, and stays empty where the row already says it.

**Running it again.** Regenerating replaces the content. Carry forward every verdict whose row is unchanged, and
answer the rows that moved. The `writing-a-report` skill states the rest, including the merge that keeps your
judgement.

**Frontmatter**

Keep the slug to 30 characters at most: the filename without `.md` and without any number or mnemonic at its
head. Write each list field's entries in alphabetical order, except `sources`.

* **`id`**: `rpt-` plus a slug naming the question. Save the file as `<slug>.md`.
* **`generated`**: who wrote the content as it stands, and when. `kac report` writes itself into `by`, which is true
  while the output stands unedited. Write yourself there once you answer a judgement cell: a person is
  `human:alex.doe`, and an agent states its version, as `coverage-sweep/1.2.0`. `report` states the report the run
  used, as `coverage`, and `tool` keeps the tool that wrote the mechanical half, as `kac/{{0.24.0}}`.
* **`sources`**: one entry per corpus the run read, naming the corpus and the `content-version` it was at. This corpus
  comes first, then one entry per corpus it imports. Raise a version by hand where a corpus moved and nothing in this
  report changed, and leave `generated` alone. A raise edits the frontmatter alone, so `verified` stays as it is.
  Where this corpus exports reports, raising an imported entry also moves its `content-version` by a patch. Raise the
  first entry to that new version.
* **`verified`**: who has checked this report, one entry per actor, oldest first. A draft states none, and every
  other status states one. Write a person as `human:alex.doe`, a process as `process:nightly-sweep`, or an agent with
  its version as `coverage-sweep/1.2.0`. Write an actor once: a second check by the same actor moves the `at` on the
  entry they have. Two actors are refused: a `role:`, because a post cannot read an answer, and the actor in
  `generated.by`, because nobody signs off their own writing. `no-self-verification` reports the actor in
  `generated.by`. Leave the status at `draft` until somebody else has read the report. They add the entry naming
  themselves and set the status.

<!-- DELETE TO HERE ---------------------------------------------------------------------------------------------- -->

## Limits

What this run could see, and what it could not. `kac report` writes this section.

## {{Totals}}

The counts, as the command printed them.

## {{The rows}}

One row per thing counted, with the judgement cells answered.

## What this leaves open

What the tool declined to decide, and what whoever verified this decided instead.
