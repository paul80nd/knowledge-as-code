# Working on `kac`

The [documentation site](https://paul80nd.github.io/knowledge-as-code/) is the reference for what each command does, one
page apiece. Its Maintaining `kac` section says how the tool is
[put together](../docs/maintaining/architecture.md),
[built, tested and released](../docs/maintaining/build-test-release.md) and [forked](../docs/maintaining/forking.md),
how a [check is written in C#](../docs/maintaining/checks.md), and
[what a pull request owes](../docs/maintaining/contributing.md). [`README.md`](README.md) says what each test layer
checks inside. This is what will bite you while changing it.

## Writing here

**Load `technical-writing`, then `writing-in-the-tool`.** Between them they carry every rule for the comments and for
the prose pages under `tooling/`, this one included. [`kac/PACKAGE.md`](kac/PACKAGE.md) is the exception: nuget.org
renders it for somebody who has installed nothing, so it takes `writing-the-docs` instead.

## Running `kac` here

**Run the build, not the install**, as
[Run `kac` from source](../docs/maintaining/build-test-release.md#run-kac-from-source) shows. A bare `kac` rewrites
generated files with an older wording without saying so.

**Run every corpus**, with the commands the [root `CLAUDE.md`](../CLAUDE.md#before-you-commit) gives. Each corpus under
`examples/` proves the tool against what it holds, and `template/` is what a new corpus receives.

**A warning fails the build.** [`Directory.Build.props`](Directory.Build.props) sets `TreatWarningsAsErrors`,
`EnforceCodeStyleInBuild` and `AnalysisLevel` at `latest`, for every project here and for `kac-tests.cs`, and
[`.editorconfig`](.editorconfig) is where the style itself is written. Both hold locally as well as in CI, on purpose: a
check running in one and not the other is how the thing it catches reaches `main`.
[`eng:std-CSSTY`](../examples/engineering/standards/platform/dotnet/code-style.md) says what those settings oblige you
to, and [`eng:std-NETTST`](../examples/engineering/standards/platform/dotnet/testing.md) says the same for a test.
[`NullForgivingTests`](kac.tests/NullForgivingTests.cs) holds the count of `!` at nothing, so a value the compiler
cannot see is settled either carries the fact on its type through `MemberNotNullWhen`, or is read through something
naming what it expected.

**Ask Rider about one file you just changed**, where the `mcp__rider__get_file_problems` tool is available. It reports
what the compiler cannot, such as an unused deconstruction, in about a second. Ask it about one file only: over a whole
project most of its findings are this repository's own idiom.

**The starter pipelines belong to a corpus.**
[`template/.github/workflows/kac.yml`](../template/.github/workflows/kac.yml) and
[`template/azure-pipelines.yml`](../template/azure-pipelines.yml) read no `template/`, and no corpus under `examples/`
keeps a copy. [`.github/workflows/kac.yml`](../.github/workflows/kac.yml) gates this repository.

## Adding or changing a check

**Read [Checks in C#](../docs/maintaining/checks.md) before you start one.** It says when a rule needs a class, which
interface the class takes, and the three places that must agree once it exists. An `expr:` rule needs none of that,
because the catalogue, the checks table and `kac checks` read it from the schema.

## Where the console is

[How `kac` is put together](../docs/maintaining/architecture.md#where-the-console-is) says which parts return values and
which write, and [how a check reads the disk](../docs/maintaining/architecture.md#testing-without-a-disk). A check that
asks the disk directly passes for whoever wrote the file and fails in CI, by which time they have stopped looking.

## Adding a generated block

**`kac.core/GeneratedFiles.cs` is the one list of what `generate` writes and where.** Adding a block is one entry there,
naming it beside the renderer that fills it, and nothing else.

`Commands.Generate` writes what the list says and `Validator.CheckAll` holds the corpus to the same list. So a block
cannot be written under a name nothing checks, or checked for under a name nothing writes.

The flag on each entry says whether the markers have to be there. It is false for `README.md` alone, because that file
belongs to the corpus and deleting the markers is how the corpus declines the block. Everywhere else the file arrives
from the framework carrying them, and one that has gone is a block that stopped being written in silence.

## The fixtures

* **Run the golden suite after touching `.schema/`**, not just `kac validate`, because every fixture reads the real
  schema. The `update` and `new` scenarios read no fixture corpus at all: each stands one up from the real template, so
  a manifest change reaches them too.
* **A required field added to `_universal.yaml` fails every fixture record with `required-field`.** Write the field into
  each record under `tests/fixtures/` in the same change. An optional field needs none of this.
* **`FixtureDateTests` sees a date only through the field a rule names.** It reads the fields named by `field('...')` or
  `days_since('...')` in an `expr:` that measures against the day of the run. A rule measuring a date any other way
  leaves that date unguarded, so a fixture can drift past today and move a golden. Extend the guard in the same change.
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
* **Read the diff after you [update the goldens](../docs/maintaining/build-test-release.md#update-the-goldens).**

## The feature specs pin more than findings

A scenario asserting a whole corpus, such as `Structure.feature` or `Shape.feature`, pins how many documents the fixture
holds as well as every finding it produces. Adding a file to a fixture changes that count, and regenerating the goldens
will not tell you: the golden layer and the feature layer assert different things about the same corpus.

**A schema edit breaks a scenario that `--update` cannot repair.** `--update` regenerates the goldens and leaves the
feature files, so repair each broken expectation by hand and prove it with `dotnet test tooling/kac.features`. A schema
edit breaks a scenario in two ways:

* **A message the schema decides.** A scenario quotes it in full. An enum's values, a field's pattern, a threshold and a
  `message:` all appear in one, so adding a value to an enum rewrites the `enum` message.
* **A line number a new field moved.** A required field written into every fixture moves each finding below it down one
  line. A finding reported against the frontmatter stays at line 1, and a document with no frontmatter keeps its
  numbers.

**Keep `Harness` to the two calls `Commands.Validate` makes.** A harness assembling its own subset of the sequence
leaves whole checks unreachable from a spec, and every spec goes on passing. A spec for any other command states a
behaviour its `docs/use/cli/` page promises.

**A scenario reads the exit code and the disk, never the output**, because `Out` keeps the console it first meets. A
behaviour visible only in what a command prints belongs to the golden suite.

**Cite a new scenario on the specification page for its part**, under `docs/maintaining/`, in `## Specified by`.
Rename the citation in the same change as the scenario.

**Mutate the Given behind every refusal.** Set the value it breaks back to a sound one and run the spec. A refusal that
still fires is refusing for a reason the scenario does not name.

## The round-trip is the layer above them all

[`tests/round-trip.sh`](tests/round-trip.sh) installs the plugin and fetches a record's source from GitHub.
[`README.md`](README.md#the-round-trip) says what it asserts, which corpus proves which skill, and which shell it keeps
to. Add an assertion in the corpus that already holds what it asks about. Write no arrays, no `[[` and no process
substitution.

## What has already cost a session

* **An XML comment cannot contain a double hyphen.** A `.csproj` comment therefore cannot spell a flag such as
  `--version`, and MSBuild fails to load the project rather than warning about it.
* **A member inserted above another member's comment takes that comment over.** Nothing reports the move, so insert
  after the end of the previous member and read the comment above what you added.
* **nuget.org answers 404 for a version it has already accepted**, for minutes afterwards. `--skip-duplicate` on the
  push is what stops a run inside that window failing. The version check ahead of it cannot see in.
* **Three walk-ups look for `kac.slnx`, and each of them means the repository**: [`kac-tests.cs`](kac-tests.cs),
  [`kac.features/Harness.cs`](kac.features/Harness.cs) and [`kac.tests/Repo.cs`](kac.tests/Repo.cs). The tool has two
  of its own: `.corpus.yaml` finds the corpus, and `.schema/` above it finds what to judge that corpus against. Do not
  unify any of them without keeping those distinctions.
* **`VerificationTests` and `ContentVersionTests` read git history, so a shallow checkout leaves them reading
  nothing.** The job running `dotnet test tooling/kac.tests` checks out with `fetch-depth: 0` and passes the base branch
  in `KAC_BASE_REF`. A job that sets the variable and cannot reach the branch fails. A job that sets neither passes,
  which is what a local run on `main` does.
* **A path written into a file a corpus keeps is a fact about somebody else's machine.** A corpus is read from wherever
  it was installed, so the generated banner and the stale-index message both name the tool instead.
  [`std-CONFIG`](../examples/dog-fooding/standards/configuration.md) is the rule.
