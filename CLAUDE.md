# Working in this repository

This repository is not a corpus. It holds `kac`, the documentation site published beside it, the schema every corpus
below is judged against, the template `kac new` sends a corpus, and four worked corpora under `examples/`. Three of
them stand in for corpora that took the framework, and `dog-fooding` holds this repository to it. The tool finds a
corpus by walking up for a `.corpus.yaml`, so it reads one of the corpora under `examples/`, or `template/`, and never
this root.

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
tool's second walk-up is for. Neither is `.plugin/` bar its manifest: each corpus here names `plugin.from` in its
descriptor, so the skills and hooks are authored once under `template/` and `kac bundle` reads them from there.
Everything else the overlay layer names does live once in `template/` and again in each corpus under `examples/`. Copy
the file across by hand, in whichever direction the change came from, then run the check that proves you did, once per
corpus:

```sh
cd examples/library && dotnet run --project ../../tooling/kac -- update --check --from ../../
```

It answers in both directions: a copy that differs, and a file the corpus holds that the template sends nothing to.
[`manifest.yaml`](manifest.yaml) says which files this reaches.

## Before you commit

Run the layers your change touches, **one `kac` invocation at a time**: concurrent runs build the same project and
contend over its output.

```bash
# in each corpus you changed, and in template/
dotnet run --project ../../tooling/kac -- validate         # the corpus
dotnet run --project ../../tooling/kac -- generate --check # generated output is fresh
dotnet run --project ../../tooling/kac -- update --check --from ../../

# from this root, which holds the tool and the tests that prove it
dotnet test tooling/kac.tests      # unit
dotnet test tooling/kac.features   # Reqnroll behaviour specs
dotnet run tooling/kac-tests.cs    # golden fixtures, plus the coverage and checks-table gates
```

A corpus declaring `consumes:` needs `restore` ahead of `validate`, and `restore` needs a package to take. That corpus's
own `CLAUDE.md` names the producer to `export` and `pack` first.

A bare `kac` runs the published tool rather than this one. [`tooling/CLAUDE.md`](tooling/CLAUDE.md) says what that
costs, and carries the `template/` runs that go beside these.

All three test layers gate the branch and assert different things about the same corpus, so regenerating goldens can
leave you green locally and red in CI.

## Conventions

These hold in every corpus under `examples/`, in `template/`, and in the prose this repository publishes.

* **Regenerate rather than edit between `BEGIN GENERATED` and `END GENERATED`.** Change the schema or the frontmatter,
  then run `kac generate`. A schema edit without a regeneration fails CI.
* **Wrap Markdown prose at 120 columns.** Tables and link definitions are exempt: a URL cannot be broken.
  `.editorconfig` says so and no check enforces it.
* **Write what exists today.** Agreed and unbuilt work goes to the issue tracker. One exception: a schema rule the tool
  does not implement, where prose says the rule is declared and does not run, and the generated checks table carries it.
* **Keep comments and documentation timeless.** Describe the design as it stands. The history of a change belongs in its
  commit message.
* **Leave a whole document, not a diff.** Fold new material into what is there and delete what it supersedes, so the
  file reads in one voice and someone arriving cold cannot tell which paragraph is newest.
* **Say it once.** Cite rather than duplicate. A paragraph that belongs in two documents belongs in
  `knowledge-as-code/`, written a single time.
* **Where a prose rule and the schema disagree, the schema is right.** Report the contradiction rather than editing
  records to match.
* **Branch and open a PR.** Pushes to `main` are rejected.

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome (what happened, or what you found) and put
the supporting detail after it.

Keep answers brief: a high-level summary unless depth is asked for, short caveats, and a written document no longer than
its substance needs.

Deliver what was asked at the scope asked, making routine judgement calls yourself. Ask only where two readings would
produce materially different work. Where the request looks mistaken, say so in a sentence and carry on with it as asked.

## Before you raise a pull request

**Write the changelog entry always. Ask before you move `<Version>`.** A tool change somebody running `kac` can
observe gets a line under `## Unreleased` in [`tooling/kac/CHANGELOG.md`](tooling/kac/CHANGELOG.md), on the branch that
makes it. An entry written after the merge reaches nobody.

Whether that entry ships is a separate question, and it belongs to whoever owns the branch. A push to `main` publishes
`kac` whenever [`tooling/kac/kac.csproj`](tooling/kac/kac.csproj) names a version nuget.org does not already hold, so
moving `<Version>` **is** the release, and the release that publish opens carries that version's section. Put the call
to them before you open the pull request, with a recommendation: release where the change stands on its own, and hold
where it is one part of a group that is no use apart. Where the tool did not change there is nothing to ask.

Releasing renames `## Unreleased` to `## <version> - <date>` and moves `<Version>` in the same commit.
`ChangelogTests` fails a version that has no section.

**Move a corpus's `content-version` whenever you change what it knows.** Each corpus under `examples/` publishes: a
push to `main` packs it to GitHub Packages and bundles it into the `marketplace` branch, and both publishers take the
version the corpus states. A version that has not moved publishes nothing, silently, so an edited record reaches
nobody and the published copy drifts from `main` with no build reporting it.

Semantic, and about the records rather than the file: major where a meaning changed or a published URL broke, minor
for a record added, patch for wording. This is the corpus's own call and not the tool's, so nothing bumps it for you,
and a corpus holding one record moves the same way as one holding fifty.

**Repoint every consumer of a corpus whose minor moved.** Below 1.0.0 a caret pins the minor, so `examples/engineering`
going to `0.4.0` leaves `examples/payments` and `examples/dog-fooding` locked at a version their own `consumes:` ranges
no longer admit, and `kac restore` fails naming the version it could not find. A local run passes over it, because
`.imports/` is untracked and a restore keeps a folder already holding the version it resolved to. Delete `.imports/`
and restore again to see what CI sees.

**Ask which pages your change makes wrong.** Nothing in CI reads prose for meaning, so this is yours to do. A change to
a command reaches [`docs/`](docs/) and often [`tooling/README.md`](tooling/README.md); a change to what the tool is for
reaches [`README.md`](README.md) and [`tooling/kac/PACKAGE.md`](tooling/kac/PACKAGE.md); a change to the schema reaches
[`.schema/README.md`](.schema/README.md), [`.schema/meta/type.schema.json`](.schema/meta/type.schema.json),
[`docs/framework/metadata.md`](docs/framework/metadata.md) and [`docs/design/held-to.md`](docs/design/held-to.md).

**Run the layers your change touches**, which the commands above cover.

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

The engineering skills read their configuration from [`.claude/agents-config/`](.claude/agents-config/), rather than the
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
