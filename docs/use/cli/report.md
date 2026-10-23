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
consumed corpus into. A corpus that inherits its policies reports on them.

### What the tool fills, and what it leaves open

A report is a template. `report` fills every cell it can read out of the corpus and leaves the rest blank. It prints
nothing it cannot justify from a record.

`coverage` prints `covered` or `uncovered` against each clause, and nothing else. In the `Verdict` column, a person
writes whether an uncovered clause is a gap or does not apply to this estate. The `Note` beside it says why.

Each coverage row arrives with four values read from frontmatter:

* the standards whose `implements:` lists the clause
* the deviations whose `departs-from:` cites it
* the controls verifying those standards
* any clause elsewhere in the corpus sharing its key

A shared key is a candidate pair: one obligation written from both sides as two clauses with one key. `report` lists
the match and never decides it. Two runs over an unchanged corpus print the same report.

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
once you answer a judgement cell. Leave `report` and `tool` as the run left them.

An actor takes one of three forms: a person as `human:alex.doe`, a process as `process:nightly-sweep`, or a tool or an
agent with its version as `kac/0.25.0`. `generated-by-a-known-actor` checks it.

`kac validate` checks `verified` four ways:

* Every status but `draft` needs at least one entry.
* An actor appears once, and a second check moves the `at` on their entry. `one-verification-per-actor` is the rule.
* An entry is a person, a process or a versioned tool, and never a `role:`. `verified-by-a-known-actor` is the rule.
* No entry is the actor in `generated.by`. `no-self-verification` is the rule, and it reports as
  `self-verification`.

The entries decide the report's trust tier. No entry is unverified, agents alone are machine-confirmed, and one `human:`
entry is human-reviewed. [`export`](export.md) ships the tier as `trust`.

`id` and `owner` arrive empty. Fill both in before you commit the file, and `kac validate` reports either one you miss.

### Moving the version without regenerating

Edit `sources[].version` by hand where you have checked that the report is still true at a newer `content-version`. You
keep every verdict.

Which entry you raise decides what else you edit:

| You raise         | You also edit                                                                                   |
|-------------------|-------------------------------------------------------------------------------------------------|
| the first entry   | nothing. `verified` stays as it is                                                              |
| an imported entry | the `Imported:` bullet under `## Limits`, which repeats the version, and a new `verified` entry |

Neither raise moves `generated`. Nothing checks the `Imported:` bullet against the frontmatter.

`validate` warns where a version falls behind its corpus. A verification taken before `generated.at` counts toward no
trust tier.

### `--out`

`--out` writes the report to the path you name, relative to where you typed the command. It refuses, and writes
nothing, in two cases:

* A file already occupies the path, so a run never overwrites a person's verdicts. Write the run somewhere else, and
  merge the two by hand.
* `.corpus.yaml` states no `corpus:`. The first entry in `sources` cites your corpus by that name. Write the key, or
  leave out `--out`.

A report printed without `--out` cites a corpus with no `corpus:` as `this corpus`.

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
clause stops citing it, and nothing else reports that.

`Standing` comes from the corpus's own register of frameworks: obliged, self-obligated or borrowing. A framework the
register does not place leaves the cell empty.

Each framework then gets a section of its own, with one row per reference. Its `Citations` count says how many clauses
cite that reference.

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
`content-version` the report names.

## Known limits

**A producer cannot see its consumers.** A clause uncovered here may be covered in a corpus that consumes this one. Each
report states that limit in its own text.

**Coverage is not verification.** A control points at a whole standard, so no column claims a clause is verified.

**A redirect into `reports/` can collide with the run.** `kac` reads every file in that folder, including the one a
shell has just opened. On Windows the run stops. Use `--out`, which writes after the corpus is read.

**`frameworks` reports on this corpus's own clauses alone.** The export includes a producer's framework references in
`frameworks.jsonl`, and `report` reads none of them, so a consumer's report leaves them out.

[Adding a report](../../maintaining/reports.md) is the page for adding a report to `kac`.
