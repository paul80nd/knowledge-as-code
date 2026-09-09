# `report` print a report over the corpus and what it imports

<!-- BEGIN GENERATED: usage-report -->

```text
kac report <NAME> [--no-color]
```

| Argument | What it does           |
|----------|------------------------|
| `<NAME>` | Which report to print. |

| Option       | What it does                                                |
|--------------|-------------------------------------------------------------|
| `--no-color` | Turn colour off. NO_COLOR in the environment does the same. |

<!-- END GENERATED: usage-report -->

## What it does

`report` prints markdown to standard output. Pipe it into a file, a pull request comment, or a record the corpus keeps.

Two reports are built in. `coverage` names every policy clause and what discharges it. `frameworks` names every
external framework reference the clause tables cite, and the clauses citing each one.

Both read the corpus you are standing in and everything under `.imports/`, the folder
[`restore`](restore.md) unpacks a consumed corpus into. A corpus that inherits the policies it answers to therefore
reports on those policies, and on the standards that arrived with them.

### The mechanical half is filled and the judgement half is left open

A report is a template. `report` fills every cell it can read out of the corpus and leaves the rest blank.

`coverage` prints `covered` or `uncovered` against each clause, and nothing else. Whether an uncovered clause is a gap
worth closing or something that does not exist in this estate is a judgement about the estate, and the tool has no way
to tell the two apart. The `Verdict` column is where somebody writes which, and the `Note` beside it is where they say
why.

That split is what makes the output worth committing. The mechanical half is the same on every run, so two people
reading one corpus get one report, and the argument on top of it is written once.

### Every run stamps what produced it

The frontmatter carries `generated`, naming the tool and version that wrote the report and the moment it ran. It also
carries `sources`, naming this corpus and each corpus it imports, with the `content-version` each answered at.

Only the tool knows both, so it writes both rather than leaving a reader to work them out. The frontmatter arrives with
`id`, `owner` and `verified` left empty, because a report is a record somebody owns and somebody else verifies.

## Examples

### See which clauses no standard implements

```sh
kac report coverage
```

```text
| Policy       | Clauses | Covered | Uncovered |
|--------------|---------|---------|-----------|
| `pol-A11Y`   | 7       | 0       | 7         |
| `pol-ACCS`   | 11      | 1       | 10        |
| **Total**    | **207** | **74**  | **133**   |
```

Each policy then gets a section of its own, one row per clause, naming the standards covering it, the deviations
departing from it, the controls behind those standards, and any clause elsewhere sharing its key.

### See which clauses cite a framework reference

```sh
kac report frameworks
```

```text
| Framework       | Standing | References | Cited once |
|-----------------|----------|------------|------------|
| ISO 27001:2022  | binding  | 59         | 18         |
| NIST SSDF 1.1   | loose    | 16         | 3          |
| **Total**       |          | **114**    | **46**     |
```

`Cited once` is the column to read before removing a citation. A reference cited by a single clause loses its last
coverage when that clause stops citing it, and nothing else in the corpus reports that.

`Standing` is read from the corpus's own register of frameworks, which says whether the corpus is obliged to a
framework, self-obligated to it, or borrowing from it. A framework the register does not place leaves the cell empty.

### Keep a report as a record

```sh
kac report coverage > reports/rpt-clause-coverage.md
```

Fill in `id`, `owner` and the verdicts, then confirm it. `validate` warns once the corpus moves past the
`content-version` the report names, so a stale report says so on the page.

## Known limits

**A producer cannot see its consumers.** A clause uncovered here may well be covered in a corpus that consumes this
one, and every consumer answers for its own coverage. Each report states that limit in its own text.

**Coverage is not verification.** A control names a standard rather than a rule, so it vouches for a whole document
whatever it checks inside it. No column claims a clause is verified.

**A pair candidate is a candidate.** Where one obligation is written from both sides, this corpus gives both clauses the
same key. `report` names the match and never decides it: two policies may reach for one word by coincidence.

**A framework reference does not travel to a consumer.** The `Alignment` column stays in the corpus that wrote it, so
`frameworks` reports on local clauses alone. [Export](../design/export.md) says why.

[Reports](../design/reports.md) is the page for deciding what a new report should carry, and what it must leave to
whoever confirms it.
