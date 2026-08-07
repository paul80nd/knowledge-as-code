# Working on `kac`

[`README.md`](README.md) is the reference for what the tool does. This is what will bite you while changing it.

## Adding or changing a check

Four places have to agree, and three of them fail a meta-test rather than a test you were looking at:

1. **`CheckCatalogue.All`** in `Findings.cs` — the registry. `kac checks` reads it, and so does the coverage gate.
2. **`Generator.DocRows`** *or* **`Generator.IntentionallyUndocumented`** — every catalogue id must appear in one of
   them or `ChecksTableProblems` fails. `DocRows` is for per-document checks a type page should advertise; the
   undocumented set is for checks a reader of that page cannot act on.
3. **A fixture that trips it** — the coverage gate fails on any reachable check no fixture exercises.
4. **Two documents** — the checks table in [`README.md`](README.md) beside this file, and the count in the root
   `README.md`. Neither is generated, so neither will tell you it is now wrong.

`DocRows` is deliberately *not* generated from the catalogue: rows are grouped and hand-worded, so several catalogue
ids fold into one reader-facing row.

## The fixtures

* They share the **real** `.schema/`. `AssembleTemp` copies it beside each fixture corpus, so a schema change ripples
  into every fixture at once — run the golden suite after touching `.schema/`, not just `./kac validate`.
* A fixture corpus is a corpus, so it obeys `type-setup`: a folder it holds needs its `<type>.md` and
  `template.md` beside it. Types it does not use are simply absent, which is silent. Adding a folder to a fixture
  without standing the type up adds a finding to every scenario that reads it.
* Only fixtures in **`validate` mode** run the validator. `index`, `index-stale` and `mechanism` modes do not, so a
  new check cannot affect them.
* Regenerate with `dotnet run .tooling/kac-tests.cs -- --update [name]`, then **read the diff**. The command rewrites
  expectations to whatever the tool now produces, so it will happily bless a regression.

## The feature specs pin more than findings

A scenario asserting a whole corpus — `Structure.feature`, `Shape.feature` — pins how many documents the fixture
holds, as well as every finding it produces. Adding a file to a fixture changes that count, and regenerating the
goldens will not tell you: the golden layer and the feature layer assert different things about the same corpus.

`Harness` runs `Corpus.Load` then `Validator.CheckAll` — the two calls `Commands.Validate` makes. Keep it that way: a
harness that assembles its own subset of the sequence leaves whole checks unreachable from a spec, and every spec goes
on passing.
