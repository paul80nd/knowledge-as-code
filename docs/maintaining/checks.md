# Checks in C\#

This page is for whoever adds a check to `kac` that the schema cannot declare on its own. It says when a rule needs C#,
how to add a fact the expression grammar can ask for, and why the numbers in the calibrated rules are what they are.

A corpus is one repository of knowledge records kept in git. A type is one kind of record, declared by one file under
`.schema/`. [Checks](../reference/checks.md) is the reference for what a schema owner declares.

## When a rule needs C\#

A question the [expression grammar](../reference/expressions.md) cannot ask needs a rule written in C#, and that rule
states the check id it reports under. `no-dependency-cycles` reports as `dependency-cycle`.

Some rules read every record at once, and some read what sits beside them:

* `dependency-cycle` reports a loop once, against the lowest id on that loop.
* `dependency-criticality` compares a record's grade with the grade on each record it depends on, reading the ranking
  from `ordered: true` on the enum.
* `alignment-rollup` reads the framework register.
* `mirrors-repo-links` reads `repos-base` from [`.corpus.yaml`](../use/corpus-descriptor.md#repos-base), because no
  record states where the estate keeps its code.

[`tooling/CLAUDE.md`][adding-a-check] says which interface a rule class takes, and the three places that must agree
once it exists.

## Adding a fact

Reach for a fact, not for the grammar. Adding one means one method on the tool's `Facts` class, one row in its function
table, and one row in the fact table on [Rule expressions](../reference/expressions.md). A test checks that table
against the function table, so a fact missing from the page fails the build. The grammar never changes.

A new fact usually serves one rule at first, and answers a question the next corpus will ask again. Extending the
grammar for one rule is almost always the wrong move.

A question that genuinely needs loops, joins or quantifiers is a rule written in C#, as [above](#when-a-rule-needs-c).

## The calibrated windows

Most rules that count words, links or days are uncalibrated, because no corpus has yet had enough records of those
types to fix a number. [Checks](../reference/checks.md#a-types-own-rules) lists the three that are.

[FedRAMP](https://www.fedramp.gov/legacy/playbook/csp/authorization/poam/) sets `high-risk-review-window` from the risk
level and gives a higher rating a shorter one: 30 days for high, 90 for moderate, 180 for low. Those windows time the
remediation of a vulnerability, where this one times the re-reading of an accepted risk, so the shape carries over and
the numbers do not. Six months is this repository's own figure, written as 184 days because six calendar months runs
from 181 days to 184.

`staleness` reads its windows off the calendar, so a rehearsal held to the cadence never trips. A date check has no
window to compare an event against, which is why `on-change` and `per-release` are reported only at `never`.

`staleness-loud` takes 92 days as a floor that every `rehearsal-frequency` meets. A runbook is read during an incident,
and the tools, permissions and commands it assumes go out of date whether or not the system it covers changed. The
price is a walk a quarter for every runbook a corpus keeps. A shorter window for a tabletop walk would fall hardest on
the runbooks whose fault nobody can cause, which are the ones no drill reaches.

A fixture pins each of `high-risk-review-window`'s numbers, so moving one is visible. A fixture cannot pin `staleness`
or `staleness-loud`, because their windows are measured against the day of the run and a fixture date drifts further
from today every day it sits there. A unit test names the day instead, and pins every window against it.

## Decisions

**A rule is data wherever it can be.** Wiring a rule as C# means a class, a registry line, unit tests, an entry in
`_checks.yaml`, a row in `ChecksTable.DocRows`, and a fixture. Wiring it as an expression means a line of YAML and a
fixture. A corpus downstream of this one may add a whole type file of its own. Without this layer every rule in that
file would be inert, and enforcing one would need an upstream code change and a release.

OPA/Rego was the obvious alternative and is the wrong shape. It would replace only the evaluation *tail* of the
pipeline, leaving all the markdown and frontmatter extraction untouched. It would add a language to learn and a runtime
dependency to the tool. The one property worth having is new rules as data, and a small hand-rolled evaluator buys it at
a fraction of that cost. `RuleExpr.cs` says when that judgement expires.

**A text pattern lives in `.schema/`.** A rule that matches text is a heuristic, and a heuristic gets tuned. Tuning a
regex in the schema costs a schema edit. Moving it in the tool would cost a release every corpus has to take.

## Where to go next

[Adding a report](reports.md) is the other way `kac` grows a question it can answer about a corpus.

[adding-a-check]: https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/CLAUDE.md#adding-or-changing-a-check
