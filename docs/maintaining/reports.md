# Adding a report

This page is for whoever adds a report to `kac report`. A report answers a question about a corpus that no single record
can, such as which clauses nothing implements. A corpus is one repository of knowledge records kept in git.

## The steps

1. Ask what fact in the corpus answers it. A report is a walk over frontmatter and the link graph. Where the answer is
   written down nowhere, the corpus is missing a field, not a report.
2. Decide what the tool must not say. Write that down before the code, because it is the boundary the output has to
   state in its own text.
3. Fill every column the corpus states, and leave the judgement columns blank.
4. State the limits in the report itself. A reader meets the output without this page beside it.

## Why a report is a record

A finished report is a record in the corpus, under the `reports` type. It has an owner, somebody has verified it before
it is published, and a reader browsing the corpus finds it beside everything else. A file generated on demand and
thrown away costs every reader the run and the judgement again. The price of a record is staleness, which the
`sources` versions and `report-stale` keep visible.

## Where to go next

[`report`](../use/cli/report.md) says what a run prints and the frontmatter it stamps.
