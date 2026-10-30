---
id: ctl-content-version-tests
type: control
tier: normative
status: active
verifies: [ std-CI.RELEASE, std-VERS.CONSUME, std-VERS.MEANING ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ corpus, versioning ]
---

# Every corpus whose export, lock or bundle changed has moved its content-version

`Control: ctl-content-version-tests` `ACTIVE`

`publish-corpus.yml` publishes each corpus at the `content-version` its descriptor states, and a registry never
replaces a version it has taken.

## What it checks

* [std-CI.RELEASE] says "`content-version` in a corpus's `.corpus.yaml` **MUST**
  move in the pull request changing what that corpus knows".
* [std-VERS.MEANING] says "`content-version` **MUST** move where a change reaches a skill, a
  hook or the manifest its bundle includes".
* [std-VERS.CONSUME] says "That consumer **MUST** move its own
  `content-version` wherever its `resolved:` lock moves".

## How it works

`ContentVersionTests` in `tooling/kac.tests` compares each corpus under `examples/` on the branch with the same corpus
at the commit the branch grew from. It asks `git diff` and `git ls-files` for every file the branch added, changed,
renamed or deleted, committed or not. It runs one test for each move below. Each test reads `content-version` from
`.corpus.yaml` at both revisions, and fails a corpus where that move happened and the stamp did not rise.

| Test                          | What counts as a move                                                                                     |
|-------------------------------|-----------------------------------------------------------------------------------------------------------|
| The export changed            | The corpus's export, built twice in memory from its records, `.corpus.yaml` and `.schema/`, differs by a byte outside `mechanismVersion`. The first build reads git at the merge base, and the second reads the working tree. |
| The lock moved                | A `consumes:` entry in `.corpus.yaml` states a different `resolved:` at the merge base and in the working tree. |
| A bundled component changed   | A file under `template/.plugin/`, or under the corpus's own `.plugin/`, changed, and `kac bundle` would copy it into this corpus's plugin. |

The export test skips a corpus where nothing in its folder or in `.schema/` changed. Both of its builds run one
exporter, so a change to `kac` alone changes no byte and asks for no move. `PublishedExportTests` checks the comparison
in each direction against corpora built in memory.

The bundle test decides what the bundle includes with `kac bundle`'s own plan, run over the working tree's plugin tree
and export. That export includes what each corpus consumes, built from the producer's folder here. So an edit to
`controls-lookup` asks for a move in every corpus whose export includes a control, its own or inherited, and in no
other.

The `tool` job runs it under `dotnet test --project tooling/kac.tests`. That job checks out with `fetch-depth: 0` and
names the target branch in `KAC_BASE_REF`, because a shallow checkout keeps no base to compare against. A job that
names a branch git cannot reach fails rather than passing on a comparison it never made.

## Coverage and gaps

This check reports whether the stamp rose, and never how far. [std-VERS] says which part a change moves, and a
reviewer judges whether a patch should have been a minor or a major.

The reach is the export. [std-VERS.MEANING] says "A change confined to a section a record's
type leaves out of `export:` **MUST NOT** move `content-version`". A rewritten `Rationale and provenance` on a standard,
or a frontmatter key no `export:` block names, leaves the export as it was and passes. A `.schema/` edit that changes
what a record exports fails without a move.

A template take is left out. It moves `upstream.template-version`, which `manifest.json` publishes as
`mechanismVersion`. Both builds drop `mechanismVersion` before they compare, because it is a stamp of its own and no
record moved with it.

The export test compares the corpus's own records. `.imports/` is untracked, so git keeps no copy of what a consumer
had restored at the merge base. The lock test reads the `resolved:` lock instead, which [std-VERS] makes
the trigger for a consumer's move. It needs no restore and no package. A producer that moves for a bundled skill alone
still obliges its consumers through their lock, with nothing new in their export.

The bundle test checks that a corpus moved where a changed skill, hook or manifest reaches its bundle. A reviewer judges
whether the change was to wording, which moves the patch, or to what the component tells a reader to do, which moves
the minor. A file the branch deleted is not in the working plugin tree, so deleting a file from a component asks for no
move here.

Only the corpora under `examples/` are read. `template/` states no `content-version`, and a fixture corpus under
`tooling/` states one as part of the fixture.

A local run with no base branch reads nothing and passes, which is what a run on `main` does. The answer arrives on
the pull request.

[std-CI.RELEASE]: ../standards/workflows.md#release-a-version-moves-by-hand-and-publishes-once
[std-VERS]: ../standards/versioning.md
[std-VERS.CONSUME]: ../standards/versioning.md#consume-a-producers-move-obliges-every-consumer-in-the-same-pull-request
[std-VERS.MEANING]: ../standards/versioning.md#meaning-what-a-move-of-each-stamp-means
