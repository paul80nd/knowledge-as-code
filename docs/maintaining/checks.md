# Checks in C\#

This page is for whoever adds a check to `kac` that the schema cannot declare on its own. It says when a rule needs C#,
how to write one, how to add a fact the expression grammar can ask for, and why the numbers in the calibrated rules are
what they are.

A corpus is one repository of knowledge records kept in git. A type is one kind of record, declared by one file under
`.schema/`. [Checks](../reference/checks.md) is the reference for what a schema owner declares.

## When a rule needs C\#

A rule needs C# when one fixed message cannot tell the author what the code could. An `expr:` rule reports the one
`message:` its schema declares, whichever part of the document failed. A rule class can name the part.

* **The grammar cannot ask the question.** A loop over records, a join between documents or a quantifier over a
  document's parts is beyond the [expression grammar](../reference/expressions.md), so only code can answer it.
* **The grammar can ask it, and its answer is only "something here is wrong".** Where code could name the missing
  piece, write the code. A rule written as an expression to save a class is cheaper and worse, and no check fails it.

Cost comes second. It argues for turning a class into an `expr:` only where one fixed message already says everything
the class would.

| Rule                         | What its message names                                                                        |
|------------------------------|-----------------------------------------------------------------------------------------------|
| `y-statement-present`        | the moves missing from a Y-statement, where an expression could only say it is not one        |
| `alternatives-have-verdicts` | the Alternatives Considered bullet with no verdict                                            |
| `terms-are-alphabetical`     | the glossary term out of order, and the term it belongs before                                |
| `no-dependency-cycles`       | the route of a dependency loop, reported once against the lowest id on it                     |
| `dependency-criticality`     | the record cited, and the grade of each end, reading the ranking from `ordered: true`         |
| `mirrors-repo-links`         | a repository in `repos:` with no link, or a link with no entry, built from [`repos-base`][rb] |

[rb]: ../use/corpus-descriptor.md#repos-base

## Writing a rule class

A rule class is one file under `tooling/kac.core/Rules/`, its own unit tests, and a line in `DocumentRules.All`,
`CorpusRules.All` or `HistoryRules.All`. Which interface it takes follows from what it reads:

| Interface       | Given                                                                     | Runs from                    | Reports                                       |
|-----------------|---------------------------------------------------------------------------|------------------------------|-----------------------------------------------|
| `IDocumentRule` | one document, through a `RuleContext`                                     | `Validator.CheckRules`       | against that document                         |
| `ICorpusRule`   | every record and the `byId` index, through a `CorpusRuleContext`          | `Validator.CheckCorpusRules` | against the document it names, rarely its own |
| `IHistoryRule`  | one record at two revisions, paired by id, through a `HistoryRuleContext` | `Validator.CheckHistory`     | against the record where it is now            |

Take the narrower one wherever it will do. A rule handed every record to judge one document reads as though it needs
them all.

### A rule that compares two revisions

An `IHistoryRule` runs only under `validate --since`. `Baseline.Read` reads each file that differs between that
revision and the working tree, through the same `Corpus.Load` the working tree takes, so both copies of a record are
parsed alike. Any other file is the same at both, and pairs with itself. `Validator.CheckHistory` pairs the two
copies by `id`, and hands a rule only a record whose text differs. `Substance.Changes` lists the parts of a record that
changed in substance, which is the question both immutability rules ask. A rule on a type's parts compares
`Before.Parts` with `After.Parts`, as `PartIdKept` does. One instance serves every type declaring `part-id-kept`, and
reads the noun and the `in-force:` value from the type it is handed.

`retire-not-delete` is no rule class. It reads only a type's `in-force:` value, so `Validator.CheckHistory` reports it
for every type.

## Rule ids and check ids

A dispatcher finds a rule class by the id the schema's `rules:` block declares, which is a `RuleId`. The class reports
under a `CheckId`, which is usually different: `no-dependency-cycles` reports as `dependency-cycle`. The two types keep
that difference deliberate.

