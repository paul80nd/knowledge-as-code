# `.tooling` — the knowledge-as-code tooling

`kac` validates and generates a knowledge corpus against the machine-readable schema in `.schema/`. The command you run
is a **thin .NET 10 file-based entrypoint** (`kac.cs`) over a small **`kac.core`** library that holds the mechanics;
`dotnet run` builds and runs it with no build step to manage. The schema is the source of truth: `kac` reads it and
enforces it, so **adding a knowledge type is adding a YAML file, not editing this tool**.

Two declarations the tool reads sit here too: [`manifest.yaml`](manifest.yaml), which says which files a corpus shares
with the framework, and each corpus's own `.corpus.yaml` at the repository root.

## Who this is for

Three readers exist and two of them are served.

* **A corpus author** writes records. [`../knowledge-as-code.md`](../knowledge-as-code.md) is theirs: the taxonomy, the
  style rules, and what each tier asks of a document.
* **A framework developer** changes the tool. `.tooling/` is theirs, and the feature documents below are written for
  them.
* **A corpus consumer** installs the plugin, reads the export and greps the terms file. Nothing here is addressed to
  them. That document is owed and tracked upstream, as
  [issue #203](https://github.com/paul80nd/knowledge-as-code/issues/203).

## Running

```bash
./kac validate            # validate the corpus
./kac validate --json     # machine-readable summary + findings
./kac index               # regenerate indexes and blocks
./kac index --check       # verify generated output is fresh
./kac export              # write the corpus to .dist/export/ as data a consumer reads
./kac export --type glossary                        # …one type rather than every one that contributes
./kac bundle              # assemble that export and .plugin/ into a plugin under .dist/plugin/
./kac checks              # list every check the validator implements
./kac checks --json       # …as JSON (the test suite reads this)
./kac mechanism --check --against ../other-corpus   # shared-layer drift vs a reference
./kac mechanism --sync                              # take the shared layers from upstream
```

`./kac` (Windows: `kac.cmd`) is a launcher at the repo root that wraps `dotnet run .tooling/kac.cs` — run it from the
solution root, or add the root to your `PATH` to drop the `./`. The explicit `dotnet run .tooling/kac.cs -- …` form
works identically and is what CI uses.

Argument parsing is handled by [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine), so every
command and option carries generated `--help`. The first run restores the packages (`System.CommandLine` on the
entrypoint; `YamlDotNet` and `Markdig` via `kac.core`) and is slow; subsequent runs are cached. Run **one `kac`
invocation per subcommand** — file-based apps share build output and contend if run concurrently.

## The features

One document per command, in [`features/`](features/). These are developer context rather than corpus records: they
carry no frontmatter, `validate` does not read them, and they are not becoming a knowledge type.

Each follows the same five sections in this order — **Intent**, **What it is not**, **Approach**, **Decisions**,
**Known limits**. It is prose throughout and not a form. A heading with nothing true to say is left out rather than
filled, because filler reads as an answer where an absence reads as work not yet done. Reasons stay inline in
**Approach**, beside whatever they explain; **Decisions** takes only the ones belonging to a feature as a whole.

| Document                                         | Covers                                                                                                |
|--------------------------------------------------|-------------------------------------------------------------------------------------------------------|
| [`features/validate.md`](features/validate.md)   | Which files are read as records, which are passed over, and which get a pass of their own.            |
| [`features/checks.md`](features/checks.md)       | Where a check comes from, how the schema is held to what the tool dispatches, and why a rule is data. |
| [`features/index.md`](features/index.md)         | What is generated, from what, and the two rules that keep the output byte-stable.                     |
| [`features/export.md`](features/export.md)       | The corpus written to `.dist/export/` as data an agent reads without cloning it.                      |
| [`features/bundle.md`](features/bundle.md)       | That export and the `.plugin/` tree assembled into an installable plugin, trimmed to what it can do.  |
| [`features/mechanism.md`](features/mechanism.md) | Drift against a reference corpus, and taking the shared layers down from it.                          |

## Tests

Three layers, all run in CI (see [`.github/workflows/kac.yml`](../.github/workflows/kac.yml)):

| Layer       | Project / file            | Run                                 | Covers                                                                                                                                                         |
|-------------|---------------------------|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Unit**    | `kac.tests` (xUnit v3)    | `dotnet test .tooling/kac.tests`    | `kac.core`'s shared primitives (`Glob`, `Yaml`, `Schema` helpers, `Manifest.Resolve`, `Md`, …) — fast, precise localization.                                   |
| **Feature** | `kac.features` (Reqnroll) | `dotnet test .tooling/kac.features` | Validator **behaviour** — "what findings this document produces" — as Gherkin specs driving `kac.core` in-process.                                             |
| **Golden**  | `kac-tests.cs`            | `dotnet run .tooling/kac-tests.cs`  | Fixtures diffed against committed goldens, plus the coverage & checks-table gates and the CLI contract (exit codes). See [`tests/README.md`](tests/README.md). |

The unit layer catches breakage in the pieces early; the feature layer is the readable regression net for what the
validator does; the golden/subprocess layer owns the end-to-end CLI contract that the in-process layers bypass.
Regenerate golden expectations after an intended rule change with `dotnet run .tooling/kac-tests.cs -- --update`.

The feature layer runs `Corpus.Load` then `Validator.CheckAll`, the pair `kac validate` itself calls, so every check the
command can emit is reachable from a spec. The golden layer builds `kac.cs` once per run and invokes the built assembly,
so each scenario is a real process without paying `dotnet run`'s up-to-date check for each one.

### Exit codes

| Code | Meaning                                                                         |
|------|---------------------------------------------------------------------------------|
| `0`  | No errors. Warnings may still have been printed.                                |
| `1`  | A corpus **error**, or a bad invocation (missing/unknown subcommand or option). |
| `2`  | Could not locate the repo root — the tool never started.                        |

Warnings never change the exit code.

