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

A report is a template. `report` fills every cell it can read out of the corpus and leaves the rest blank. It prints
nothing it cannot justify from a record.

`coverage` prints `covered` or `uncovered` against each clause, and nothing else. Whether an uncovered clause is a gap
worth closing, or something that does not exist in this estate, is a judgement about the estate. The tool cannot tell
the two apart. The `Verdict` column is where somebody writes which, and the `Note` beside it is where they say why.

Each coverage row arrives with four values read from frontmatter:

* the standards whose `implements:` lists the clause
* the deviations whose `departs-from:` cites it
* the controls verifying those standards
* any clause elsewhere in the corpus sharing its key

The shared key is a candidate. One obligation stated from both sides is written as two clauses with one key, so a match
is usually a pair. Only a person can tell a pair from two policies that chose the same word.

The mechanical half is the same on every run, so two people reading one corpus get one report. A diff between two runs
is a change in the corpus.

### The provenance frontmatter

A run writes three keys, following the
[Open Knowledge Format](https://github.com/GoogleCloudPlatform/knowledge-catalog/blob/main/okf/SPEC.md):

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

| Key         | What it states                                                                                     |
|-------------|----------------------------------------------------------------------------------------------------|
| `generated` | who wrote the content as it stands, and when. `report` is the report and `tool` the `kac` that ran |
| `sources`   | this corpus first, then each corpus it imports, with the `content-version` each answered at        |
| `verified`  | who has checked the report, one entry per actor, oldest first                                      |

A run writes itself into `generated.by`, and writes `status: draft` and an empty `verified`. Write yourself into `by`
once you answer a judgement cell, because `by` is whoever wrote the content a reader now meets. Leave `report` and
`tool` as the run left them, so a reader can run the same report again.

An actor takes one of three forms: a person as `human:alex.doe`, a process as `process:nightly-sweep`, or a tool or an
agent with its version as `kac/0.25.0`. `generated-by-a-known-actor` checks it.

`kac validate` checks `verified` four ways:

* Every status but `draft` needs at least one entry. A report nobody has read stays a draft.
* An actor appears once, and a second check moves the `at` on their entry. `one-verification-per-actor` is the rule.
* An entry is a person, a process or a versioned tool, and never a `role:`. `verified-by-a-known-actor` is the rule.
* No entry is the actor in `generated.by`. `no-self-verification` is the rule, and it reports as
  `self-verification`.

The entries decide the report's trust tier. No entry is unverified, agents alone are machine-confirmed, and one `human:`
entry is human-reviewed. [`export`](export.md) ships the tier as `trust`.

`id` and `owner` arrive empty, because a report is a record somebody owns. Fill both in before you commit the file.
`kac validate` reports either one you miss.

### Moving the version without regenerating

Edit `sources[].version` by hand where you have checked that the report is still true at a newer `content-version`. Most
changes to a corpus touch no report, so you raise the version and keep every verdict.

Which entry you raise decides what else you edit:

| You raise         | You also edit                                                                                   |
|-------------------|-------------------------------------------------------------------------------------------------|
| the first entry   | nothing. `verified` stays as it is                                                              |
| an imported entry | the `Imported:` bullet under `## Limits`, which repeats the version, and a new `verified` entry |

Neither raise moves `generated`. Nothing checks the `Imported:` bullet against the frontmatter.

`validate` warns where a version falls behind its corpus. A report carried forward across several versions is one to
run again. A verification taken before `generated.at` counts toward no trust tier.

### `--out`

Without the flag the report goes to standard output, and you send it wherever you want it. With it, `kac` writes the
file at the path you name, relative to where you typed the command.

`kac` refuses a path a file already occupies, and writes nothing. A finished report holds verdicts and notes somebody
wrote, and a run cannot tell those from output of its own. Write this run somewhere else, and merge the two by hand.

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
`frameworks` reports on local clauses alone. [Export](../../reference/export.md) says why.

[Adding a report](../../maintaining/reports.md) is the page for adding a report to `kac`.
