# Working in this repository

This repository is not a corpus. It holds `kac`, the documentation site published beside it, the schema every corpus
below is judged against, the template `kac new` sends a corpus, and three worked corpora standing in for ones that took
it. The tool finds a corpus by walking up for a `.corpus.yaml`, so it reads one of the corpora under `examples/`, or
`template/`, and never this root.

**Load `i-want-to` before you plan.** It routes the work to the playbook carrying its steps, and names the writing skill
for the surface you are on.

| Working on                                  | Read                                                                             |
|---------------------------------------------|----------------------------------------------------------------------------------|
| a record, or anything else a corpus holds   | [`examples/README.md`](examples/README.md)                                       |
| the schema, or a rule it declares           | [`.schema/CLAUDE.md`](.schema/CLAUDE.md)                                         |
| `kac`, its checks, or the tests behind them | [`tooling/CLAUDE.md`](tooling/CLAUDE.md)                                         |
| a page of the documentation site            | [`tooling/README.md`](tooling/README.md#the-documentation-site) and `mkdocs.yml` |

[`template/CLAUDE.md`](template/CLAUDE.md) is the fourth, and it is addressed to somebody working in a corpus that
copied the template. It describes that corpus rather than this repository, so read it as content you may need to change.

**Load `technical-writing`, then `writing-the-docs`, before you change [`README.md`](README.md) or
[`tooling/kac/PACKAGE.md`](tooling/kac/PACKAGE.md).** Both are read by somebody who has installed nothing and has nobody
here to ask.

## Four trees hold the same file

`.schema/` is not one of them. It is authored once at this root and read from there by every corpus, which is what the
tool's second walk-up is for. Everything else the overlay layer names does live once in `template/` and again in each
corpus under `examples/`. Copy the file across by hand, in whichever direction the change came from, then run the check
that proves you did, once per corpus:

```sh
cd examples/library && dotnet run --project ../../tooling/kac -- update --check --from ../../
```

It answers in both directions: a copy that differs, and a file the corpus holds that the template sends nothing to.
[`manifest.yaml`](manifest.yaml) says which files this reaches.

## Before you raise a pull request

**Move `<Version>` and write the changelog section together.** A push to `main` publishes `kac` whenever
[`tooling/kac/kac.csproj`](tooling/kac/kac.csproj) names a version nuget.org does not already hold, and the release that
publish opens carries the matching section from [`tooling/kac/CHANGELOG.md`](tooling/kac/CHANGELOG.md). A section
written after the merge reaches nobody, and `ChangelogTests` fails a version that has none.

**Ask which pages your change makes wrong.** Nothing in CI reads prose for meaning, so this is yours to do. A change to
a command reaches [`docs/`](docs/) and often [`tooling/README.md`](tooling/README.md); a change to what the tool is for
reaches [`README.md`](README.md) and [`tooling/kac/PACKAGE.md`](tooling/kac/PACKAGE.md); a change to the schema reaches
[`.schema/README.md`](.schema/README.md), [`.schema/meta/type.schema.json`](.schema/meta/type.schema.json) and
[`docs/schema/`](docs/schema/).

**Run the layers your change touches.** [`examples/library/CLAUDE.md`](examples/library/CLAUDE.md) carries the commands,
and every corpus carries the same set. Run one `kac` invocation at a time: concurrent runs build the same project and
contend over its output.

## What has already cost a session

* **Count characters, not bytes, when sweeping for long lines.** These corpora are full of em dashes, so
  `awk 'length > 120'` reports violations that are not there.
* **A seeded root type page assumes every type exists.** `services.md` and its siblings link to the other sixteen, so a
  corpus adopting a subset carries links to pages it does not hold, and `validate` fails on every one. Name the type
  and drop the link, which is what the pages under `knowledge-as-code/` already do for the same reason.
* **An XML comment cannot contain a double hyphen.** A `.csproj` comment therefore cannot spell a flag such as
  `--version`, and MSBuild fails to load the project rather than warning about it.
* **nuget.org answers 404 for a version it has already accepted**, for minutes afterwards. `--skip-duplicate` on the
  push is what stops a run inside that window failing. The version check ahead of it cannot see in.
* **[`tooling/tests/round-trip.sh`](tooling/tests/round-trip.sh) fails locally on a commit you have not pushed**,
  because it fetches the commit `HEAD` stands on from `raw.githubusercontent.com`. That failure is not a defect. CI runs
  against a pushed head and passes.
* **Regenerating goldens with `--update` blesses a regression as happily as a fix.** Regenerate, then read the diff.
* **Three walk-ups look for `kac.slnx`, and each of them means the repository**: `tooling/kac-tests.cs`,
  `tooling/kac.features/Harness.cs` and `tooling/kac.tests/Repo.cs`. The tool has two of its own: `.corpus.yaml` finds
  the corpus, and `.schema/` above it finds what to judge that corpus against. Do not unify any of them without keeping
  those distinctions.
* **Never write a path into a file a corpus keeps.** The generated banner and the stale-index message both name the tool
  instead. A corpus is read from wherever it was installed, so a path written into its content is a fact about somebody
  else's machine.

## Agent skills

The engineering skills read three files from [`.claude/agents-config/`](.claude/agents-config/), rather than the
`docs/agents/` their author assumes. `docs/` here is the published site, and `NavigationTests` fails a page the nav does
not list.

### Issue tracker

Issues live as GitHub issues on `paul80nd/knowledge-as-code`, reached with the `gh` CLI. See
[`.claude/agents-config/issue-tracker.md`](.claude/agents-config/issue-tracker.md).

### Triage labels

The five canonical roles, each label string equal to its name. See
[`.claude/agents-config/triage-labels.md`](.claude/agents-config/triage-labels.md).

### Domain docs

Single-context, and the domain is described by the four `CLAUDE.md` files rather than by a `CONTEXT.md`. See
[`.claude/agents-config/domain.md`](.claude/agents-config/domain.md).
