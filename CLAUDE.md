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
Everything else the overlay layer names does live once in `template/` and again in each corpus under `examples/`.
[`manifest.yaml`](manifest.yaml) says which files that reaches, and
[`std-CONFIG`](examples/dog-fooding/standards/configuration.md) says what you owe each of them.

```sh
cd examples/library && dotnet run --project ../../tooling/kac -- update --check --from ../../
```

That check answers in both directions: a copy that differs, and a file the corpus holds that the template sends nothing
to.

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
yamllint --strict .
actionlint
```

A corpus declaring `consumes:` needs `restore` ahead of `validate`, and `restore` needs a package to take. That corpus's
own `CLAUDE.md` names the producer to `export` and `pack` first.

A bare `kac` runs the published tool rather than this one. [`tooling/CLAUDE.md`](tooling/CLAUDE.md) says what that
costs, and carries the `template/` runs that go beside these.

All three test layers gate the branch and assert different things about the same corpus, so regenerating goldens can
leave you green locally and red in CI.

**Install the two linters once.** `brew install yamllint actionlint` takes the versions the `lint` job pins. That job's
own `pip install -r .github/requirements.txt` needs Python 3.10 or newer, and macOS ships an older `python3`.

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome (what happened, or what you found) and put
the supporting detail after it.

## Ask this repository's own corpus

`example-dogfooding` is what this repository knows about itself, installed here as a plugin. The rules you build to are
records in it rather than lines on this page.

| Before you change                                       | Read                                                                                                                                                              |
|---------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| a workflow, a version, a changelog or a consumer's lock | [`std-CI`](examples/dog-fooding/standards/workflows.md)                                                                                                           |
| a YAML file, a pin, or a file two of the trees hold     | [`std-CONFIG`](examples/dog-fooding/standards/configuration.md)                                                                                                   |
| any prose, comment, commit message or generated block   | [`std-PROSE`](examples/dog-fooding/standards/prose.md)                                                                                                            |
| a skill, a hook or a `plugin.json` under `.plugin/`     | [`std-PLUGIN`](examples/dog-fooding/standards/plugin.md)                                                                                                          |
| C# or a test under `tooling/`                           | [`eng:std-CSSTY`](examples/engineering/standards/platform/dotnet/code-style.md) and [`eng:std-NETTST`](examples/engineering/standards/platform/dotnet/testing.md) |

`eng:std-CSSTY` and `eng:std-NETTST` are authored in `examples/engineering` and arrive through `consumes:`, so a clause
of either is cited as `eng:`.

[`.claude/settings.json`](.claude/settings.json) names the `marketplace` branch the plugin is served from and turns it
on, so a clone has it without anyone adding a marketplace by hand. Three skills read it:

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

Raise an issue for each that is a gap rather than a slip, and label it `dogfood`. The installed copy is read-only, so
an issue is the only way an agent writes back to it, and the label is what makes the loop countable. The same label
covers a finding about the agent configuration under [`.claude/agents-config/`](.claude/agents-config/), which a
session meets the same way and by using it.

**The export is frozen at bundle time, and the branch serves what is on `main`.** A branch editing
`examples/dog-fooding` leaves the installed plugin behind, so a lookup can answer with a record that branch has already
changed. [That corpus's own README](examples/dog-fooding/README.md#reading-this-corpus-as-a-plugin-while-you-change-it)
carries the loop that installs the working tree instead, and says what to rebuild after a record changes and after a
skill does.

## Where a page or a skill let you down

The plugin is a read-only copy, so an issue is the only way an agent writes back to it. The guidance is not. The four
`CLAUDE.md` files, the playbooks under [`.claude/skills/i-want-to/`](.claude/skills/i-want-to/) and the four writing
skills all sit in the working tree, one edit from whoever is reading your reply.

Tell the developer, in the reply that closes the session, whichever of these happened:

* an instruction that sent you the wrong way, or that you had to read twice before you could act on it
* a question one of those pages should have answered and did not
* an instruction you would have followed anyway, because Claude Code already asks for it
* a playbook whose steps did not fit the work, or work no playbook covered
* a rule in a writing skill that misfired on the surface you were writing

**Say nothing where none of that happened.** Most sessions have no finding. One invented to fill the space costs the
developer a read and a check, and leaves them nothing.

**Raise no issue either.** The file is in front of you both, so the developer decides on the spot whether the edit is
worth making, and says so where it is.

## Before you raise a pull request

**The changelog entry and the version are two separate calls, and only the second is yours to ask about.**
[`std-CI`](examples/dog-fooding/standards/workflows.md) carries the entry, the version, the `content-version` each
corpus you changed owes, and the lock every consumer of that corpus owes back. Whether the entry you wrote ships is the
question that belongs to whoever owns the branch. A push to
`main` publishes `kac` whenever [`tooling/kac/kac.csproj`](tooling/kac/kac.csproj) names a version nuget.org does not
already hold, so moving `<Version>` **is** the release. Put the call to them before you open the pull request, with a
recommendation: release where the change stands on its own, and hold where it is one part of a group that is no use
apart. Where the tool did not change there is nothing to ask.

`ChangelogTests` fails a version that has no section. A local run passes over the consumer's lock, because `.imports/`
is untracked and a restore keeps a folder already holding the version it resolved to. Delete `.imports/` in
`examples/payments` and `examples/dog-fooding`, repack the producer, and restore again to see what CI sees.

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
