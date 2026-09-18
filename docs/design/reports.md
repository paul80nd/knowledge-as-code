# Reports

A report answers a question about the corpus that no single record can: which clauses nothing implements, which
framework references hang on one citation. [`kac report`](../cli/report.md) prints one as markdown.

This page is for deciding what a new report carries. It says where the line between the tool and its reader falls, why a
report is a record rather than a build artefact, and what the frontmatter must hold.

## What the tool states

`report` prints what the corpus states, and never a word it cannot justify from a record.

`coverage` is the worked case. A clause either has a standard whose `implements:` names it or it does not, so the tool
prints `covered` or `uncovered`. It cannot tell a gap worth closing from an obligation about something this estate does
not have. Both arrive as `uncovered`, and the reader splits them.

Drawing the line there is what makes the mechanical half reproducible. Two runs over one corpus print the same bytes, so
a diff is a change in the corpus rather than a change of mind.

### What is filled in

The judgement is expensive, so the tool spends nothing of it and everything else.

A coverage row arrives with the standards covering the clause, the deviations whose `departs-from:` names it, the
controls verifying those standards, and any clause elsewhere in the corpus sharing its key. Each of the four is a value
in frontmatter. A reader who had to gather them by hand would answer a handful of rows and abandon the rest.

The shared key is the softest of the four, and the tool offers it as a candidate. This corpus writes one obligation
stated from both sides as two clauses with one key, so a match is usually a pair. Two policies reaching for the same
word by coincidence is the other reading, and only a person can tell the two apart.

## Why a report is a record

A finished report is a record in the corpus, under the `reports` type. It has an owner, somebody has verified it before
it is published, and a reader browsing the corpus finds it beside everything else.

The alternative is a file nobody keeps: generated on demand, read once, thrown away. That costs every reader the run and
the judgement, and the argument written on top of the numbers gets written again each time.

Making it a record has a price, and the price is staleness. A record about the corpus is wrong the moment the corpus
moves, and the frontmatter is what keeps that visible.

## The provenance frontmatter

Three keys, following the
[Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md), a specification
for knowledge documents that a growing set of tools reads.

```yaml
generated:
  at: 2026-09-11T09:42:20Z
  by: coverage-sweep/1.0.0
  report: coverage
  tool: kac/0.25.0
sources:
  - { resource: example-dogfooding, version: "0.22.3" }
  - { resource: example-engineering, version: "0.16.0" }
verified:
  - { at: 2026-09-11T10:30:00Z, by: human:alex.doe }
```

`generated` gives who wrote the content as it stands, and when. It records one event, where `verified` keeps a list. A
regeneration replaces the content, so the moment before it describes a document that has gone. A verification is added
to the document instead, which is why every one of them is kept.

### Who `generated.by` names

`kac report` writes itself there, and that is true while the output stands unedited. A report is half mechanical and
half judgement, and somebody answers the judgement cells the tool left open. That person or agent wrote the content a
reader now meets, so `by` names them instead.

`report` and `tool` survive the handover. `report` states the report the run used, as `coverage`. That is the name
`kac report` takes, and never this record's id, so a reader holding the record can regenerate it. `tool` keeps the
tool that wrote the mechanical half, as `kac/0.25.0`. Both stay absent where nobody ran the tool.

An actor takes one of OKF's three forms: a person as `human:alex.doe`, a process as `process:nightly-sweep`, or a tool
or an agent with its version as `kac/0.25.0`. `generated-by-a-known-actor` is the rule.

`sources` is a field every record may have, and a report refines it. Elsewhere it says where content came from, as a URL
or as a description of what was read. Here it gives each corpus the report answers for. OKF puts no version on a source,
and this one does. The fact a reader needs is which content the report is true of, and `content-version` is what the
corpus already keeps.

`verified` says who has checked the report, one entry per actor and oldest first, and the fix type has the same field.
A draft states none, and every other status states one. An actor is written once, and a check they make again moves the
`at` on the entry they have, because git keeps every earlier state of the file. `one-verification-per-actor` is the
rule. A report an agent wrote and nobody has read is a draft, and writing an entry to get past the schema is the one
thing this field must never hold. Two values are refused. A `role:` is out,
because a post cannot read an answer, and the person who did stays named after the post changes hands. The actor that
`generated.by` names is out too, because nobody signs off their own writing. `no-self-verification` is the rule, and it
reports as `self-verification`.

Who is in that list decides the report's trust tier, which is OKF's word for how much weight an answer carries. No
entry at all is unverified. A list of agents alone is machine-confirmed. One `human:` entry makes it human-reviewed.
`kac` derives the tier rather than storing it, so a record has one place saying who checked it. The export ships the
answer as `trust`, so a consumer that never opens the record still knows what it is reading.

### Moving the version without regenerating

A reviewer edits `sources[].version` by hand. That is the point of it.

A corpus moves its `content-version` whenever what it knows changes, and most of those changes touch no report. Adding a
fix does not alter which clauses a standard implements. So a reviewer who has checked that the report still holds raises
the version by hand, instead of running the report again and re-reading every verdict.

Which entry the reviewer raises decides what else they edit. `sources` lists the corpus the report answers for first,
then one entry per corpus it imports. Raising the first entry edits frontmatter alone, and the `verified` list stays as
it is. Raising an imported entry also means editing the `Imported:` bullet under `## Limits`, which repeats that
version, and adding a `verified` entry for the body they changed. Nothing checks the bullet against the frontmatter.

Neither raise moves `generated`. `at` dates the content's last meaningful change, and the bullet repeats a version
`sources` already carries, so a verification taken before the raise vouches for the same fact it did before.

`validate` warns where the version falls behind the corpus. A report carried forward across several versions without a
regeneration is one to run again. The export reads the same gap: a verification taken before `generated.at` read text a
later edit replaced, so it counts toward no trust tier.

## Adding a report

1. Ask what fact in the corpus answers it. A report is a walk over frontmatter and the link graph. Where the answer is
   written down nowhere, the corpus is missing a field rather than a report.
2. Decide what the tool must not say. Write that down before the code, because it is the boundary the output has to
   state in its own text.
3. Fill every column the corpus states, and leave the judgement columns blank.
4. State the limits in the report itself. A reader meets the output without this page beside it.

[`kac report`](../cli/report.md) is the page for running one.
