# Working in this repository

This repository is not a corpus. It holds `kac`, the documentation site published beside it, the schema every corpus
below is judged against, the template `kac new` sends a corpus, and four worked corpora under `examples/`. Three of
them stand in for corpora that took the framework, and `dog-fooding` holds this repository to it. `trials/` keeps the
experiments that measure the site, with every run. The tool finds a corpus by walking up for a `.corpus.yaml`, so it
reads one of the corpora under `examples/`, or `template/`, and never this root.

**Load `i-want-to` before you plan.** It routes the work to the process or playbook with its steps, and picks the
writing skill for the surface you are on.

| Working on                                  | Read                                                                             |
|---------------------------------------------|----------------------------------------------------------------------------------|
| a record, or anything else a corpus holds   | [`examples/README.md`](examples/README.md)                                       |
| the schema, or a rule it declares           | [`.schema/CLAUDE.md`](.schema/CLAUDE.md)                                         |
| `kac`, its checks, or the tests behind them | [`tooling/CLAUDE.md`](tooling/CLAUDE.md)                                         |
| a page of the documentation site            | [`tooling/README.md`](tooling/README.md#the-documentation-site) and `mkdocs.yml` |
| a trial, or a run of one                    | [`trials/README.md`](trials/README.md)                                           |

[`template/CLAUDE.md`](template/CLAUDE.md) goes to every corpus that copies the template, and describes that corpus.
Read it as content you may need to change, not as guidance for this repository.

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

That check answers in both directions: an overlay copy that differs, and a file the corpus holds that the template sends
nothing to. A type page and a `_template.md` are seeds, which it never compares, so copy each by hand and `diff` it
against `template/`.

## Before you commit

Run [one `kac` at a time](docs/maintaining/build-test-release.md#run-kac-from-source). In each corpus you changed
under `examples/`, run these. In `template/`, which is one level up, write `../` for each `../../`.

```sh
dotnet run --project ../../tooling/kac -- validate         # the corpus
dotnet run --project ../../tooling/kac -- generate --check # generated output is fresh
dotnet run --project ../../tooling/kac -- update --check --from ../../
```

A corpus declaring `consumes:` needs `restore` ahead of `validate`, and `restore` needs a package to take. That corpus's
own `CLAUDE.md` names the producer to `export` and `pack` first.

Where a change reaches what `bundle` writes, run
[Check the plugin](docs/maintaining/build-test-release.md#check-the-plugin) in each corpus whose plugin it reaches.
Where it changes what an export publishes, such as a part's key, an anchor or a field a skill reads, also run the
[round-trip scripts](docs/maintaining/build-test-release.md#the-round-trip-scripts). `round-trip.sh` fetches from
GitHub at the commit it builds from, so push first.

Where `kac` or a test changed, run the unit, feature and golden
[test layers](docs/maintaining/build-test-release.md#the-three-test-layers). Each asserts something the others do not,
so goldens regenerated alone can pass locally and fail in CI. Where a YAML file or a workflow changed, run the
[linters](docs/maintaining/build-test-release.md#check-the-site-and-the-configuration).

## Your working style

Say in one sentence what you are about to do before your first tool call. While working, report what you found or where
you changed direction, and nothing else. Finish by leading with the outcome (what happened, or what you found) and put
the supporting detail after it.

## Ask this repository's own corpus

`example-dogfooding` is what this repository knows about itself, installed here as a plugin. The rules you build to are
records in it. Each of its skills says in its description when to ask it.

| Before you change                                       | Read                                                                                                                                                              |
|---------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| a workflow, a version or a changelog                    | [`std-CI`](examples/dog-fooding/standards/workflows.md)                                                                                                           |
| a version stamp, a consumer's lock or its range         | [`std-VERS`](examples/dog-fooding/standards/versioning.md)                                                                                                        |
| a YAML file, a pin, or a file two of the trees hold     | [`std-CONFIG`](examples/dog-fooding/standards/configuration.md)                                                                                                   |
| any prose, comment, commit message or generated block   | [`std-PROSE`](examples/dog-fooding/standards/prose.md)                                                                                                            |
| a skill, a hook or a `plugin.json` under `.plugin/`     | [`std-PLUGIN`](examples/dog-fooding/standards/plugin.md)                                                                                                          |
| a page of the documentation site, or what `kac` prints  | [`std-A11Y`](examples/dog-fooding/standards/accessibility.md)                                                                                                     |
| C# or a test under `tooling/`                           | [`eng:std-CSSTY`](examples/engineering/standards/platform/dotnet/code-style.md) and [`eng:std-NETTST`](examples/engineering/standards/platform/dotnet/testing.md) |

`eng:std-CSSTY` and `eng:std-NETTST` are authored in `examples/engineering` and arrive through `consumes:`, so a clause
of either is cited as `eng:`.

[`.claude/settings.json`](.claude/settings.json) turns the plugin on from the `marketplace` branch, so a clone has it
without anyone adding a marketplace by hand.

**`harvest-findings` is in the working tree, under
[`.claude/skills/`](.claude/skills/harvest-findings/SKILL.md).** It triages the findings already filed and drafts the
record one of them asks for. Ask it before you plan a session's work here, because a finding nobody triaged is work
nobody has seen. Start it from inside a corpus, such as `examples/dog-fooding`, because this root has no `.corpus.yaml`.

**Use the skills, and then raise a finding where they let you down.** This is the one corpus whose subject is the
repository you are working in, so a session here is the only reader who can tell what it is missing. Raise one for
whichever of these happened:

* a question one of the skills should have answered and could not
* a record that is missing, or one whose wording sent you the wrong way
* a lookup that would have helped, that you only thought of afterwards
* something the plugin cannot do that would have made it worth reaching for
* something about the agent configuration under [`.claude/agents-config/`](.claude/agents-config/), which a session
  meets the same way and by using it

**Load `raise-finding` and let it write the issue.** It writes the body, the provenance and the label in the shape the
next run reads. Say what you are about to raise in the reply that closes the session, and let the developer answer.

**The clause owner here is the developer you are talking to.** A consumer elsewhere files a deviation request because
they cannot reach that person. You can, so put the departure to them in the conversation, and let `request-deviation`
write the issue where they say one belongs. The record itself lands in
[`examples/dog-fooding/deviations/`](examples/dog-fooding/deviations/) once they have accepted it.

**Place what you filed on the board.** The skill files the issue and stops, because a corpus elsewhere has no board for
it to know about. This repository does: project 3, where each issue takes a `Status`, a `Phase` and a `Rank`. The board
sorts each `Phase` lane by `Rank`, so an issue without one sits in no order. In a lane with an epic, the epic is `1`
and its stories count up from `2` in delivery order. An issue left off the board is one the developer never sees.

**The board runs by stage, and each stage lane has one epic.** A lane's name starts with its stage: `1` before the
trial, `2` the trial, `3` the route to v1 and `4` future functionality. The stages run in that order. Every issue in a
stage lane is a sub-issue of that lane's epic alone, and the epic closes when its lane drains.

**A finding goes to `Intake`.** Triage then moves it into the stage lane it is worth delivering with, or into
`4 Future functionality`. Neither `Intake` nor `4 Future functionality` has an epic. `addSubIssue` takes a GraphQL node
id for both issues, and `replaceParent: true` moves an issue from another epic.

**The export is frozen at bundle time, and the branch serves what is on `main`.** A branch editing
`examples/dog-fooding` leaves the installed plugin behind, so a lookup can answer with a record that branch has already
changed. [That corpus's own README](examples/dog-fooding/README.md#reading-this-corpus-as-a-plugin-while-you-change-it)
carries the loop that installs the working tree instead, and says what to rebuild after a record changes and after a
skill does.

## Where a page or a skill let you down

The plugin is a read-only copy, so an issue is the only way an agent writes back to it. The guidance is not. The
`CLAUDE.md` files, the playbooks under [`.claude/skills/i-want-to/`](.claude/skills/i-want-to/) and the four writing
skills all sit in the working tree, one edit from whoever is reading your reply.

Tell the developer, in the reply that closes the session, whichever of these happened:

* an instruction that sent you the wrong way, or that you had to read twice before you could act on it
* a question one of those pages should have answered and did not
* an instruction you would have followed anyway, because Claude Code already asks for it
* a playbook whose steps did not fit the work, or work no playbook covered
* a rule in a writing skill that misfired on the surface you were writing

**Say nothing where none of that happened.** Most sessions have nothing to say here. One invented to fill the space
costs the developer a read and a check, and leaves them nothing.

**Raise no issue either.** The file is in front of you both, so the developer decides on the spot whether the edit is
worth making, and says so where it is.

## Before you raise a pull request

[What a pull request owes](docs/maintaining/contributing.md#what-a-pull-request-owes) lists the changelog entry, the
`content-version` and the consumer locks. [`std-CI`](examples/dog-fooding/standards/workflows.md) and
[`std-VERS`](examples/dog-fooding/standards/versioning.md) state the rules behind them.

**Write the body with `mattpocock-skills:pr`**, so every pull request has the same shape. List each test layer you ran,
and what it reported, under its Evidence. After Merge Danger, say what you did not do and why, then give the `Closes`
line.

**[The release](docs/maintaining/build-test-release.md#release-a-version) is the developer's call.** Where `kac`
changed, put it to them before you open the pull request, with a recommendation. Work usually runs one lane at a time,
and a release is cut once that lane drains. Recommend releasing sooner only where the change stands on its own and
somebody needs it before then.

A local run passes over the consumer's lock, because `.imports/` is untracked and a restore keeps a folder already
holding the version it resolved to. Delete `.imports/` in `examples/payments` and `examples/dog-fooding`, repack the
producer, and restore again to see what CI sees.

## Agent skills

The engineering skills read their configuration from [`.claude/agents-config/`](.claude/agents-config/), rather than the
`docs/agents/` their author assumes. `docs/` here is the published site, and `NavigationTests` fails a page the nav does
not list. Three files carry that configuration:

* [`issue-tracker.md`](.claude/agents-config/issue-tracker.md): issues live as GitHub issues on
  `paul80nd/knowledge-as-code`, reached with the `gh` CLI.
* [`triage-labels.md`](.claude/agents-config/triage-labels.md): the five canonical roles, each label string equal to
  its name.
* [`domain.md`](.claude/agents-config/domain.md): single-context. The `CLAUDE.md` files describe the domain, and
  `glossary-lookup` stands in for the `GLOSSARY.md` the skills read and write.