* The class lists the check ids it reports in `Emits`.
* Each check id needs an entry in `.schema/_checks.yaml`. `schema-dispatch` reports one that has none.
* A finding goes to the `Report` the pass hands down, as `RuleContext.Report` in a rule class. Write the id as a
  `CheckId` at the call, so the compiler stops a message landing in the id's place.

A rule id that no class implements and no `expr:` states is an intention. It declares no `severity:`, and the type
page renders it as declared but not yet enforced.

## A rule's config

A rule class declares the config keys it reads, such as `max-words`, in `Reads`. The loader reads a rule's entry for
those keys alone. `schema-dispatch` reports a config key on a rule whose class does not read it. An `expr:` rule reads
none, so any config key on one is reported.

A rule class also checks its own config in `Misconfigured`, and returns one sentence for each fault.
`y-statement-present` checks that `max-words` is a whole number above zero. `alignment-rollup` checks each of its
`postures` against the level-two headings on the register pages the records cite. A corpus rule's `Misconfigured`
therefore takes the corpus, and a document rule's takes the `Schema`.

Each fault is reported once under `schema-dispatch`, against the rule's line in its `.schema/` file. The rule then
checks no record. Write `Misconfigured` for a value that would make every finding the rule reports wrong.

## The three places that must agree

Each one fails a meta-test, not the test you were looking at.

1. **An entry in `.schema/_checks.yaml`.** Its `description:` is what `kac checks` prints. A check id a rule class
   reports with no entry fails `schema-dispatch` when the schema loads.
2. **A row in `ChecksTable.DocRows`, or `on-type-page: false` on the entry.** `ChecksTable.Problems` fails a check with
   neither. The flag is for a check that reads the schema, the template or the type page, which an author of a record
   cannot act on.
3. **A fixture that trips it.** The coverage gate fails a reachable check no fixture exercises. It counts ids, not
   branches, so a rule reporting three faults under one id needs a fixture for each, and unit tests for the rest.

## Core checks

A core check runs on every document, in the order `Validator.CheckDocument` reads one, and is never looked up in a
registry. Several return early, so a later check does not report on a document already known to be broken.

A self-contained group with logic worth testing becomes a static class under `tooling/kac.core/Checks/`:

* **`IdChecks`** states the shape of an id once. Three passes read it, and a second copy would let them disagree.
* **`ValueChecks`** judges one frontmatter value against its `FieldSpec` alone. It reads a template's unfilled marks as
  absent before a field's own check sees them, so a placeholder is never reported as a malformed date. `IsAbsent` is
  public because the required-field pass asks the same question.
* **`SchemaChecks`** reads no document. It runs once, before the corpus, and asks whether the schema declares anything
  the tool cannot act on.

`SchemaChecks` reads each vocabulary it tests from the code that dispatches it: `IdChecks.IdStyles`,
`Generator.IndexOrders`, the two `ByRuleId` maps and each rule class's `Reads`. A copy would list what is spelled
correctly, and not what runs.

## Adding a fact

Reach for a fact, not for the grammar. Adding one means one method on the tool's `Facts` class, one row in its function
table, and one row in the fact table on [Rule expressions](../reference/expressions.md). A test checks that table
against the function table, so a fact missing from the page fails the build. The grammar never changes.

A new fact usually serves one rule at first, and answers a question the next corpus will ask again. Extending the
grammar for one rule is almost always the wrong move.

`Facts.cs` holds the fact functions, and nothing else an expression can reach. `RuleExpr.cs` parses, type-checks and
evaluates an expression. A `Facts` is built for one document and discarded after that document's rules run, which is
what makes `words()` safe to memoise.

A question that needs loops, joins or quantifiers is a rule class, as [above](#when-a-rule-needs-c).

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

**`ChecksTable.DocRows` is written by hand.** Several catalogue ids fold into one row a reader of a type page can act
on, so the rows are grouped and worded for that reader. An `expr:` rule's row comes from its `description:` instead.

**A text pattern lives in `.schema/`.** A rule that matches text is a heuristic, and a heuristic gets tuned. Tuning a
regex in the schema costs a schema edit. Moving it in the tool would cost a release every corpus has to take.

## Where to go next

[Adding a report](reports.md) is the other way `kac` grows a question it can answer about a corpus.
