# Working on `kac`

[`README.md`](README.md) is the reference for what the tool does. This is what will bite you while changing it.

## Adding or changing a check

**Ask first whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in `.schema/<type>.yaml` — see [`../.schema/README.md`](../.schema/README.md) for what one may say.
That costs the YAML and a fixture, and nothing else on this page applies: the catalogue, the checks table and
`kac checks` all pick it up from the schema.

**What decides it is what the author is told.** Write the expression where one fixed message says everything the code
would have said. Write the code where it can name *which* part of the document is at fault and a single string cannot.
A rule reporting "something here is wrong" where it could have named the missing piece has been made cheaper and worse,
and nothing in the gate will notice. Cost is the second question, and it only ever argues for converting a rule that has
already passed the first — a schema with no C# behind it was never the aim.

Eighteen rules fail that test and they cluster, which is worth knowing before starting one. Four are written, and the
argument for a class reads differently beside a class that exists.

| Cluster            | Rules                                                                                                                                                       | Why code                                                                                                                                                                                                               |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Git history**    | `immutable-after-accepted`, `immutable-after-published`, `changelog-begins-at-active`, `changelog-on-material-change`                                       | All four ask what changed in this commit against the committed content, and whether it was substantive. One mechanism answers them, and it is the largest piece of work left.                                          |
| **Cross-document** | `store-has-service`, `not-load-bearing`, `constraint-consistency`, `rules-have-controls`, `redefinitions-are-reciprocal`, `undefined-terms`, `unused-terms` | Each fits `ICorpusRule`. `redefinitions-are-reciprocal` is the hardest: it holds an entry inside one document against an entry inside another.                                                                         |
| **Graph**          | `no-dependency-cycles` — **written**                                                                                                                        | A loop lives in the set of edges and no document holds it, so the walk needs every record and the message has to name the ones it runs through.                                                                        |
| **Per-part**       | `alternatives-have-verdicts` and `terms-are-alphabetical` — **written** — beside `terms-are-singular`, `carried-in-full-by-digest`, `escalation-required`   | Each judges the parts of one document — bullets under a heading, entries in a glossary, branches of a diagnosis tree — and its message has to name the part that failed.                                               |
| **A fixed form**   | `y-statement-present` — **written**                                                                                                                         | A Y-statement is six moves in one block-quote, and the message worth reading names the move that is absent. An expression could only report that the block-quote is not a Y-statement, which the author already knows. |

**Wanting loops, joins or quantifiers in the grammar to reach one of these is the signal to stop** — that way lies
rebuilding OPA. Write a rule class.

**A rule that needs C# is a class, not an arm.** One file in `kac.core/Rules/`, its own unit tests, and a line in the
registry beside it. It declares the checks it `Emits` and `CheckCatalogue.All` reads them from there, so implementing a
rule and registering it are one edit and its id cannot drift from its catalogue entry. A dispatcher finds it by the id
the schema's `rules:` block declares. A rule id nothing implements is a statement of intent: skipped, and rendered on
the type page as declared-but-not-enforced, so long as it declares no `severity:` — which `SchemaChecks` holds it to.

**Which interface follows from what the rule has to read.**

| Interface       | Given                                                            | Runs from                    | Reports                                       |
|-----------------|------------------------------------------------------------------|------------------------------|-----------------------------------------------|
| `IDocumentRule` | one document, through a `RuleContext`                            | `Validator.CheckRules`       | against that document                         |
| `ICorpusRule`   | every record and the `byId` index, through a `CorpusRuleContext` | `Validator.CheckCorpusRules` | against the document it names, rarely its own |

What a rule is handed also decides when it runs: a pass narrowed to given paths applies the document rules and skips the
corpus ones, because a question about the set answered from a handful of its members is answered wrongly and with no
sign of it. A rule needing git history fits neither interface. Design that one against the first real case.

**A core check is not a rule.** It runs on every document, in the order `CheckDocument` reads one, and several return
early so a later check does not report nonsense about a document already known to be broken. That order is the design,
so core checks are called in sequence and never looked up in a registry. Where a group of them is self-contained —
`Checks/IdChecks.cs`, `Checks/LinkChecks.cs`, `Checks/ClauseChecks.cs` — it is a static class of its own with unit
tests; the rest stay in `Validator.cs`, and extracting one buys nothing unless it has logic worth testing directly.
`IdChecks` is the case for extracting: three passes read the shape of an id and of the filename carrying it, in three
directions, and a second copy of that shape would be a place for the styles to disagree silently.

**`Checks/SchemaChecks.cs` reads no document at all.** It runs once, before the corpus, and asks whether the schema
declares anything the tool cannot act on. Read every vocabulary it tests from the code that dispatches the value —
`IdChecks.IdStyles`, `Generator.IndexOrders`, the two `ByRuleId` maps — because a copy is a list of what is spelled
correctly rather than of what runs. The key vocabulary needs no list at all: `Schema.Load` reads every mapping through a
`Level` that records what it was asked for, so adding a `Get` is what admits a key. Adding one without the code that
reads what it parsed into moves the failure from `schema-unknown-key` to `schema-dispatch` rather than removing it.

