---
id: ctl-0011
type: control
tier: normative
status: active
verifies: [ std-CI ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - all
review-by: "2027-09-22"
owner: human:paul.law
tags: [ corpus, versioning ]
---

# Every corpus whose export changed has moved its content-version

`Control: ctl-0011` `ACTIVE`

`publish-corpus.yml` publishes each corpus at the `content-version` its descriptor states, and a registry never
replaces a version it has taken.

## What it checks

* [std-CI.a-version-moves-by-hand-and-publishes-once] says "`content-version` in a corpus's `.corpus.yaml` **MUST**
  move in the pull request changing what that corpus knows".

## How it works

`ContentVersionTests` in `tooling/kac.tests` compares what each corpus exports on the branch with what it exported at
the commit the branch grew from, in four steps:

* It asks `git diff` and `git ls-files` for every file the branch added, changed, renamed or deleted, committed or not.
* It skips a corpus under `examples/` where nothing in its folder or in `.schema/` changed.
* It builds that corpus's export twice in memory, with the branch's exporter both times. The first build reads the
  records, `.corpus.yaml` and `.schema/` as git stored them at the merge base. The second reads the working tree.
* Where the two exports differ by a byte outside `mechanismVersion`, it reads `content-version` from `.corpus.yaml` at
  both revisions, and fails where the stamp did not rise.

Both builds run one exporter, so a change to `kac` alone changes no byte and asks for no move. `PublishedExportTests`
checks the comparison in each direction against corpora built in memory.

The `tool` job runs it under `dotnet test tooling/kac.tests`. That job checks out with `fetch-depth: 0` and names the
target branch in `KAC_BASE_REF`, because a shallow checkout keeps no base to compare against. A job that names a
branch git cannot reach fails rather than passing on a comparison it never made.

## Coverage and gaps

This check reports whether the stamp rose, and never how far. [std-VERS] says which part a change moves, and a
reviewer judges whether a patch should have been a minor or a major.

The reach is the export. [std-VERS.what-a-move-of-each-stamp-means] says "A change confined to a section a record's
type leaves out of `export:` **MUST NOT** move `content-version`". A rewritten `Rationale and provenance` on a standard,
or a frontmatter key no `export:` block names, leaves the export as it was and passes. A `.schema/` edit that changes
what a record exports fails without a move.

A template take is left out. It moves `upstream.template-version`, which `manifest.json` publishes as
`mechanismVersion`. Both builds drop `mechanismVersion` before they compare, because it is a stamp of its own and no
record moved with it.

The export compared is the corpus's own records. `.imports/` is untracked, so git keeps no copy of what a consumer had
restored at the merge base, and neither build reads it. A consumer whose inherited records changed is asked for no
move here. [#1084](https://github.com/paul80nd/knowledge-as-code/issues/1084) tracks that gap.

A reviewer checks the bundled skills. The same clause says "`content-version` **MUST** move where a skill in the bundle
changes what it tells a reader to do". A test can see that a skill file changed, and not whether what it tells a reader
changed.

Only the corpora under `examples/` are read. `template/` states no `content-version`, and a fixture corpus under
`tooling/` states one as part of the fixture.

A local run with no base branch reads nothing and passes, which is what a run on `main` does. The answer arrives on
the pull request.

[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
[std-VERS]: ../standards/versioning.md
[std-VERS.what-a-move-of-each-stamp-means]: ../standards/versioning.md#what-a-move-of-each-stamp-means
