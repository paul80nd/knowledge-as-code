# Build, test and release `kac`

Clone the repository, install the .NET 10 SDK, and build every project from the repository root. `kac` is a .NET tool
that checks a corpus (one repository of knowledge records kept in git) against the schema each type of record declares.

```sh
git clone https://github.com/paul80nd/knowledge-as-code.git
cd knowledge-as-code
dotnet build kac.slnx   # kac, kac.core, kac.tests and kac.features
```

The first build restores the NuGet packages and takes longest. Every command on this page runs from the repository
root unless its block changes directory.

## What to install

| Tool                      | Needed for                                  |
|---------------------------|---------------------------------------------|
| .NET 10 SDK               | building `kac` and running every test layer |
| Python 3.10 or newer      | building the site and running `yamllint`    |
| `actionlint`              | checking the workflows                      |
| `jq`, `curl`, Claude Code | the round-trip scripts                      |

On macOS, `brew install yamllint actionlint` installs both linters at the versions CI pins.

## Run `kac` from source

`kac` reads the corpus it finds above the working directory, so run it from inside one. Every folder under
`examples/` is a corpus, and so is `template/`.

```sh
cd examples/library
dotnet run --project ../../tooling/kac -- validate   # build kac from this clone and check the corpus
```

A `kac` on your `PATH` is whichever version you last installed from nuget.org. It is not the code in your clone.

Run one `kac` at a time. Two runs build the same project at once and contend over its output.

## The three test layers

Each layer proves something the others cannot. CI runs all three on every pull request.

| Layer   | Run                                | What it proves                                                                                |
|---------|------------------------------------|-----------------------------------------------------------------------------------------------|
| Unit    | `dotnet test tooling/kac.tests`    | Each piece of `kac.core` behaves, and the pages and comments in this repository agree with it |
| Feature | `dotnet test tooling/kac.features` | Each check reports what its Reqnroll scenario says, run in-process over a fixture corpus      |
| Golden  | `dotnet run tooling/kac-tests.cs`  | Each command, run as a real process, prints the output and writes the files a fixture expects |

The golden suite also fails two things no other layer sees: a check that no fixture trips, and a type page whose
checks table is out of date.

### Run the unit layer in part

The unit layer has two kinds of test. Most test `kac`. The rest, marked `Kind=Repository`, test this repository: a
page that cites a symbol the code no longer declares, or a changelog with no section for the version.

```sh
dotnet test tooling/kac.tests --filter "Kind!=Repository"   # kac alone, while you change it
dotnet test tooling/kac.tests --filter "Kind=Repository"    # the repository alone, after you change a page
```

### Update the goldens

A change to what a command prints changes a golden file. Regenerate every golden, or one fixture by name, then read
the diff. The update accepts a regression as readily as a fix.

```sh
dotnet run tooling/kac-tests.cs -- --update          # every fixture
dotnet run tooling/kac-tests.cs -- --update new      # the fixture named new
```

## The round-trip scripts

Two shell scripts test what no layer above can. CI runs both on Linux and Windows. A plugin is a corpus packaged for
Claude Code.

| Script                               | What it proves                                                                         |
|--------------------------------------|----------------------------------------------------------------------------------------|
| `tooling/tests/round-trip.sh`        | A plugin built from a corpus installs into Claude Code, and its skills answer          |
| `tooling/tests/import-round-trip.sh` | A citation into another corpus resolves, and fails once that corpus renames the clause |

```sh
sh tooling/tests/import-round-trip.sh   # needs jq

cd examples/library
dotnet run --project ../../tooling/kac -- export
dotnet run --project ../../tooling/kac -- bundle
sh ../../tooling/tests/round-trip.sh    # needs jq, curl and the Claude Code CLI
```

`round-trip.sh` fetches a record from GitHub at the commit you are on, so it fails on a commit you have not pushed.
[`tooling/README.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/tooling/README.md#the-round-trip) says
which corpus proves which skill.

## Check the site and the configuration

```sh
pip install -r docs/requirements.txt
mkdocs build              # fails on a dead link, because mkdocs.yml sets strict
yamllint --strict .       # every YAML file
actionlint                # every workflow
```

## Try the package before a release

Pack the tool, install it into a folder of the clone, and run that copy. A corpus receives exactly this package.

```sh
dotnet pack tooling/kac/kac.csproj -o .dist/pack
dotnet tool install --tool-path .dist/tools --add-source .dist/pack KnowledgeAsCode.Tool
cd examples/library && ../../.dist/tools/kac validate
```

`.dist/` is untracked, so the install leaves nothing to commit.

## Release a version

Moving `<Version>` in `tooling/kac/kac.csproj` is the release. A push to `main` runs `publish-tool.yml`, which
publishes whenever nuget.org does not already hold that version.

1. Write each change under `## Unreleased` in `tooling/kac/CHANGELOG.md` as it lands.
2. Rename that heading to the version and the date, as `## 0.33.0 - 2026-10-13`. `ChangelogTests` fails a version
   with no section.
3. Move `<Version>` to the same number in the same pull request.
4. Merge. A reviewer on the `nuget.org` environment approves the publish job before it runs.

The job runs the unit, feature and golden layers again on the merge commit, packs once, and pushes that package.
It then tags the commit `v<version>` and opens a GitHub release from the changelog section.

nuget.org never reuses a version number. Fix a bad release with the next patch.

## Where to go next

[Contributing to `kac`](contributing.md) says what a pull request owes and which checks gate it.
