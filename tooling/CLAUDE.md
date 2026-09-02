# Working on `kac`

The [documentation site](https://paul80nd.github.io/knowledge-as-code/) is the reference for what each command does, one
page apiece, and [`README.md`](README.md) says how it is built. This is what will bite you while changing it.

## Writing here

**Load `technical-writing`, then `writing-in-the-tool`.** Between them they carry every rule for the comments and for
the prose pages under `tooling/`, this one included. [`kac/PACKAGE.md`](kac/PACKAGE.md) is the exception: nuget.org
renders it for somebody who has installed nothing, so it takes `writing-the-docs` instead.

## Running `kac` here

**Run the build, not the install.** `dotnet run --project tooling/kac -- <verb>` is the tool this branch holds. A bare
`kac` is the published tool from `~/.dotnet/tools`, at whatever version was installed last, and it rewrites generated
files with an older wording without saying so.

**Run every corpus.** Each one under `examples/` proves the tool against what it holds. `template/` holds what a new
corpus receives, and a copy of it has to validate before its owner has run anything.

```sh
# each corpus under examples/, one at a time
cd examples/library
dotnet run --project ../../tooling/kac -- validate
dotnet run --project ../../tooling/kac -- generate --check

# what a new corpus receives
cd ../../template
dotnet run --project ../tooling/kac -- validate
dotnet run --project ../tooling/kac -- generate --check
```

Both find their corpus by the `.corpus.yaml` at its root, and then find the `.schema/` to judge it against by walking up
to this repository's root, where it is authored once.

**Run one invocation at a time.** Concurrent runs build the same project and contend over its output.

**A warning fails the build.** [`Directory.Build.props`](Directory.Build.props) sets `TreatWarningsAsErrors`,
`EnforceCodeStyleInBuild` and `AnalysisLevel` at `latest`, for every project here and for `kac-tests.cs`. A nullable
warning and a style violation are both a broken build rather than a line in the log nobody reads, and
[`.editorconfig`](.editorconfig) is where the style itself is written. It holds locally as well as in CI, on purpose: a
check running in one and not the other is how the thing it catches reaches `main`. Where a warning is genuinely wrong,
suppress that one with a reason beside it rather than turning the setting off. `!` is not one of those
ways. [`NullForgivingTests`](kac.tests/NullForgivingTests.cs) holds the count of it at nothing, so a value
the compiler cannot see is settled either carries the fact on its type through `MemberNotNullWhen`, or is
read through something naming what it expected.

**Ask Rider about a file you just changed, where you can reach it.** The `mcp__rider__get_file_problems` tool reports
what the compiler above cannot: a member hiding one on an outer class, an unused deconstruction, a redundant cast. It
answers for one file in about a second. The tool comes from the Rider MCP server, so a session running inside Rider has
it and a session in a bare terminal does not. Check whether it is there, and carry on without it where it is not:
nothing here depends on it, and the build is what gates the branch.

**Do not sweep a project with it.** A pass over `kac.core` returns roughly 120 findings, about 46 of them warnings, and
most are this repository's own idiom rather than defects. Reading them costs more than they are worth. One file you have
just edited is the case where the ratio is good.

**Each pipeline has one reader.** [`.github/workflows/kac.yml`](../.github/workflows/kac.yml) and
[`.azuredevops/kac.yml`](../.azuredevops/kac.yml) gate this repository, and a change to one belongs in the other.
[`template/azure-pipelines.yml`](../template/azure-pipelines.yml) is the starter a corpus receives and then owns, so it
runs `kac` over that corpus and reads no `template/`. No corpus under `examples/` keeps a copy: the two gates above
are what cover them.

## Adding or changing a check

**Ask first whether it needs C# at all.** A check that is a predicate over frontmatter, sections, links or length is an
`expr:` on a rule in `.schema/<type>.yaml`. See [`docs/design/expressions.md`](../docs/design/expressions.md) for what
one may say. That costs the YAML and a fixture, and nothing else on this page applies: the catalogue, the checks table
and `kac checks` all pick it up from the schema.

**What decides it is what the author is told.** Write the expression where one fixed message says everything the code
would have said. Write the code where it can name *which* part of the document is at fault and a single string cannot. A
rule reporting "something here is wrong" where it could have named the missing piece has been made cheaper and worse,
and nothing in the gate will notice. Cost is the second question, and it only ever argues for converting a rule that has
already passed the first. A schema with no C# behind it was never the aim.

The rules that fail it cluster, which is worth knowing before starting one. The table marks the ones already written,
because the argument for a class reads differently beside a class that exists.

| Cluster            | Rules                                                                                                                                                       | Why code                                                                                                                                                                                                               |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Git history**    | `immutable-after-accepted`, `immutable-after-published`, `changelog-begins-at-active`, `changelog-on-material-change`                                       | All four ask what changed in this commit against the committed content, and whether it was substantive. One mechanism answers them, and it is the largest piece of work left.                                          |
| **Cross-document** | `store-has-service`, `not-load-bearing`, `constraint-consistency`, `rules-have-controls`, `redefinitions-are-reciprocal`, `undefined-terms`, `unused-terms` | Each fits `ICorpusRule`. `redefinitions-are-reciprocal` is the hardest: it holds an entry inside one document against an entry inside another.                                                                         |
| **Graph**          | `no-dependency-cycles` (**written**)                                                                                                                        | A loop lives in the set of edges and no document holds it, so the walk needs every record and the message has to name the ones it runs through.                                                                        |
| **Per-part**       | `alternatives-have-verdicts` and `terms-are-alphabetical` (**written**) beside `terms-are-singular`, `carried-in-full-by-digest`, `escalation-required`     | Each judges the parts of one document: bullets under a heading, entries in a glossary, branches of a diagnosis tree. Its message has to name the part that failed.                                                     |
| **A fixed form**   | `y-statement-present` (**written**)                                                                                                                         | A Y-statement is six moves in one block-quote, and the message worth reading names the move that is absent. An expression could only report that the block-quote is not a Y-statement, which the author already knows. |

**Wanting loops, joins or quantifiers in the grammar to reach one of these is the signal to stop.** That way lies
rebuilding OPA. Write a rule class.

**A rule that needs C# is a class, not an arm.** One file in `kac.core/Rules/`, its own unit tests, and a line in the
registry beside it.

It declares the check ids it `Emits`, and each of those needs an entry in `_checks.yaml` saying what it means.
`schema-dispatch` reports one that has none, so implementing a rule and declaring what it reports cannot come apart.

A dispatcher finds the rule by the id the schema's `rules:` block declares. That is a `RuleId`, and usually not the
`CheckId` it reports under: `y-statement-present` emits `y-statement`, and the two types are what keep that deliberate.

A rule id nothing implements is a statement of intent. It is skipped, and rendered on the type page as
declared-but-not-enforced, so long as it declares no `severity:`. `SchemaChecks` holds it to that.

**Which interface follows from what the rule has to read.**

| Interface       | Given                                                            | Runs from                    | Reports                                       |
|-----------------|------------------------------------------------------------------|------------------------------|-----------------------------------------------|
| `IDocumentRule` | one document, through a `RuleContext`                            | `Validator.CheckRules`       | against that document                         |
| `ICorpusRule`   | every record and the `byId` index, through a `CorpusRuleContext` | `Validator.CheckCorpusRules` | against the document it names, rarely its own |

**Where an expression is evaluated.** `Facts.cs` holds the fact functions and nothing else an expression can reach.
`RuleExpr.cs` is the lexer, recursive-descent parser, type checker and evaluator, and takes no dependency. `RuleSpec`
in `Schema.cs` carries `Expr`, `Compiled`, `Severity` and `Message`, and `ParseRule` compiles at load.

`Facts` is built for one document and discarded once that document's rules have run. That lifetime is what makes
`words()` safe to memoise there.

Take the narrower one wherever it will do. A rule handed every record to judge one document reads as though it needs
them all, and nothing in its signature says otherwise. A rule needing git history fits neither interface; design that
one against the first real case.

**A core check is not a rule.** It runs on every document, in the order `CheckDocument` reads one, and several return
early so a later check does not report nonsense about a document already known to be broken. That order is the design,
so core checks are called in sequence and never looked up in a registry.

Where a group of them is self-contained it becomes a static class with unit tests: `Checks/IdChecks.cs`,
`Checks/LinkChecks.cs`, `Checks/PartChecks.cs`, `Checks/ValueChecks.cs`. The rest stay in `Validator.cs`, and extracting
one buys nothing unless it has logic worth testing directly. `IdChecks` is the case for extracting. Three passes read
the shape of an id and of the filename carrying it, in three directions, and a second copy of that shape would be a
place for the styles to disagree silently.

`ValueChecks` is the other case, drawn by what a check reads and not by how much of it there is. What one frontmatter
value is held to is its `FieldSpec` and nothing about the document around it. So `Check` takes four things: the field's
declaration, the node, the kind of document, and the line the frontmatter opens on. A date's calendar arithmetic, an
enum's casing, a list's floor and a per-entry pattern are then all decidable from a declaration a test writes itself.

The order inside it carries the design as much as the outer sequence does. A template's unfilled marks are read as
absent before any field's own check sees them, so a placeholder is never reported as a malformed date. `IsAbsent` is
public because the required-field pass asks the same question before the class is reached, and a second reading of
"absent" would be free to disagree with this one.

**Whatever it is, a check writes what it found to a `Report`**, which every pass that walks one file builds and hands
down, and which a rule class receives as `RuleContext.Report`. Write the id out as a `CheckId` at the call: both halves
are strings, so the compiler is what stops a message landing in the id's place. `Findings.cs` says which passes build
one and which do not.

**`Checks/SchemaChecks.cs` reads no document at all.** It runs once, before the corpus, and asks whether the schema
declares anything the tool cannot act on.

Read every vocabulary it tests from the code that dispatches the value: `IdChecks.IdStyles`, `Generator.IndexOrders`,
and the two `ByRuleId` maps. A copy is a list of what is spelled correctly, and never of what runs. The key vocabulary
needs no list at all, because `Schema.Load` reads every mapping through a `Level` that records what it was asked for, so
adding a `Get` is what admits a key. Add one without the code that reads what it parsed into and the failure moves from
`schema-unknown-key` to `schema-dispatch`; it does not go away.

Not every question there is about a vocabulary. A `mirrors-section:` names any section and the code acts on whatever it
names. What makes it sound is the type's own `sections:` block, one declaration held against another in the same file.
That is `schema-shape`, not `schema-dispatch`.

Wherever it lives, three places have to agree, and each fails a meta-test rather than a test you were looking at:

1. **An entry in [`../.schema/_checks.yaml`](../.schema/_checks.yaml)**, the declaration. Its
   `description:` is what `kac checks` prints and what a reader meets; its `notes:` take the reasoning and the boundary.
   A check a rule class reports under with no entry here fails `schema-dispatch` when the schema loads.
2. **A row in `ChecksTable.DocRows`**, unless the check declares `on-type-page: false`. One or the other, or
   `ChecksTable.Problems` fails. `DocRows` is for the checks a type page should advertise to whoever writes one of its
   records. The flag is for a check that reads the schema, the template or the page itself, which is real and is not
   theirs to act on. The flag sits with the check because it is a fact about the check.
3. **A fixture that trips it.** The coverage gate fails on any reachable check no fixture exercises, and that is also
   what catches a check declared in the schema and reported by nothing.

No prose states a check count: `kac checks` reports it. [Checks](https://paul80nd.github.io/knowledge-as-code/design/checks/)
carries no table of checks either: it points at the schema, so there is nothing there to go quietly out of date.

`DocRows` is deliberately *not* generated from the catalogue. Rows are grouped and hand-worded, so several catalogue ids
fold into one reader-facing row. An expression rule is the opposite, one id reporting under its own name, so its row
comes from its `description:` and writing one into `DocRows` would duplicate it.

**The coverage gate reads ids, not branches**, so a rule reporting three faults under one id needs a fixture for each,
and unit tests beside the rule class for the branches a fixture would only duplicate.
[`tests/README.md`](tests/README.md) is where that gate and its consequences are set out.

## Where the console is

**`kac.core` answers in values and `Commands` writes them out.** `Validator.CheckAll` returns findings, `New.Plan`
returns a plan, `Update.Plan` returns another, and none of them prints. The exit code is `Commands`'s too, derived from
the value it was handed.

That is what makes each of them testable from a set of strings, and never from a tree and a subprocess.

`New.Plan` and `Update.Plan` take file listings, a manifest and a `Func` answering whether two copies of a path say the
same thing. What either command comes to is therefore decidable without a filesystem, and a new arm is a unit test
instead of a fixture corpus. `Tree` strikes the same bargain for the corpus itself: a listing, a `Func` that reads one
of its paths, and a `Func` answering whether a path is on the disk at all. **Every pass reads the corpus through it,
`Validator.CheckAll` included.**

`Corpus.Load` takes the listing, the schema and the descriptor, and its other overload is the one place a path becomes a
corpus. A test can therefore ask the whole of `validate` about a corpus nobody ever wrote to disk, and a new check gets
unit tests as well as a fixture.

`Schema.Load` strikes the same bargain a level up. Its values overload takes the schema's files by name, already read,
and its path overload lists `.schema/` and is the one place a path becomes a schema. So a test can ask what the loader
makes of a declaration nobody would commit. A name the map does not hold reads as an empty document, which leaves a
`.schema/` short of a shared block failing through the findings that name what is missing. The names are walked in
ordinal order, because a map carries none and `New` iterates `ByFolder` without sorting it first.

Ask the listing about presence and `Tree.OnDisk` only where `Tree` says to. A check that asks the disk directly passes
for whoever wrote the file and fails in CI, by which time they have stopped looking.

Deciding and doing stay apart on both sides. `Update.Plan` names what an update comes to and `Apply` carries it out;
`GeneratedFiles.Plan` names what a regeneration comes to and `Write` carries it out. In each case the files acted on are
the ones the plan already reports, so nothing is decided twice.

## Adding a generated block

**`kac.core/GeneratedFiles.cs` is the one list of what `generate` writes and where.** Adding a block is one entry there,
naming it beside the renderer that fills it, and nothing else.

`Commands.Generate` writes what the list says and `Validator.CheckAll` holds the corpus to the same list. So a block
cannot be written under a name nothing checks, or checked for under a name nothing writes.

`Blocks` projects the names out without calling a renderer, which lets `validate` ask what a file should carry without
building any of it. `Plan` renders them against what the corpus holds now, so `generate` and `generate --check` read one
answer between them.

The flag on each entry says whether the markers have to be there. It is false for `README.md` alone, because that file
belongs to the corpus and deleting the markers is how the corpus declines the block. Everywhere else the file arrives
from the framework carrying them, and one that has gone is a block that stopped being written in silence.

## The fixtures

* They share the **real** `.schema/`. `AssembleTemp` copies it beside each fixture corpus, and writes the `.corpus.yaml`
  that makes the tool read the assembled tree as one. So a schema change ripples into every fixture at once. Run the
  golden suite after touching `.schema/`, not just `kac validate`. The `update` and `new` scenarios read no fixture
  corpus at all: each stands one up from the real template, so a manifest change reaches them too.
* A fixture corpus is a corpus, so it obeys `type-setup`: a folder it holds needs its `<type>.md` and `_template.md`
  beside it. Types it does not use are absent, which is silent. Adding a folder to a fixture without standing the type
  up adds a finding to every scenario that reads it.
* Only fixtures in **`validate` mode** run the validator. `generate`, `generate-stale`, `update`, `export`, `bundle`
  and `new` modes do not, so a new check cannot affect them directly. `update`, `export`, `bundle` and `new` are the
  modes that write. Each asserts the tree the command left rather than only what the command printed, so its
  expectations name files and their content instead of a findings golden.
* **The two export fixtures commit the export itself**, under `expected-dist/`, and a diff there is a change to what a
  consumer reads. `export` runs both declared shapes together and `export-policies` runs one alone, so a manifest entry
  or a directory written for a type the corpus never adopted fails in the second. Their READMEs say what a diff asks of
  you: see
  [`export`](tests/fixtures/export/README.md) and [`export-policies`](tests/fixtures/export-policies/README.md).
  Nothing else in the suite holds a tracked copy of an untracked artefact. The `bundle` fixtures deliberately do not,
  because most of a bundle is that same export and a second copy would be a second thing to keep in step.
* **A `.jsonl` is one complete JSON object per line.** A formatter pretty-printing one destroys the format and leaves
  valid JSON behind. The two under `expected-dist/` are where that bites, because the golden diff then reports an
  export that moved rather than a file a tool broke. `JsonLinesTests` asks the invariant of every `.jsonl` git would
  list, staged or not, and names the file and the line.
* Regenerate with `dotnet run tooling/kac-tests.cs -- --update [name]`, then **read the diff**. The command rewrites
  expectations to whatever the tool now produces, so it will happily bless a regression.

## The feature specs pin more than findings

A scenario asserting a whole corpus, such as `Structure.feature` or `Shape.feature`, pins how many documents the fixture
holds as well as every finding it produces. Adding a file to a fixture changes that count, and regenerating the goldens
will not tell you: the golden layer and the feature layer assert different things about the same corpus.

`Harness` runs `Corpus.Load` then `Validator.CheckAll`: the two calls `Commands.Validate` makes. Keep it that way: a
harness assembling its own subset of the sequence leaves whole checks unreachable from a spec, and every spec goes on
passing.

That also decides which commands get a spec. A spec asserts findings, so `Schema.feature` covers what a type declares in
its `export:` block. What the exporter then writes is not a finding: the golden holds those bytes, the unit tests beside
`Exporter` hold the rules behind them, and a feature file would only say it a third time.

## The round-trip is the layer above them all

[`tests/round-trip.sh`](tests/round-trip.sh) is the only test that leaves the repository. The layers above prove the
export and the bundle over data: the goldens diff the tree file for file, the unit tests hold the rules that built
it, and the specs hold what the validator says about it. None of them can show that the thing assembled installs, that
the paths its skill names resolve inside the installed copy, or that a link built from its template fetches the record
it points at.

So it installs the plugin into a Claude config directory of its own and asks it those questions.
[`README.md`](README.md#the-round-trip) says what it asserts, how to run it and which corpus proves which skill, and the
script's own header carries that last part beside the code. Adding an assertion means choosing the corpus that already
holds what it asks about.

**It is a shell script rather than another scenario in the golden suite**, because CI runs it on two platforms.
Development happens on macOS and the first audience is on Windows, so it is held to the subset Git Bash and older macOS
bash agree on: no arrays, no `[[`, no process substitution.

**The fetch is the assertion that cannot be faked from the working tree.** A template built from the wrong host, the
wrong ref, or a path prefix that no longer exists assembles into a string that matches any pattern you would write for
it. Only fetching it and comparing the response against the file in the tree tells a good template from one that
resolves to a 404 or to a version of the corpus nobody asked about. It is written against the export's declared types
rather than against glossary, so a corpus adopting a second type brings its records under the same check without a line
changing.

**The trim is asserted from `bundle.json` inside the installed copy**, in both directions: a component the run kept is a
directory the install holds, and one it trimmed is a path the install does not. Each surviving skill is then held to
naming the parts file of the type its `requires` declares, and no other type's. A corpus adopting one type can only show
half of that, which is why more than one corpus runs.

Everything after that is written about one corpus, because what a skill tells a reader to search for is content. Two of
the glossary assertions restate over the real corpus what `ExporterTests` pins over corpora it builds for the purpose:
the chain ordering, and the stability that carries no ranking. A rule and the artefact a reader receives are the two
things that could have come apart.

The policy assertions ask a different question of the same chain. A clause carries its wording and its `level`
separately, and `level` is the field a reader acts on, so `pol-VURM.SHIP` is asserted at `MUST NOT` whole. A level
rebuilt by matching the first word of the wording would file that prohibition as an obligation and pass every other
check here.

The standards assertions ask it of a type with no `level` at all. A rule line holds several obligations and the keyword
sits inside the markdown, so the bold `**MUST**` and `**MUST NOT**` are what is asserted. `std-IDEM` is also where a
part addressing itself is proved: a rule is a heading, so its `anchor` is its own key, where a clause resolves to the
section holding its table.

payments is the corpus in this set that consumes another, so the merge is asserted there. An engineering rule has to
arrive in payments' own `rules.jsonl` carrying `eng:` on its `id` and on its `record`, and its source has to fetch from
the `sources` entry for eng: eng's ref, under eng's path prefix. Payments publishes under a different prefix, so a link
built from its own block reaches a file that is not there.

**The breadcrumb is asserted for every corpus**, because it is the one text a session reads without having asked
anything. A corpus whose manifest names `sources` has to credit each of them on a line of its own, or a reader is told
their own corpus holds records nobody there wrote.

## What has already cost a session

* **An XML comment cannot contain a double hyphen.** A `.csproj` comment therefore cannot spell a flag such as
  `--version`, and MSBuild fails to load the project rather than warning about it.
* **nuget.org answers 404 for a version it has already accepted**, for minutes afterwards. `--skip-duplicate` on the
  push is what stops a run inside that window failing. The version check ahead of it cannot see in.
* **[`tests/round-trip.sh`](tests/round-trip.sh) fails locally on a commit you have not pushed**, because it fetches
  the commit `HEAD` stands on from `raw.githubusercontent.com`. That failure is not a defect. CI runs against a pushed
  head and passes.
* **Three walk-ups look for `kac.slnx`, and each of them means the repository**: [`kac-tests.cs`](kac-tests.cs),
  [`kac.features/Harness.cs`](kac.features/Harness.cs) and [`kac.tests/Repo.cs`](kac.tests/Repo.cs). The tool has two
  of its own: `.corpus.yaml` finds the corpus, and `.schema/` above it finds what to judge that corpus against. Do not
  unify any of them without keeping those distinctions.
* **Never write a path into a file a corpus keeps.** The generated banner and the stale-index message both name the
  tool instead. A corpus is read from wherever it was installed, so a path written into its content is a fact about
  somebody else's machine.
