# How `kac` is put together

`kac` is a .NET 10 command-line tool. A thin entrypoint parses the command line and hands each verb to `kac.core`, a
library that does the work. Every rule about a record comes from YAML files under `.schema/`, which `kac` reads at run
time. A corpus is one repository of knowledge records kept in git, and a record is one Markdown document in it. An
export is the set of files a corpus publishes for another corpus to consume.

```sh
dotnet run --project tooling/kac -- --help   # build the entrypoint from source and list its verbs
```

## The parts

| Part                                             | What it contains                                                                                                     |
|--------------------------------------------------|----------------------------------------------------------------------------------------------------------------------|
| `tooling/kac/`                                   | `Program.cs`, which declares each verb, parses its options and reads the clock, the environment, git and the network |
| `tooling/kac.core/`                              | Loading a corpus, every check, generation, export, bundling, packing, restore, update and reports                    |
| `.schema/`                                       | One YAML file per type (one kind of record), plus the shared fields, tiers, enums and checks                         |
| `manifest.yaml` and `template/`                  | The files `kac new` sends a new corpus and `kac update` sends an existing one                                        |
| `tooling/kac.tests/` and `tooling/kac.features/` | The unit tests and the Reqnroll behaviour specs                                                                      |
| `tooling/kac.goldens/` and `tooling/tests/`      | The golden suite, its fixtures, and the two round-trip scripts                                                       |

[`.schema/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/.schema/README.md) lists every schema
file and what it declares.

`kac.core` depends on `YamlDotNet` and `Markdig`. The entrypoint adds `Spectre.Console.Cli`, which generates `--help`
for every verb and option.

## How `kac` finds a corpus and its schema

`kac` makes two searches, both upward from the working directory:

1. The nearest folder holding a `.corpus.yaml` is the corpus. Where none exists, `kac` exits with code 2.
2. The nearest folder at or above the corpus holding a `.schema/` folder gives the schema. Where none exists, `kac`
   exits with code 1.

In this repository, each corpus under `examples/` and `template/` finds the one `.schema/` at the root.

`--version` and `--help` need neither search, so both answer from any folder.

## The order a command runs in

`kac validate` shows the order every verb follows:

1. `Spectre.Console.Cli` parses the arguments. An option the verb does not declare stops the run with exit code 1.
2. `Cli.InCorpus` finds the corpus and the schema.
3. `Program.cs` reads today's date, builds the HTTP client and reads which CI platform is running it. It then calls
   `Commands.Validate`.
4. `Corpus.Load` reads `.corpus.yaml`, asks git for the corpus's files, loads the schema and reads `.imports/`.
   [Discovery](../use/discovery.md) then selects the records from that listing and parses each one.
5. `Validator.CheckAll` checks the schema first, then each record, then the corpus as a whole. It returns a list of
   findings and prints nothing.
6. `Commands` prints the findings and returns the exit code.

## Where the console is

`kac.core` returns values, and only `Commands` writes to the console. `Validator.CheckAll` returns findings,
`New.Plan` and `Update.Plan` each return a plan, and `GeneratedFiles.Plan` returns the files `generate` would write.
`Commands` derives the exit code from the value it was handed.

A plan is decided before anything is done. `New.Apply`, `Update.Apply` and `GeneratedFiles.Write` act on exactly the
files their plan reports, so nothing is decided twice.

### Testing without a disk

`Tree` stands between every check and the disk. It is a listing of paths, a function that reads one, and a function
that says whether a path is on the disk. A test builds a `Tree` from strings, so it can validate a corpus that was never
written to disk. A check asks the listing whether a file is present, and calls `Tree.OnDisk` only where `Tree` says to.

The rest of `kac.core` follows the same pattern:

* `New.Plan` and `Update.Plan` take file listings, a manifest and a function saying whether two copies of a path match.
  A new case in either is a unit test, not a fixture corpus.
* `Corpus.Load` and `Schema.Load` each take either values or a path. The path overload is the only place a path becomes
  a corpus or a schema.

## Where a check lives

Most checks are data. A rule in `.schema/<type>.yaml` with an `expr:` needs no C#, and `kac` picks it up at load.
A check that needs C# is a class under `tooling/kac.core/Rules/`, or a core check under `tooling/kac.core/Checks/`.
`.schema/_checks.yaml` declares every check id, whichever kind reports it.

[Checks in C#](checks.md) says when a rule needs a class and how to add one.

## Where to go next

[Build, test and release](build-test-release.md) builds `kac` from a clone and runs every test layer.
