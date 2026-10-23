# Contributing to `kac`

This repository is not open to contributions of code. Raise an issue instead: a bug, a contradiction, a question or a
correction of fact. A pull request opened without a prior conversation is closed unmerged. Every change, the
maintainer's included, is merged into `main` through a pull request that passes the `validate` check.

```sh
gh issue create --repo paul80nd/knowledge-as-code   # raise the bug, contradiction or question first
```

[`.github/CONTRIBUTING.md`](https://github.com/paul80nd/knowledge-as-code/blob/main/.github/CONTRIBUTING.md) says why
the repository works this way. To change `kac` without waiting on anybody, read [Forking `kac`](forking.md).

This page is about changing `kac` and this repository. [Contributing knowledge](../framework/contributing.md) covers
adding a record (one Markdown document) to a corpus (one repository of records).

## What an issue is for

An issue is welcome for any of these:

* A bug in `kac`.
* A rule that contradicts another, or fails on a real corpus.
* A question about the taxonomy, the update model, or why something has its shape.
* A page that says something the tool does not do.

## What a pull request owes

Branch off `main`. A push to `main` is rejected. Before you open the pull request:

1. Fix every page your change made wrong. A change to a command usually changes its page under `docs/use/cli/`.
2. Where `kac` changed, add an entry under `## Unreleased` in `tooling/kac/CHANGELOG.md`.
3. Leave `<Version>` alone. Moving it publishes a release, and that is the owner's decision.
4. Where a corpus's own records or schema change what it exports, or its plugin bundle includes a skill, a hook or a
   manifest you changed, move `content-version` in that corpus's `.corpus.yaml`. Where that corpus has consumers, move
   their `resolved:` lock and their own `content-version` in the same pull request. A change to `kac` moves no
   `content-version`, even where the exported bytes change.
   [std-VERS](https://github.com/paul80nd/knowledge-as-code/blob/main/examples/dog-fooding/standards/versioning.md#which-stamps-move-together-and-which-move-alone)
   states the rule.
5. Run the [test layers](build-test-release.md#the-three-test-layers) your change touches.
6. Put a change of behaviour in a commit of its own.

The commit subject says what changed. The body and the pull request description say why, and list each test layer you
ran with its result.

## The checks that gate a merge

`kac.yml` runs on every pull request into `main`. Its `validate` job waits for every other job, and the branch rule
requires `validate`.

| Job                 | What it runs                                                                                            |
|---------------------|---------------------------------------------------------------------------------------------------------|
| `corpora`           | `validate`, `generate --check`, `update --check`, `export`, `bundle` and `pack` in each corpus          |
| `tool`              | The template's own checks, the unit, feature and golden layers, and a corpus created by the packed tool |
| `lint`              | `yamllint --strict`, `actionlint` and `zizmor`                                                          |
| `docs`              | `mkdocs build`, which fails on a dead link                                                              |
| `round-trip`        | `tooling/tests/round-trip.sh` over `library`, `engineering` and `payments`, on Linux and Windows                                  |
| `import-round-trip` | `tooling/tests/import-round-trip.sh`, on Linux and Windows                                              |

A job missing from the `validate` job's `needs:` still runs but blocks nothing. `WorkflowGateTests` fails one.

## Where to go next

[How `kac` is put together](architecture.md) shows where a change to `kac` goes.
