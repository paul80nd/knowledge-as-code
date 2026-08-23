# `tooling`: the `kac` tool

`kac` validates and generates a knowledge corpus against the machine-readable schema that corpus carries in
`.schema/`. The command is a **thin .NET 10 entrypoint** ([`kac/`](kac/)) over a small **`kac.core`** library holding
the mechanics. It packs as the dotnet tool `KnowledgeAsCode.Tool`, which installs the command `kac`, so a corpus can
install and pin a version instead of carrying a copy. The schema is the source of truth: `kac` reads it and enforces it,
so **adding a knowledge type is adding a YAML file, not editing this tool**.

[`manifest.yaml`](manifest.yaml) sits here too, and says which files a corpus shares with the framework. Each corpus's
own `.corpus.yaml`, at the corpus root, says what that corpus is.

Writing records rather than changing the tool?
[`../template/knowledge-as-code.md`](../template/knowledge-as-code.md) is the document for that: the taxonomy, the style
rules, and what each tier asks of a document. A corpus consumer, who installs the plugin and reads the export, is owed a
document nothing here provides, tracked as
[issue #203](https://github.com/paul80nd/knowledge-as-code/issues/203).

## Building

Requires the **.NET 10 SDK**. `dotnet run --project` builds and runs the entrypoint, so there is no build step to
manage. The first run restores the packages and is slow; later runs are cached. Those packages are
`Spectre.Console.Cli` on the entrypoint, and `YamlDotNet` and `Markdig` through `kac.core`.

These run from the repository root, as the test commands further down do:

```bash
dotnet build kac.slnx                      # kac, kac.core, kac.tests and kac.features together
dotnet pack tooling/kac/kac.csproj -o .dist/pack   # the tool as a package, which is how a corpus receives it
```

The solution sits at the repository root. It names projects in this folder, and an IDE opening it should see
`template/` and `example/` too. This page names it rather than linking it. The solution is in the verification layer, so
a corpus running a tool proven upstream holds no such file for a link to reach.

The package's version is `<Version>` in [`kac/kac.csproj`](kac/kac.csproj), moved by hand.
[`kac/PACKAGE.md`](kac/PACKAGE.md) is what nuget.org shows, written for whoever installs the tool rather than for
whoever changes it, which is this page. CI packs on every run and installs what it packed, so metadata that has gone
missing fails a pull request long before it could fail a publish.

**Releasing is moving that version.** A push to `main` publishes the version it carries, unless nuget.org already holds
that one. So a change to the tool ships by editing that line in the pull request that changes it. What is published is
permanent: nuget.org will unlist a version but never free the number, so a mistake is followed by the next patch. That
is why [`.github/workflows/publish-tool.yml`](../.github/workflows/publish-tool.yml) runs all three test layers again
against the commit that merged, and pushes the same package it proved. A required reviewer on the `nuget.org`
environment holds the job before any of that runs, so a merge asks a person before it reaches anybody else.

The publish then tags the commit `v<version>` and opens a release carrying that version's section of
[`kac/CHANGELOG.md`](kac/CHANGELOG.md), which `ChangelogTests` holds the version to having.

Run **one `kac` invocation at a time**: concurrent runs build the same project and contend over its output.

Argument parsing is [`Spectre.Console.Cli`](https://www.nuget.org/packages/Spectre.Console.Cli), so every command and
option carries generated `--help`. `Program.cs` says why that library and not another.

## Running it against a corpus

`kac` finds a corpus by walking up from the working directory for a `.schema/`, so it is run from inside one. Where the
tool's own files sit says nothing about which corpus it reads. Running it from here reaches no corpus at all.

```bash
cd ../example

kac validate            # validate the corpus
kac validate --json     # machine-readable summary + findings
kac generate            # regenerate indexes and blocks
kac generate --check    # verify generated output is fresh
kac export              # write the corpus to .dist/export/ as data a consumer reads
kac export --type glossary                        # …one type rather than every one that contributes
kac bundle              # assemble that export and .plugin/ into a plugin under .dist/plugin/
kac checks              # list every check the validator implements
kac checks --json       # …as JSON (the test suite reads this)
kac mechanism --check --against ../other-corpus   # shared-layer drift vs a reference
kac mechanism --sync                              # take the shared layers from upstream
```

Those read as a corpus runs them, against an installed `kac`. While changing the tool you want the working tree instead:
`dotnet run --project ../tooling/kac -- validate`, which is what CI uses, or the `./kac` launcher at
`example/`'s root that wraps it.

Every verb takes `--no-color`, and every verb reads `NO_COLOR` from the environment. `NO_COLOR` is the cross-tool
standard for the same request. A redirected stream carries no colour on its own, but an environment naming a runner
that renders escapes in its logs turns it back on, and GitHub Actions is one. Set `NO_COLOR` wherever the bytes have to
be the same everywhere. The golden suite sets it on every process it starts, for that reason.

### Exit codes

| Code | Meaning                                                                         |
|------|---------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                |
| `1`  | A corpus **error**, or a bad invocation (missing/unknown subcommand or option). |
| `2`  | A verb found no corpus. `--version` and `--help` need none and answer anyway.   |

Warnings never change the exit code.

## The features

One document per command, in [`features/`](features/). These are developer context rather than corpus records: they
carry no frontmatter, `validate` does not read them, and they are not becoming a knowledge type.

Each follows the same sections in this order: **Intent**, **What it is not**, **Approach**, **Decisions**, **Known
limits**. It is prose throughout and not a form. A heading with nothing true to say is left out, because filler reads as
an answer where an absence reads as work not yet done. Reasons stay inline in **Approach**, beside whatever they
explain. **Decisions** takes only the ones belonging to a feature as a whole.

| Document                                         | Covers                                                                                                |
|--------------------------------------------------|-------------------------------------------------------------------------------------------------------|
| [`features/validate.md`](features/validate.md)   | Which files are read as records, which are passed over, and which get a pass of their own.            |
| [`features/checks.md`](features/checks.md)       | Where a check comes from, how the schema is held to what the tool dispatches, and why a rule is data. |
| [`features/generate.md`](features/generate.md)   | What is generated, from what, and the two rules that keep the output byte-stable.                     |
| [`features/export.md`](features/export.md)       | The corpus written to `.dist/export/` as data an agent reads without cloning it.                      |
| [`features/bundle.md`](features/bundle.md)       | That export and the `.plugin/` tree assembled into an installable plugin, trimmed to what it can do.  |
| [`features/mechanism.md`](features/mechanism.md) | Drift against a reference corpus, and taking the shared layers down from it.                          |
| [`features/new.md`](features/new.md)             | Standing a corpus up in the folder you are in, from a template fetched at a ref.                      |
| [`features/update.md`](features/update.md)       | Taking a newer framework into a corpus, and adopting or giving up a type.                             |

`new.md` and `update.md` are specifications written before their commands exist, and say so at their head.

[`CLAUDE.md`](CLAUDE.md) is what will bite you while changing any of it.

## Tests

Three layers, all run from the repository root and all run in CI, on GitHub through
[`.github/workflows/kac.yml`](../.github/workflows/kac.yml) and on Azure DevOps through
[`.azuredevops/kac.yml`](../.azuredevops/kac.yml):

| Layer       | Project / file            | Run                                | Covers                                                                                                                                                           |
|-------------|---------------------------|------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Unit**    | `kac.tests` (xUnit v3)    | `dotnet test tooling/kac.tests`    | `kac.core`'s shared primitives (`Glob`, `Yaml`, `Schema` helpers, `Manifest.Resolve`, `Md`, …). Fast, precise localisation.                                      |
| **Feature** | `kac.features` (Reqnroll) | `dotnet test tooling/kac.features` | Validator **behaviour**, which is what findings a document produces, as Gherkin specs driving `kac.core` in-process.                                             |
| **Golden**  | `kac-tests.cs`            | `dotnet run tooling/kac-tests.cs`  | Fixtures diffed against committed goldens, plus the coverage and checks-table gates and the CLI contract (exit codes). See [`tests/README.md`](tests/README.md). |

The unit layer catches breakage in the pieces early. The feature layer is the readable regression net for what the
validator does. The golden layer owns the end-to-end CLI contract that the in-process layers bypass. Regenerate golden
expectations after an intended rule change with `dotnet run tooling/kac-tests.cs -- --update`.

The feature layer runs `Corpus.Load` then `Validator.CheckAll`, the pair `kac validate` itself calls, so every check the
command can emit is reachable from a spec. The golden layer builds `kac/` once per run and invokes the built assembly.
Each scenario is then a real process, without paying `dotnet run`'s up-to-date check for every one.

All three read the schema from [`../template/.schema/`](../template/.schema/), where it is authored. A schema edit
therefore ripples into every fixture in the same run, rather than into a copy someone has to keep in step.
`TemplateTests` holds `example/`'s copy of it to matching, in both directions.

### The round-trip

[`tests/round-trip.sh`](tests/round-trip.sh) is the layer above all three and the only test that leaves the repository.
It installs the built plugin into a Claude config directory of its own, looks a term up, and fetches a record through
the raw link the export wrote. That fetch is the one assertion that cannot be faked from the working tree. Run it from a
corpus, after `kac export` and `kac bundle`, with `jq`, `curl` and the Claude Code CLI on the path:

```bash
cd ../example && sh ../tooling/tests/round-trip.sh
```

CI runs it on Linux and Windows, which is why it is a shell script held to the subset Git Bash and older macOS bash
agree on.
