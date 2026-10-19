---
name: writing-a-report
description: Turn `kac report <name>` into a published report record, and bring an existing one up to date. Load it whenever somebody asks for a coverage report, a framework report, or any answer about what the corpus covers, and whenever a report record has to be regenerated after the corpus moved.
---

# Writing a report

Load `technical-writing`, then `writing-a-record`, before you write a word of the record. This page carries only what
a report adds to them.

**The tool states facts and you state judgement.** `kac report` fills every cell the corpus can answer and leaves the
rest blank. It prints `covered` and `uncovered` and stops, because it cannot tell a clause nobody has got to from a
clause about something this organisation does not have. Those are the cells you fill.

**The name is the question, not the record.** `rpt-clause-coverage` is the record and `coverage` is the run behind it,
so `generated.report` states which report regenerates this one. Where a record states none, run `kac report` with any
name: it refuses one it does not have and lists what it takes.

## The verdicts

Four words, and every row takes exactly one.

| Verdict               | When                                                                      |
|-----------------------|---------------------------------------------------------------------------|
| `Covered`             | The `Covered by` column names something. Copy the verdict, not the ids.   |
| `Covered by its pair` | The `Pair candidate` is covered, and the two state one obligation.        |
| `Gap`                 | Nothing covers it, and the thing it governs exists here.                  |
| `Out of scope`        | Nothing covers it, and the thing it governs does not exist here.          |

**A gap is a fact, not a task.** The row says `Gap` about today. Where one is worth closing, that is an issue with
somebody's name on it, and the row still says `Gap` the day the issue opens.

**Read `Pair candidate` before you write `Gap`.** The tool offers the match and you decide. Two policies reaching for
one word by coincidence is the other reading, and only a person can tell them apart.

**A `Note` says what the verdict does not.** Why this is out of scope. What a gap would take. Which side of a pair the
coverage sits on. Where the verdict is obvious from the row, leave the cell empty rather than restating it.

## Writing the first one

1. Run `kac report <name>` from inside the corpus.
2. Copy `reports/_template.md` to a filename naming the question, and paste the output under its frontmatter. Keep the
   `## Limits` section: a reader meets the numbers without the command beside them.
3. Fill `id` and `owner`. `generated`, `sources` and `status: draft` arrive filled in.
4. Answer every `Verdict` cell, and every `Note` that earns one.
5. Write yourself into `generated.by`, and set `generated.at` to the moment you finished. The run wrote the table and
   you wrote the verdicts, so the content is yours now. Leave `report` and `tool` as the run left them.
6. Leave `verified` empty and leave the status at `draft`. Nobody has read this yet.
7. Run `kac validate`, then `kac generate`.

**A verdict you cannot reach is a question, not a blank.** Ask whoever owns the area. A report published with an empty
cell says the corpus was read and it was not.

**Somebody else moves it off `draft`.** They read what you wrote, add a `verified` entry naming themselves, and set
the status. An actor is written once, so somebody reading it again moves the `at` on the entry they have. A person is
`human:alex.doe`, a process is `process:nightly-sweep`, and an agent states its version, as `coverage-sweep/1.2.0`.
Two actors are refused: a `role:`, and the actor in `generated.by`.

## Bringing one up to date

A report is wrong the moment the corpus moves, and `report-stale` says so once `sources` falls behind.

**Two ways forward, and the mechanical cells decide which.** Run `kac report` and compare every mechanical cell with
the record. Where all of them match, raise the `sources` version by hand. Where any moved, merge as below and write
yourself into `generated`, even where every verdict carries forward. A cell can move because a record the report only
lists changed, such as a deviation or a control.

**A hand-raise leaves `generated` alone.** It applies only where every mechanical cell matched. `generated.by` states
who answered the judgement cells, and a raise answers none of them. So `generated` goes on stating an edit older than
the version `sources` now lists.

**Which entry you raise decides what else you edit.** `sources` lists the corpus the report answers for first, then
one entry per corpus it imports. `kac report` writes the imported entries into the `Imported:` bullet under
`## Limits`, and nothing checks that bullet against the frontmatter. So raising an imported entry takes three more
edits:

1. Edit the bullet to the same version by hand.
2. Where your corpus exports reports, move its `content-version` by a patch. The raise changed one of its records.
3. Raise the first entry to the version you just set.

**Sign the raise that changed the body.** Add a `verified` entry stating who checked the comparison.
`no-self-verification` rejects the actor named in `generated.by`, so ask somebody else to check it and write their
name. Raising the first entry edits frontmatter alone: the prose is exactly what the last verifier read, and the list
stays as it is.

**The merge, in order:**

1. Run `kac report <name>` and keep the output beside the record.
2. Key both by the row's first cell, within the section that heads it. A clause key is unique inside its policy and a
   framework reference inside its framework.
3. Carry the `Verdict` and `Note` forward wherever the mechanical cells are unchanged.
4. Ask about every row whose mechanical cells moved. A clause that gained a covering standard is now `Covered` and its
   note may have gone stale. A clause that lost one needs a fresh verdict.
5. Carry forward every section the tool does not write. It writes `Limits`, `Totals`, the rows, and
   `What this leaves open`, and anything else on the page is yours.
6. Take the new `sources` whole, and take `report` and `tool` from the run. Write yourself into `generated.by` with
   the moment you finished the merge.

**Mark nothing as carried forward.** The judgement lives in the cell, so a merge is a read of two documents, and a
marker would be a second thing to keep in step.

## What the report is not

**It is not a compliance claim.** No column says a clause is verified. A control names a standard rather than a rule,
so it vouches for a whole document whatever it checks inside it. The `Limits` section says this and stays.

**It answers for this corpus alone.** A clause uncovered here may be covered in a corpus consuming this one.
