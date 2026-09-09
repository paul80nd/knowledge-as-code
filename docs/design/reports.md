# Reports

A report answers a question about the corpus that no single record can: which clauses nothing implements, which
framework references hang on one citation. [`kac report`](../cli/report.md) prints one as markdown.

This page is for deciding what a new report carries. It says where the line between the tool and its reader falls, why a
report is a record rather than a build artefact, and what the frontmatter has to hold.

## The tool states facts and stops

`report` prints what the corpus states. It never prints a word it cannot justify from a record.

`coverage` is the worked case. A clause either has a standard whose `implements:` names it or it does not, so the tool
prints `covered` or `uncovered`. It cannot tell a gap worth closing from an obligation about something this estate does
not have. Both arrive as `uncovered`, and the reader splits them.

Drawing the line there is what makes the mechanical half reproducible. Two runs over one corpus print the same bytes, so
a diff is a change in the corpus rather than a change of mind.

### Everything the corpus already states is filled in

The judgement is expensive, so the tool spends nothing of it and everything else.

A coverage row arrives carrying the standards covering the clause, the deviations whose `departs-from:` names it, the
controls verifying those standards, and any clause elsewhere in the corpus sharing its key. Each is a value in
frontmatter. A reader who had to gather those four by hand would answer a handful of rows and abandon the rest.

The shared key is the softest of the four, and it is offered as a candidate. This corpus writes one obligation stated
from both sides as two clauses with one key, so a match is usually a pair. Two policies reaching for the same word by
coincidence is the other reading, and only a person can tell them apart.

## A report is a record

A finished report is a record in the corpus, under the `reports` type. It has an owner, it is confirmed by a person
before it is published, and a reader browsing the corpus finds it beside everything else.

The alternative is a file nobody keeps: generated on demand, read once, thrown away. That costs every reader the run and
the judgement, and it means the argument written on top of the numbers is written again each time.

Making it a record has a price, and the price is staleness. A record about the corpus is wrong the moment the corpus
moves, and the frontmatter is what keeps that visible.

## The frontmatter carries the provenance

Three keys, following the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md),
a specification for knowledge documents that a growing set of tools reads.

```yaml
generated: { at: 2026-09-08T10:00:00Z, by: kac/0.24.0 }
sources:
  - { resource: example-engineering, version: "0.10.2" }
confirmed:
  - { at: 2026-09-08T11:00:00Z, by: human:alex.doe }
```

`generated` names what produced the content and when. `by` takes OKF's `<producer>/<version>` form, so a report an agent
extended names the agent the same way the tool names itself.

`sources` names each corpus the report answers for. OKF carries no version on a source, and this one does: the fact a
reader needs is which content the report is true of, and `content-version` is what the corpus already keeps.

`confirmed` is this corpus's own field, shared with the fix type, and it holds every confirmation the report has had.

### The version moves without a regeneration

A reviewer edits `sources[].version` by hand. That is the point of it.

A corpus moves its `content-version` whenever what it knows changes, and most of those changes touch no report. Adding
a fix does not alter which clauses a standard implements. So a reviewer who has checked that the report still holds
raises the version and adds a `confirmed` entry, rather than running the report again and re-reading every verdict.

`validate` warns where the version falls behind the corpus. The gap between `generated.at` and the newest `confirmed`
entry is worth reading too: a report carried forward across several versions without a regeneration is one to run again.

## Adding a report

1. Ask what fact in the corpus answers it. A report is a walk over frontmatter and the link graph. Where the answer is
   not written down anywhere, the corpus is missing a field rather than a report.
2. Decide what the tool must not say. Write that down before the code, because it is the boundary the output has to
   state in its own text.
3. Fill every column the corpus states, and leave the judgement columns blank.
4. State the limits in the report itself. A reader meets the output without this page beside it.

[`kac report`](../cli/report.md) is the page for running one.
