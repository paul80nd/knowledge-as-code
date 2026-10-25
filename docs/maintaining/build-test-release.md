# Build, test and release `kac`

Clone the repository, install the .NET SDK that `global.json` names, and build every project from the repository root.
`kac` is a .NET tool that checks a corpus (one repository of knowledge records kept in git) against the schema each type
of record declares.

```sh
git clone https://github.com/paul80nd/knowledge-as-code.git
cd knowledge-as-code
dotnet build kac.slnx   # kac, kac.core, kac.tests, kac.features and kac.goldens
```

The first build restores the NuGet packages and takes longest. Every command on this page runs from the repository
root unless its block changes directory.

## What to install

| Tool                      | Needed for                                  |
|---------------------------|---------------------------------------------|
| .NET SDK                  | building `kac` and running every test layer |
| Python 3.11 or newer      | building the site, `yamllint` and `zizmor`  |
| `actionlint`              | checking the workflows                      |
| `jq`, `curl`, Claude Code | the round-trip scripts                      |

`global.json` at the repository root pins the SDK to one version, and `dotnet` refuses to run here with any other. Its
error names the version to install. CI installs the same version.

On macOS, `brew install yamllint actionlint zizmor` installs the three linters. CI pins yamllint and zizmor in
`.github/requirements.txt`, and actionlint in the `go install` line of `.github/workflows/kac.yml`. Check each
version against its pin.

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

| Layer   | Run                                          | What it proves                                                                                                           |
|---------|----------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| Unit    | `dotnet test --project tooling/kac.tests`    | Each piece of `kac.core` behaves, and the pages and comments in this repository agree with it                            |
| Feature | `dotnet test --project tooling/kac.features` | Each check, and each command a `.feature` file covers, does what its scenarios say, run in-process over a fixture corpus |
| Golden  | `dotnet test --project tooling/kac.goldens`  | Each command, run as a real process, prints the output and writes the files a fixture expects                            |

The golden suite also fails four things no other layer sees: a check that no fixture trips, a command the suite never
runs, a type page whose checks table is out of date, and a `--version` or `--help` that needs a corpus. Each is a test
case of its own beside the fixtures.

### Run one golden scenario

Each fixture under `tooling/tests/fixtures/` is one test case. Select it by the `Scenario` trait:

```sh
dotnet test --project tooling/kac.goldens --filter-trait "Scenario=new"   # the fixture named new
```

Set `GITHUB_ACTIONS=true` to run the suite the way CI does.

### Run the unit layer in part

The unit layer has two kinds of test. Most test `kac`. The rest, marked `Kind=Repository`, test this repository: a
page that cites a symbol the code no longer declares, or a changelog with no section for the version.

```sh
# kac alone, while you change it
dotnet test --project tooling/kac.tests --filter-not-trait "Kind=Repository"
# the repository alone, after you change a page
dotnet test --project tooling/kac.tests --filter-trait "Kind=Repository"
```

### Update the goldens

A change to what a command prints changes a golden file. Regenerate every golden, or one fixture by name, then read
the diff. The update accepts a regression as readily as a fix.

```sh
KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens                                # every fixture
KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens --filter-trait "Scenario=new"   # the fixture named new
```

## The round-trip scripts

Two shell scripts test what no layer above can. CI runs both on Linux and Windows: `import-round-trip.sh` once on each,
and `round-trip.sh` over three corpora on Linux and over `examples/payments` alone on Windows. A plugin is a corpus
packaged for Claude Code.

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

## Check the plugin

Build the plugin and check it the way CI does. The commands below cover two jobs: `corpora` exports and packs, and
`round-trip` exports, bundles and runs `claude plugin validate`. Run this from inside the corpus whose plugin your
change reaches. It needs `jq` and the Claude Code CLI.

!!! note
    `examples/payments` and `examples/dog-fooding` consume `examples/engineering`. Run `export` and `pack` there, and
    `restore` in the consumer, before `export`.

```sh
cd examples/dog-fooding
dotnet run --project ../../tooling/kac -- export
dotnet run --project ../../tooling/kac -- bundle   # the plugin, under .dist/plugin/
dotnet run --project ../../tooling/kac -- pack
claude plugin validate ./.dist/plugin --strict     # the plugin
claude plugin validate ./.dist --strict            # the marketplace beside it
sh ../../tooling/tests/bundled-components.sh .dist/plugin
```

`bundled-components.sh` fails in three cases:

* A component `bundle.json` kept is missing from the plugin. A component is a skill directory, a hook directory or a
  view's file.
* A component it trimmed is present.
* `.claude-plugin/` contains anything except `plugin.json`. Claude Code loads nothing else from there.

`round-trip.sh` runs the same script over the installed copy.

## Check the site and the configuration

```sh
pip install -r docs/requirements.txt
zensical build            # fails on a dead link or anchor, because mkdocs.yml sets strict
yamllint --strict .       # every YAML file
actionlint                # every workflow
zizmor .github template/.github   # every workflow and dependabot.yml, for insecure patterns
```

zizmor also asks GitHub about each pinned commit when `GH_TOKEN` is set, as it is in CI.

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
