# Working in this repository

This repository is not a corpus. It holds `kac`, the template a corpus is copied from, and one worked corpus that took
that copy. The tool finds a corpus by walking up for a `.schema/`, so it reads `example/` and never this root.

| Working on                                  | Read                                                     |
|---------------------------------------------|----------------------------------------------------------|
| a record, or anything else `example/` holds | [`example/CLAUDE.md`](example/CLAUDE.md)                 |
| the schema, or a rule it declares           | [`example/.schema/CLAUDE.md`](example/.schema/CLAUDE.md) |
| `kac`, its checks, or the tests behind them | [`tooling/CLAUDE.md`](tooling/CLAUDE.md)                 |

[`template/CLAUDE.md`](template/CLAUDE.md) is the fourth, and it is addressed to somebody working in a corpus that
copied the template. It describes that corpus rather than this repository, so read it as content you may need to
change. The schema page beside it is the same file as `example/`'s, for the reason below.

## Two trees hold the same file

`.schema/` and everything else the overlay layer names live once in `template/` and again in `example/`, and
`TemplateTests` holds the two copies to the same bytes. `kac mechanism --sync` cannot do that job here, because
`template/` carries no `.corpus.yaml` and is therefore not a corpus the command can read. Copy the file across by hand,
in whichever direction the change came from, and let the test prove you did.
[`template/manifest.yaml`](template/manifest.yaml) says which files this reaches.

## Before you raise a pull request

**Move `<Version>` and write the changelog section together.** A push to `main` publishes `kac` whenever
[`tooling/kac/kac.csproj`](tooling/kac/kac.csproj) names a version nuget.org does not already hold, and the release
that publish opens carries the matching section from [`tooling/kac/CHANGELOG.md`](tooling/kac/CHANGELOG.md). A section
written after the merge reaches nobody, and `ChangelogTests` fails a version that has none.

**Ask which pages your change makes wrong.** Nothing in CI reads prose for meaning, so this is yours to do. A change to
a command reaches [`tooling/features/`](tooling/features/) and often [`tooling/README.md`](tooling/README.md); a change
to what the tool is for reaches [`README.md`](README.md) and [`tooling/kac/PACKAGE.md`](tooling/kac/PACKAGE.md); a
change to the schema reaches the `.schema/README.md` in both trees.

**Run the layers your change touches.** [`example/CLAUDE.md`](example/CLAUDE.md) carries the commands. Run one `kac`
invocation at a time: concurrent runs build the same project and contend over its output.

## What has already cost a session

* **Count characters, not bytes, when sweeping for long lines.** This corpus is full of em dashes, so
  `awk 'length > 120'` reports violations that are not there.
* **An XML comment cannot contain a double hyphen.** A `.csproj` comment therefore cannot spell a flag such as
  `--version`, and MSBuild fails to load the project rather than warning about it.
* **nuget.org answers 404 for a version it has already accepted**, for minutes afterwards. `--skip-duplicate` on the
  push is what stops a run inside that window failing. The version check ahead of it cannot see in.
* **[`tooling/tests/round-trip.sh`](tooling/tests/round-trip.sh) fails locally on a commit you have not pushed**,
  because it fetches the commit `HEAD` stands on from `raw.githubusercontent.com`. That failure is not a defect. CI runs
  against a pushed head and passes.
* **Regenerating goldens with `--update` blesses a regression as happily as a fix.** Regenerate, then read the diff.
* **Three walk-ups look for `kac.slnx`, and each of them means the repository**: `tooling/kac-tests.cs`,
  `tooling/kac.features/Harness.cs` and `tooling/kac.tests/Repo.cs`. The tool's own walk-up looks for a `.schema/` and
  means a corpus. Do not unify them without keeping that distinction.
* **Never write a path into a file a corpus keeps.** The generated banner and the stale-index message both name the tool
  instead. A corpus is read from wherever it was installed, so a path written into its content is a fact about somebody
  else's machine.
