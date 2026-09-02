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

# from this root, where you changed a YAML file or a workflow
pip install -r .github/requirements.txt && yamllint --strict .
go install github.com/rhysd/actionlint/cmd/actionlint@v1.7.12 && "$(go env GOPATH)"/bin/actionlint
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
  `.editorconfig` says so and no check enforces it. Count characters and not bytes when you sweep for long lines: these
  corpora are full of em dashes, so `awk 'length > 120'` reports violations that are not there.
* **A YAML file answers to `yamllint`, and a workflow to `actionlint`.** [`.yamllint`](.yamllint) extends yamllint's
  `default` ruleset and carries the four places this repository departs from it. The `lint` job runs both, so an
  unlinted file fails the build rather than a review.
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

## Ask this repository's own corpus

`example-dogfooding` is what this repository knows about itself, installed here as a plugin.
[`.claude/settings.json`](.claude/settings.json) names the `marketplace` branch it is served from and turns it on, so
a clone has it without anyone adding a marketplace by hand. Three skills read it:

* **`standards-lookup`** finds the rule you have to build to. Ask it before you change a workflow, a YAML file, or any
  prose published here.
* **`policy-lookup`** finds what the estate is committed to, in the clauses `examples/engineering` states. Ask it
  before you propose anything touching secrets, access, dependencies, or what reaches production.
* **`glossary-lookup`** says what a word here means. Ask it before you infer a meaning from usage.

**Use them, and then say where they let you down.** This is the one corpus whose subject is the repository you are
working in, so a session here is the only reader who can tell what it is missing. Tell the developer, in the reply that
closes the session, whichever of these happened:

* a question one of the skills should have answered and could not
* a record that is missing, or one whose wording sent you the wrong way
* a lookup that would have helped, that you only thought of afterwards
* something the plugin cannot do that would have made it worth reaching for

Raise an issue for each that is a gap rather than a slip. The installed copy is read-only, so that is the only way an
agent writes back to it.

**The export is frozen at bundle time, and the branch serves what is on `main`.** A branch editing
`examples/dog-fooding` leaves the installed plugin behind, so a lookup can answer with a record that branch has already
changed. To read the working tree instead, run `kac export` and `kac bundle` in that corpus and add its `.dist/` as a
marketplace of your own. That one stays yours: a directory source resolves against the marketplace rather than the
project, so an absolute path is the only thing that works and none belongs in a checked-in file.

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

## Agent skills

The engineering skills read their configuration from [`.claude/agents-config/`](.claude/agents-config/), rather than the
`docs/agents/` their author assumes. `docs/` here is the published site, and `NavigationTests` fails a page the nav does
not list. Three files carry that configuration:

* [`issue-tracker.md`](.claude/agents-config/issue-tracker.md): issues live as GitHub issues on
  `paul80nd/knowledge-as-code`, reached with the `gh` CLI.
* [`triage-labels.md`](.claude/agents-config/triage-labels.md): the five canonical roles, each label string equal to
  its name.
* [`domain.md`](.claude/agents-config/domain.md): single-context, and the domain is described by the four `CLAUDE.md`
  files rather than by a `CONTEXT.md`.
