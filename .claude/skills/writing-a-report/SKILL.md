---
name: writing-a-report
description: Turn `kac report <name>` into a published report record, and bring an existing one up to date. Load it whenever somebody asks for a coverage report, a framework report, or any answer about what the corpus covers, and whenever a report record has to be regenerated after the corpus moved. It carries the verdict vocabulary the tool refuses to print, and the merge that keeps a person's judgement across a regeneration.
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

**Never teach the tool your words.** `Gap` and `Out of scope` are verdicts about one estate at one moment. A tool
printing either would be asserting something no record supports.

## The verdicts

Four words, and every row takes exactly one.

| Verdict               | When                                                                      |
|-----------------------|---------------------------------------------------------------------------|
| `Covered`             | The `Covered by` column names something. Copy the verdict, not the ids.   |
| `Covered by its pair` | The `Pair candidate` is covered, and the two state one obligation.        |
| `Gap`                 | Nothing covers it, and the thing it governs exists here.                  |
| `Out of scope`        | Nothing covers it, and the thing it governs does not exist here.          |

**A gap is a fact rather than a task.** Some are worth closing and some are what the organisation costs. Saying so is
the report's job; deciding what to do about it is not.

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
6. Leave `verified` empty and leave the status at `draft`. Nobody has read this yet, and an entry written to fill the
   field is worse than the blank it replaced.
7. Run `kac validate`, then `kac generate`.

**A verdict you cannot reach is a question, not a blank.** Ask whoever owns the area. A report published with an empty
cell says the corpus was read and it was not.

**Somebody else moves it off `draft`.** They read what you wrote, add a `verified` entry naming themselves, and set the
status. A person is `human:alex.doe`, a process is `process:nightly-sweep`, and an agent states its version, as
`coverage-sweep/1.2.0`. Two actors are refused: a `role:`, and the actor in `generated.by`.

## Bringing one up to date

A report is wrong the moment the corpus moves, and `report-stale` says so once `sources` falls behind.

**Two ways forward, and the corpus decides which.** Where nothing that moved touches this report, run `kac report`
and check that every mechanical cell still matches the record. Then raise the `sources` version by hand. Where the
coverage itself moved, run the report again, merge, and write yourself into `generated`.

**A hand-raise leaves `generated` alone.** `generated.by` states who answered the judgement cells, and a raise answers
none of them. So `generated` goes on stating an edit older than the version `sources` now lists. The alternative is
running the report again for output nobody expects to differ, and moving a version stamp through every consumer of the
corpus.

**Sign a raise that changes the prose.** `kac report` writes each imported corpus and its version into the `Imported:`
bullet under `## Limits`. Raising `sources` on a report with that bullet edits the bullet too, so add a `verified`
entry stating who checked the comparison. `no-self-verification` rejects the actor named in `generated.by`. Where that
is you, ask the report's owner to check it, and write their name.

**A report that imports nothing keeps its `verified` list.** The raise changes frontmatter alone. The prose is exactly
what the last verifier read, so nothing is asked of the list.

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

**Nothing marks a cell as carried forward.** The judgement lives in the cell, so a merge is a read of two documents
rather than a splice of one into the other. A marker would be a second thing to keep in step and the first thing to
rot.

## What the report is not

**It is not a task list.** A row saying `Gap` is a statement about today. Where a gap is worth closing, that is an
issue with somebody's name on it, and the row still says `Gap` the day the issue opens.

**It is not a compliance claim.** No column says a clause is verified. A control names a standard rather than a rule,
so it vouches for a whole document whatever it checks inside it. The `Limits` section says this and stays.

**It answers for this corpus alone.** A clause uncovered here may well be covered in a corpus consuming this one, and
every consumer answers for its own coverage.
