# `report` print a report over the corpus and what it imports

<!-- BEGIN GENERATED: usage-report -->

```text
kac report <NAME> [--no-color] [--out <PATH>]
```

| Argument | What it does           |
|----------|------------------------|
| `<NAME>` | Which report to print. |

| Option         | What it does                                                                          |
|----------------|---------------------------------------------------------------------------------------|
| `--no-color`   | Turn colour off. NO_COLOR in the environment does the same.                           |
| `--out <PATH>` | Write the report to this file instead of printing it. Refuses a path something holds. |

<!-- END GENERATED: usage-report -->

## What it does

`report` prints markdown to standard output. Pipe it into a file, a pull request comment, or a record the corpus keeps.

Two reports are built in. `coverage` lists every policy clause and what discharges it. `frameworks` lists every external
framework reference the clause tables cite, and the clauses citing each one.

Both read the corpus you are standing in and everything under `.imports/`, the folder [`restore`](restore.md) unpacks a
consumed corpus into. So a corpus that inherits the policies it answers to reports on those policies, and on the
standards that arrived with them.

### What the tool fills, and what it leaves open

A report is a template. `report` fills every cell it can read out of the corpus and leaves the rest blank.

`coverage` prints `covered` or `uncovered` against each clause, and nothing else. Whether an uncovered clause is a gap
worth closing, or something that does not exist in this estate, is a judgement about the estate. The tool cannot tell
the two apart. The `Verdict` column is where somebody writes which, and the `Note` beside it is where they say why.

That split is what makes the output worth committing. The mechanical half is the same on every run, so two people
reading one corpus get one report, and the argument on top of it is written once.

### The frontmatter a run stamps

The frontmatter has `generated`, giving the tool and version that wrote the report and the moment it ran. It also has
`sources`, listing this corpus and each corpus it imports, with the `content-version` each answered at. Only the tool
knows those, so it writes them. It writes `status: draft` as well, because a report nobody has read yet is a draft.

`id`, `owner` and `verified` arrive empty, because a report is a record somebody owns and somebody else verifies. Fill
all three in before you commit the file. `kac validate` reports any you miss.

### `--out`

Without the flag the report goes to standard output, and you send it wherever you want it. With it, `kac` writes the
file at the path you name, relative to where you typed the command.

`kac` refuses a path a file already occupies, and writes nothing. A finished report holds verdicts and notes somebody
wrote, and a run cannot tell those from output of its own. Write this run somewhere else, and merge the two by hand.
[Reports](../design/reports.md) says what a merge keeps.

## Examples

### A coverage report

```sh
kac report coverage
```

```text
| Policy         | Clauses | Covered | Uncovered |
|----------------|---------|---------|-----------|
| `eng:pol-A11Y` | 11      | 5       | 6         |
| `eng:pol-ACCS` | 13      | 2       | 11        |
| `eng:pol-AGNT` | 8       | 8       | 0         |
| **Total**      | **243** | **92**  | **151**   |
```

Each policy then gets a section of its own, one row per clause. The rows give the standards covering it, the deviations
departing from it, the controls behind those standards, and any clause elsewhere sharing its key.

### A frameworks report

```sh
kac report frameworks
```

```text
| Framework      | Standing    | References | Cited once |
|----------------|-------------|------------|------------|
| Azure WAF      | Inspiration | 4          | 0          |
| DORA metrics   | Inspiration | 4          | 1          |
| ISO 27001:2022 | Obliged     | 58         | 14         |
| **Total**      |             | **120**    | **41**     |
```

Read `Cited once` before you remove a citation. A reference cited by a single clause loses its last coverage when that
clause stops citing it, and nothing else in the corpus reports that.

`Standing` comes from the corpus's own register of frameworks, which says whether the corpus is obliged to a framework,
self-obligated to it, or borrowing from it. A framework the register does not place leaves the cell empty.

Each framework then gets a section of its own. The line under the heading repeats that standing and links the register
entry behind it, so a reader working down one table never scrolls back for it. The table gives one row per reference,
and its `Citations` count says how many clauses reach that reference. A count of `1` is one of the rows `Cited once`
counted.

### A report kept as a record

```sh
kac report coverage --out reports/clause-coverage.md
```

```text
wrote reports/clause-coverage.md
```

The filename says which question the report answers. The `rpt-` prefix belongs to the `id` alone, and a filename
repeating it fails `id-matches-filename`.

Fill in `id`, `owner` and the verdicts, then confirm it. `validate` warns once the corpus moves past the
`content-version` the report names, so a stale report says so on the page.

## Known limits

**A producer cannot see its consumers.** A clause uncovered here may well be covered in a corpus that consumes this one,
and every consumer answers for its own coverage. Each report states that limit in its own text.

**Coverage is not verification.** A control points at a standard, not at a rule, so it vouches for a whole document
whatever it checks inside it. No column claims a clause is verified.

**A pair candidate is a candidate.** Where one obligation is written from both sides, this corpus gives both clauses the
same key. `report` reports the match and never decides it. Two policies may reach for one word by coincidence.

**A redirect into `reports/` can collide with the run.** `kac` reads every file in that folder, including the one a
shell has just opened for the redirect. On Windows the two share one file and the run stops. `--out` has no such trap,
because `kac` writes the file after it has read the corpus.

**A framework reference does not travel to a consumer.** The `Alignment` column stays in the corpus that wrote it, so
`frameworks` reports on local clauses alone. [Export](../design/export.md) says why.

[Reports](../design/reports.md) is the page for deciding what a new report should carry, and what it must leave to
whoever confirms it.
