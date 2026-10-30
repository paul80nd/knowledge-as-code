# Reporting

`kac report` builds a report over a corpus and everything it imports, and prints it or writes it to a file. A corpus is
one repository of knowledge records kept in git. This page lists what reporting is built to do, and the scenario
proving each behaviour. [`report`](../use/cli/report.md) owns the full statement of each one.

## What it does

* It builds `coverage`, which lists every policy clause, and `frameworks`, which lists every framework reference.
  [What it does](../use/cli/report.md#what-it-does)
* It reads the corpus and everything under `.imports/`, so a corpus that inherits policies and standards reports on
  them, whether or not it adopts the `policies` and `standards` types. [What it does](../use/cli/report.md#what-it-does)
* It marks each clause `covered` or `uncovered`, and leaves the `Verdict` and `Note` cells blank.
  [What the tool fills](../use/cli/report.md#what-the-tool-fills-and-what-it-leaves-open)
* It fills the standards, deviations, controls and candidate pairs each coverage row reads from frontmatter.
  [What the tool fills](../use/cli/report.md#what-the-tool-fills-and-what-it-leaves-open)
* It lists a deviation only while its `status` is the value its type declares in `in-force:`.
  [What the tool fills](../use/cli/report.md#what-the-tool-fills-and-what-it-leaves-open)
* Two runs over an unchanged corpus write the same report.
  [What the tool fills](../use/cli/report.md#what-the-tool-fills-and-what-it-leaves-open)
* It links every record id it cites to that record's page, and defines each link once at the foot of the report. An
  import stating no page is cited as bare code, and `## Limits` names it.
  [Links to each record](../use/cli/report.md#links-to-each-record)
* It stamps `generated`, and writes `status: draft`, an empty `verified`, and an empty `id` and `owner`.
  [The provenance frontmatter](../use/cli/report.md#the-provenance-frontmatter)
* It lists this corpus first in `sources`, then each corpus it imports, with the `content-version` each answered at.
  [The provenance frontmatter](../use/cli/report.md#the-provenance-frontmatter)
* `--out` writes the report to a path relative to where the command was typed. [`--out`](../use/cli/report.md#-out)
* `validate` reports a kept report's empty `id` and `owner`.
  [A report kept as a record](../use/cli/report.md#a-report-kept-as-a-record)
* `frameworks` reports on local clauses alone, because an `Alignment` column does not travel.
  [Known limits](../use/cli/report.md#known-limits)

## What it refuses

Each refusal exits `1` and writes nothing.

* a report name that is not built in. [What it does](../use/cli/report.md#what-it-does)
* an `--out` path a file already occupies, so a run never overwrites a person's verdicts.
  [`--out`](../use/cli/report.md#-out)
* a consumed corpus with nothing restored under `.imports/`. [What it does](../use/cli/report.md#what-it-does)
* an `--out` run over a corpus whose `.corpus.yaml` states no `corpus:`. [`--out`](../use/cli/report.md#-out)

## Specified by

[`Report.feature`][Report.feature]:

* A coverage report marks each clause covered or uncovered and leaves the judgement blank
* A report links each record it cites, and defines each link once at its foot
* A report cites an import stating no page as bare code, and names it in its limits
* Every link a report kept as a record writes resolves
* A coverage row lists the deviations, the controls and the pair candidates the frontmatter states
* A coverage row leaves out a deviation that is no longer in force
* A frameworks report counts the clauses citing each reference
* A frameworks report leaves out the references an imported corpus cites
* A report's frontmatter states the run that wrote it and leaves the rest to a person
* A corpus that imports policies reports on them, and names this corpus first among its sources
* Two runs over an unchanged corpus write the same report
* A report kept as a record has its id and owner reported until a person fills them in
* A report refuses a path a file already occupies
* A report refuses to write a file for a corpus its descriptor does not name
* A report name that is not built in is refused
* A report refuses a consumed corpus that has not been restored

## Unspecified

* `--out` resolving against where the command was typed. Scenarios share one working directory, so none can move it
  safely.
* What a run prints without `--out`. A scenario reads the disk and the exit code, never the output.
* A redirect into `reports/` colliding with the run on Windows. Only a shell opens the file early.

## Where to go next

[Adding a report](reports.md) says how to add a report of your own to `kac`.

[Report.feature]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/kac.features/Report.feature
