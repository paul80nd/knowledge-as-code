# `tooling`: the `kac` tool

`kac` validates and generates a knowledge corpus against the machine-readable schema that corpus carries in
`.schema/`. The command is a **thin .NET 10 entrypoint** ([`kac/`](kac/)) over a small **`kac.core`** library holding
the mechanics. It packs as the dotnet tool `KnowledgeAsCode.Tool`, which installs the command `kac`, so a corpus can
install and pin a version instead of carrying a copy. The schema is the source of truth: `kac` reads it and enforces it,
so **adding a knowledge type is adding a YAML file, not editing this tool**.

[`../manifest.yaml`](../manifest.yaml), at the repository root, says which files a corpus is made of and which
layer each one falls in. Each corpus's own `.corpus.yaml`, at the corpus root, says what that corpus is.

Writing records rather than changing the tool?
[`../template/knowledge-as-code.md`](../template/knowledge-as-code.md) is the document for that: the taxonomy, the style
rules, and what each tier asks of a document. A corpus consumer, who installs the plugin and reads the export, is owed a
document nothing here provides, tracked as
[issue #203](https://github.com/paul80nd/knowledge-as-code/issues/203).

## Building, testing and releasing

[Build, test and release][site-build] on the documentation site says how to build `kac`, run its test layers and
publish a version. [Architecture][site-architecture] says how the projects fit together, and [Forking][site-forking]
lists what a fork replaces before it can publish.

The solution, `kac.slnx`, sits at the repository root. It names projects in this folder, and an IDE opening it should
see `template/` and `examples/` too. This page names it rather than linking it. The solution is in the verification
layer, so a corpus running a tool proven upstream holds no such file for a link to reach.

[`kac/PACKAGE.md`](kac/PACKAGE.md) is what nuget.org shows, written for whoever installs the tool rather than for
whoever changes it. CI packs on every run and installs what it packed, so metadata that has gone missing fails a pull
request long before it could fail a publish.

## Running it against a corpus

`kac` finds a corpus by walking up from the working directory for a `.corpus.yaml`, so it is run from inside one.
Every folder under [`../examples/`](../examples/) is one, and so is [`../template/`](../template/), which holds most of
what `kac new` sends a corpus. [Run `kac` from source][site-run] has the command, and says why a `kac` on your `PATH`
is the wrong one to run here.

[The CLI reference](https://paul80nd.github.io/knowledge-as-code/use/cli/) carries every verb, its options and the exit
codes, written for whoever installed the tool. Two things there are worth repeating here because they bite during
development. The golden suite sets `NO_COLOR` on every process it starts, because a runner that renders escapes in its
logs turns colour back on where a redirected stream would have dropped it. And `--version` and `--help` are answered by
the parser from wherever they were typed, so neither needs a corpus.

## The documentation site

Every command has a page at <https://paul80nd.github.io/knowledge-as-code/>, built by MkDocs from [`../docs/`](../docs/)
and published by [`publish-docs.yml`](../.github/workflows/publish-docs.yml). `../mkdocs.yml` names the pages and the
order they read in. A dead link fails `mkdocs build --strict`; a page no nav entry reaches is reported at INFO and
passes, so `NavigationTests` is what catches that.

The site also carries the framework itself, under `../docs/framework/`: what a type and a tier are, the types that ship,
what a record carries, how knowledge is contributed, and where the type names came from. A corpus keeps the narrow,
corpus-facing half of each of those pages, around tables generated from the types it adopted.
`DefaultTypesTests` holds the types page to the names and tiers the schema declares, and leaves the wording alone.

A command page opens with a usage block generated from the parser's own command model, and `CliReferenceTests` holds it
to matching. A command that grows an option gets its page back in step with
`KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests`. Read the diff afterwards, because the update blesses a regression as
happily as a fix.

Beneath that block, each page follows the same sections in this order: **What it does**, **Examples**, **Known
limits**. That set is fixed, and it is what lets a reader ask the same question of any command and find the answer in
the same place. Deeper structure goes under those headings rather than beside them, so a command with several worked
examples keeps each at `###`. `CliReferenceTests` holds every page to that set, and refuses one missing
**What it does** or **Examples**.

**Examples carry the output the command prints**, taken from a real run rather than written by hand. A reader deciding
whether they have a clean run needs to see one.

A command page argues nothing, which keeps it the length somebody reads while running the command.
[The documentation charter](../docs/maintaining/charter.md#where-an-argument-goes) says where the argument behind a
command goes.

It is prose throughout and not a form. **Known limits** is left out where a command has none, because filler reads as
an answer where an absence reads as work not yet done.

[`CLAUDE.md`](CLAUDE.md) is what will bite you while changing any of it.

## Tests

[The three test layers][site-layers] on the documentation site says what each layer proves and how to run it. All
three run in CI through [`.github/workflows/kac.yml`](../.github/workflows/kac.yml). What follows is what you need while
changing one.

### The unit layer holds two kinds of thing

Most of `kac.tests` asks whether `kac` behaves. A small tail asks whether **this repository** still holds together:
whether a page states the usage the parser accepts, whether a comment cites a file that exists, stays within three lines
and sits where Microsoft's C# conventions put it, whether `CHANGELOG.md` carries a section for the version `kac.csproj`
names, whether a fixture's dates still sit clear of today, whether a skill tells a reader to leave out a field every
type requires, whether a `fix` or a `report` whose prose changed was verified again, and whether a corpus whose records
changed moved its `content-version`. Those fail when content here drifts, when the calendar moves under it, or when a
change goes in unread, and never when the tool's logic is wrong. No corpus consuming `kac` carries one, because none of
it ships. The last two read git history, so the job running them checks out with `fetch-depth: 0`.

A `Kind` trait separates them, so a tight loop can leave the second out.
[Run the unit layer in part][site-filters] has the filters.

A trait rather than a project of its own, because the guards are a small tail rather than half the suite, `Repo` and
`CliReference` are read from both sides, and the whole suite runs in under a second. A second project would buy a name
and cost a build.

`KindTests` holds the convention: a test reading `Repo.Root` either carries the trait or is named as one that reads
the template to exercise the tool. Nothing else would catch a guard written without it, because the class still runs.

### The feature and golden layers

The feature layer runs `Corpus.Load` then `Validator.CheckAll`, the pair `kac validate` itself calls, so every check the
command can emit is reachable from a spec. Every other spec calls the `Commands` method its verb runs, which is the
whole of what that command does, so a refusal cannot stop it and go unnoticed here. Those specs assert the exit code
and the files the run left, and [`CLAUDE.md`](CLAUDE.md#the-feature-specs-pin-more-than-findings) says why not the
output.

The golden layer builds `kac/` once per run and invokes the built assembly. Each scenario is then a real process,
without paying `dotnet run`'s up-to-date check for every one.

Every layer reads the schema from [`../.schema/`](../.schema/), the one copy at the repository root. A schema edit
therefore ripples into every fixture in the same run, rather than into a copy someone has to keep in step.

### The round-trip

[The round-trip scripts][site-round-trips] on the documentation site says what each script proves. This section says
how this one is built.

[`tests/round-trip.sh`](tests/round-trip.sh) sits above them all, and is the only test that leaves the repository.
It installs the built plugin into a Claude config directory of its own, checks that the components `bundle.json` kept
arrived and the ones it trimmed did not, asks a surviving skill for something only its own type can answer, and fetches
a record's source from the base, prefix and ref the export wrote. That fetch is the one assertion that cannot be faked
from the working tree. Run it from a corpus, after `kac export` and `kac bundle`, with `jq`, `curl` and the Claude Code
CLI on the path:

```bash
cd ../examples/library && sh ../../tooling/tests/round-trip.sh
```

The corpus it runs in decides which skill it questions. `example-libraries` adopts glossary and not policies, so it
proves the trim as well as the glossary skill. `example-engineering` adopts glossary, policies and standards, so it
proves that several skills ship over one export and that none names another's file. `example-payments` is the one whose
rules are headings rather than table rows, so it is where a part addressing itself is proved.

CI runs it over each of those corpora on Linux and Windows, which is why it is a shell script held to the subset Git
Bash and older macOS bash agree on. `example-dogfooding` is left out: it is the same shape as `example-payments`, so
running it there would repeat that job rather than add one.

### The import round-trip

[`tests/import-round-trip.sh`](tests/import-round-trip.sh) is the other end-to-end script, and it stays inside the
repository. It runs `export` and `pack` in [`../examples/engineering/`](../examples/engineering/), then `restore` and
`validate` in [`../examples/payments/`](../examples/payments/), asserting that a scoped citation resolved against what
arrived. It then renames a clause upstream, leaving that corpus valid, and asserts the downstream build goes red naming
the citation nobody downstream touched.

That last assertion is the one the layers above cannot make. Each of them holds one corpus, and the promise being
proved is that a governance change reaches a repository its owner does not control. Run it from anywhere, with `jq` on
the path:

```bash
sh tooling/tests/import-round-trip.sh
```

It edits one file under `../examples/engineering/` and puts it back from a copy rather than from git, so an uncommitted
edit of your own survives a run. Both pipelines run it, this one on Linux and Windows, because it is held to the same
shell subset as the script above.

[site-architecture]: ../docs/maintaining/architecture.md
[site-build]: ../docs/maintaining/build-test-release.md
[site-filters]: ../docs/maintaining/build-test-release.md#run-the-unit-layer-in-part
[site-forking]: ../docs/maintaining/forking.md
[site-layers]: ../docs/maintaining/build-test-release.md#the-three-test-layers
[site-run]: ../docs/maintaining/build-test-release.md#run-kac-from-source
[site-round-trips]: ../docs/maintaining/build-test-release.md#the-round-trip-scripts