Not every question there is about a vocabulary. A `mirrors-section:` names any section and the code acts on whatever it
names, so what makes it sound is the type's own `sections:` block — one declaration held against another in the same
file, which is `schema-shape` rather than `schema-dispatch`.

Wherever it lives, four places have to agree, and three of them fail a meta-test rather than a test you were looking at:

1. **`CheckCatalogue.All`** in `Findings.cs` — the registry. `kac checks` reads it, and so does the coverage gate.
2. **`Generator.DocRows`** *or* **`Generator.IntentionallyUndocumented`** — every catalogue id must appear in one of
   them or `ChecksTableProblems` fails. `DocRows` is for the checks a type page should advertise to whoever writes one
   of its records; the undocumented set is for checks a reader of that page cannot act on.
3. **A fixture that trips it** — the coverage gate fails on any reachable check no fixture exercises.
4. **The checks table** in [`README.md`](README.md) beside this file. It is hand-curated rather than generated, so it
   will not tell you it is now wrong. No prose states a check count: `kac checks` reports it.

`DocRows` is deliberately *not* generated from the catalogue: rows are grouped and hand-worded, so several catalogue ids
fold into one reader-facing row. An expression rule is the opposite — one id, reporting under its own name — so its row
comes from its `description:`, and writing one into `DocRows` would duplicate it.

**The coverage gate reads ids, not branches.** A check with two ways to fail is green once a fixture trips either one. A
rule reporting three faults under one id needs a fixture for each, and unit tests beside the rule class for the branches
a fixture would only duplicate.

## Where the console is

**`kac.core` answers in values and `Commands` writes them out.** `Validator.CheckAll` returns findings,
`MechanismCheck.Classify` returns a report, `MechanismSync.Plan` returns a plan, and none of them prints. The exit code
is `Commands`'s too, derived from the value it was handed.

That is what makes each of them testable from a set of strings rather than from a tree and a subprocess. The two
mechanism engines take the file listings and a `Func<string, bool>` answering whether two copies of a path say the same
thing, so the whole classification is decidable without a filesystem — a new arm is a unit test, not a fixture corpus.
Deciding and doing stay apart on the sync side as well: `Plan` names what a sync comes to, `Apply` carries it out, and
the files it copies are the ones the plan already reports.

## Adding a generated block

**`kac.core/GeneratedFiles.cs` is the one list of what `index` writes and where.** Adding a block is one entry there,
naming it beside the renderer that fills it, and nothing else: `Commands.Index` writes what the list says and
`Validator.CheckAll` holds the corpus to the same list, so a block cannot be written under a name nothing checks or
checked for under a name nothing writes. `Blocks` projects the names out without calling a renderer, which is what lets
`validate` ask what a file should carry without building any of it.

The flag on each entry says whether the markers have to be there. It is false for `README.md` alone, because that file
belongs to the corpus and deleting the markers is how the corpus declines the block. Everywhere else the file arrives
from the framework carrying them, and one that has gone is a block that stopped being written in silence.

## The fixtures

* They share the **real** `.schema/`. `AssembleTemp` copies it beside each fixture corpus, so a schema change ripples
  into every fixture at once — run the golden suite after touching `.schema/`, not just `./kac validate`. A `sync`
  scenario may narrow one side with `corpus-schema.txt`, which names the type files that side holds *before* the sync.
  The real schema cannot express a corpus holding fewer files than upstream, and that is the state a sync resolves.
* A fixture corpus is a corpus, so it obeys `type-setup`: a folder it holds needs its `<type>.md` and `_template.md`
  beside it. Types it does not use are simply absent, which is silent. Adding a folder to a fixture without standing the
  type up adds a finding to every scenario that reads it.
* Only fixtures in **`validate` mode** run the validator. `index`, `index-stale`, `mechanism` and `sync` modes do not,
  so a new check cannot affect them. `sync` is the only mode that writes. It asserts the tree the command left rather
  than only what the command printed, so its expectations name files and their content instead of a findings golden.
* Regenerate with `dotnet run .tooling/kac-tests.cs -- --update [name]`, then **read the diff**. The command rewrites
  expectations to whatever the tool now produces, so it will happily bless a regression.

## The feature specs pin more than findings

A scenario asserting a whole corpus — `Structure.feature`, `Shape.feature` — pins how many documents the fixture holds
as well as every finding it produces. Adding a file to a fixture changes that count, and regenerating the goldens will
not tell you: the golden layer and the feature layer assert different things about the same corpus.

`Harness` runs `Corpus.Load` then `Validator.CheckAll` — the two calls `Commands.Validate` makes. Keep it that way: a
harness assembling its own subset of the sequence leaves whole checks unreachable from a spec, and every spec goes on
passing.
