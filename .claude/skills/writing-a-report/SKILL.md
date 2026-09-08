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

1. Run `kac report <name>` from inside the corpus, with `dotnet run --project ../../tooling/kac -- report <name>`.
2. Copy [`_template.md`](../../../template/reports/_template.md) to a filename naming the question, and paste the
   output under its frontmatter. Keep the `## Limits` section: a reader meets the numbers without the command beside
   them.
3. Fill `id`, `owner` and `status`. `generated` and `sources` arrive filled in.
4. Answer every `Verdict` cell, and every `Note` that earns one.
5. Add a `confirmed` entry naming the person who read it. Ask them; do not write a name they have not given you.
6. Run `kac validate`, then `kac generate`.

**A verdict you cannot reach is a question, not a blank.** Ask whoever owns the area. A report published with an empty
cell says the corpus was read and it was not.

## Bringing one up to date

A report is wrong the moment the corpus moves, and `report-stale` says so once `sources` falls behind.

**Two ways forward, and the corpus decides which.** Where nothing that moved touches this report, raise the
`sources` version by hand and add a `confirmed` entry. That says somebody checked. Where the coverage itself moved,
run the report again and merge.

**The merge, in order:**

1. Run `kac report <name>` and keep the output beside the record.
2. Key both by the row's first cell, within the section that heads it. A clause key is unique inside its policy and a
   framework reference inside its framework.
3. Carry the `Verdict` and `Note` forward wherever the mechanical cells are unchanged.
4. Ask about every row whose mechanical cells moved. A clause that gained a covering standard is now `Covered` and its
   note may have gone stale. A clause that lost one needs a fresh verdict.
5. Carry forward every section the tool does not write. It writes `Limits`, `Totals`, the rows, and
   `What this leaves open`, and anything else on the page is yours.
6. Take the new `generated` and `sources` whole, and add a `confirmed` entry.

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
