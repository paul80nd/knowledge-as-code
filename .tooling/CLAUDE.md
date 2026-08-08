# Working on `kac`

[`README.md`](README.md) is the reference for what the tool does. This is what will bite you while changing it.

## Adding or changing a check

**First ask whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in `.schema/<type>.yaml` — see [`SPEC.md`](SPEC.md). That costs the YAML and a fixture, and nothing
below applies: the catalogue, the checks table and `kac checks` all pick it up from the schema. Reach for a C# check
only when the question needs git history, a graph walk, or more than one document at once.

**A rule that does need C# is a class, not an arm.** One file in `kac.core/Rules/` implementing `IDocumentRule`, with
its own unit tests, and a line in `DocumentRules.All`. It declares the checks it `Emits`, and `CheckCatalogue.All`
reads them from there — so implementing it and registering it are the same edit, and its id cannot drift from its
catalogue entry. `Validator.CheckRules` finds it by the id the schema's `rules:` block declares; a rule id nothing
implements is a statement of intent and is skipped in silence.

Only the per-document shape has an interface. The rules still to come that need the whole corpus, a graph walk or git
history do not fit `RuleContext`, and their interface should be designed against the first real one rather than ahead of
it.

**A core check is not a rule.** It runs on every document, in the order `CheckDocument` reads one, and several return
early so a later check does not report nonsense about a document already known to be broken. That order is the design,
so core checks are called in sequence and never looked up in a registry. Where a group of them is self-contained —
`Checks/LinkChecks.cs`, `Checks/ClauseChecks.cs` — it is a static class of its own with unit tests; the rest stay in
`Validator.cs`, and extracting one buys nothing unless it has logic worth testing directly.

Wherever it lives, four places have to agree, and three of them fail a meta-test rather than a test you were looking
at:

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
  `template.md` beside it. Types it does not use are simply absent, which is silent. Adding a folder to a fixture
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
