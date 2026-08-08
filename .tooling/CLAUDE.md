# Working on `kac`

[`README.md`](README.md) is the reference for what the tool does. This is what will bite you while changing it.

## Adding or changing a check

**First ask whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in `.schema/<type>.yaml` — see [`../.schema/README.md`](../.schema/README.md) for what one may say.
That costs the YAML and a fixture, and nothing below applies: the catalogue, the checks table and `kac checks` all pick
it up from the schema.

**The test is what the author is told.** Write the expression when one fixed message says everything the code would have
said. Write the code where it can name *which* part of the document is at fault and a single string cannot. A rule that
reports "something here is wrong" where it could have named the missing piece has been made cheaper and worse, and
nothing in the gate will notice. Cost is the second question, and it only ever argues for converting a rule that has
already passed the first — a schema with no C# behind it was never the aim.

Sixteen rules fail that test, and they cluster, which is worth knowing before starting one:

* **Git history — 4.** `immutable-after-accepted`, `immutable-after-published`, `changelog-begins-at-active`,
  `changelog-on-material-change`. All four ask the same question: what changed in this commit versus the committed
  content, and was it substantive? One mechanism answers all of them, and it is the largest single piece of work left.
* **Cross-document — 6.** `store-has-service`, `not-load-bearing`, `constraint-consistency`, `rules-have-controls`, and
  the corpus-wide glossary pair `undefined-terms` and `unused-terms`. `Validator.CheckCorpus` already builds a
  `byId` index and resolves clause citations and reciprocals against it; these are more of that.
* **Graph — 1.** `no-dependency-cycles`.
* **Per-part — 4.** `alternatives-have-verdicts`, `terms-are-singular`, `carried-in-full-by-digest` and
  `escalation-required`. Each judges the parts of one document — bullets under a heading, entries in a glossary,
  branches of a diagnosis tree — and its message has to name the part that failed. Only the first is written.
* **A fixed form — 1.** `y-statement-present`. A Y-statement is six moves in one block-quote, and the message worth
  reading names the move that is absent. An expression could report that the block-quote is not a Y-statement, which is
  the one thing the author already knows.

**If you find yourself wanting loops, joins or quantifiers in the grammar to reach one of these, stop** — that is the
signal you are rebuilding OPA. Write a rule class.

**A rule that does need C# is a class, not an arm.** One file in `kac.core/Rules/` implementing `IDocumentRule`, with
its own unit tests, and a line in `DocumentRules.All`. It declares the checks it `Emits`, and `CheckCatalogue.All`
reads them from there — so implementing it and registering it are the same edit, and its id cannot drift from its
catalogue entry. `Validator.CheckRules` finds it by the id the schema's `rules:` block declares; a rule id nothing
implements is a statement of intent, is skipped, and is rendered on the type page as declared-but-not-enforced — so
long as it declares no `severity:`, which `SchemaChecks` holds it to.

Only the per-document shape has an interface. The rules still to come that need the whole corpus, a graph walk or git
history do not fit `RuleContext`, and their interface should be designed against the first real one rather than ahead of
it.

**A core check is not a rule.** It runs on every document, in the order `CheckDocument` reads one, and several return
early so a later check does not report nonsense about a document already known to be broken. That order is the design,
so core checks are called in sequence and never looked up in a registry. Where a group of them is self-contained —
`Checks/LinkChecks.cs`, `Checks/ClauseChecks.cs` — it is a static class of its own with unit tests; the rest stay in
`Validator.cs`, and extracting one buys nothing unless it has logic worth testing directly.

**`Checks/SchemaChecks.cs` reads no document at all.** It runs once, before the corpus, and asks whether the schema
declares anything the tool cannot act on. A vocabulary it tests must be read from the code that dispatches the value —
`Validator.IdStyles`, `Doc.RelatedSection`, `DocumentRules.ByRuleId` — never restated there, because a copy is a list
of what is spelled correctly rather than of what runs.

Wherever it lives, four places have to agree, and three of them fail a meta-test rather than a test you were looking at:

1. **`CheckCatalogue.All`** in `Findings.cs` — the registry. `kac checks` reads it, and so does the coverage gate.
2. **`Generator.DocRows`** *or* **`Generator.IntentionallyUndocumented`** — every catalogue id must appear in one of
   them or `ChecksTableProblems` fails. `DocRows` is for per-document checks a type page should advertise; the
   undocumented set is for checks a reader of that page cannot act on.
3. **A fixture that trips it** — the coverage gate fails on any reachable check no fixture exercises.
4. **Two documents** — the checks table in [`README.md`](README.md) beside this file, and the count in the root
   `README.md`. Neither is generated, so neither will tell you it is now wrong.

`DocRows` is deliberately *not* generated from the catalogue: rows are grouped and hand-worded, so several catalogue ids
fold into one reader-facing row. An expression rule is the opposite — one id, reporting under its own name — so its row
comes from its `description:` and writing one into `DocRows` would duplicate it.

**The coverage gate reads ids, not branches.** A check with two ways to fail is green once a fixture trips either one. A
rule reporting three faults under one id needs a fixture for each, and unit tests beside the rule class for the branches
a fixture would only duplicate.

## The fixtures

* They share the **real** `.schema/`. `AssembleTemp` copies it beside each fixture corpus, so a schema change ripples
  into every fixture at once — run the golden suite after touching `.schema/`, not just `./kac validate`.
* A fixture corpus is a corpus, so it obeys `type-setup`: a folder it holds needs its `<type>.md` and
  `_template.md` beside it. Types it does not use are simply absent, which is silent. Adding a folder to a fixture
  without standing the type up adds a finding to every scenario that reads it.
* Only fixtures in **`validate` mode** run the validator. `index`, `index-stale` and `mechanism` modes do not, so a new
  check cannot affect them.
* Regenerate with `dotnet run .tooling/kac-tests.cs -- --update [name]`, then **read the diff**. The command rewrites
  expectations to whatever the tool now produces, so it will happily bless a regression.

## The feature specs pin more than findings

A scenario asserting a whole corpus — `Structure.feature`, `Shape.feature` — pins how many documents the fixture holds,
as well as every finding it produces. Adding a file to a fixture changes that count, and regenerating the goldens will
not tell you: the golden layer and the feature layer assert different things about the same corpus.

`Harness` runs `Corpus.Load` then `Validator.CheckAll` — the two calls `Commands.Validate` makes. Keep it that way: a
harness that assembles its own subset of the sequence leaves whole checks unreachable from a spec, and every spec goes
on passing.
