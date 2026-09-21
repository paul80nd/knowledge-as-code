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
owner: human:paul.law
tags: [ corpus, versioning ]
---

# Every corpus whose records changed has moved its content-version

`Control: ctl-0011` `ACTIVE`

`publish-corpus.yml` publishes each corpus at the `content-version` its descriptor states, and a registry never
replaces a version it has taken.

## What it checks

* [std-CI.a-version-moves-by-hand-and-publishes-once] says "`content-version` in a corpus's `.corpus.yaml` **MUST**
  move in the pull request changing what that corpus knows".

## How it works

`ContentVersionTests` in `tooling/kac.tests` compares the branch against the commit it grew from, in four steps:

* It asks `git diff` for every file the branch added, changed, renamed or deleted since the merge base.
* It asks `git ls-files` for a file the working tree keeps that nobody has committed, which `git diff` never reports.
* It keeps the files that are records of a type declaring an `export:` block, in a corpus under `examples/`.
* For each corpus left, it reads `content-version` from `.corpus.yaml` at both revisions, and fails where the stamp
  did not rise.

The `tool` job runs it under `dotnet test tooling/kac.tests`. That job checks out with `fetch-depth: 0` and names the
target branch in `KAC_BASE_REF`, because a shallow checkout keeps no base to compare against. A job that names a
branch git cannot reach fails rather than passing on a comparison it never made.

## Coverage and gaps

This check reports whether the stamp rose, and never how far. [std-VERS] says which part a change moves, and a
reviewer judges whether a patch should have been a minor or a major.

The reach is a whole record, which is wider than the obligation.
[std-VERS.what-a-move-of-each-stamp-means] says "A change confined to a section a record's type leaves out of
`export:` **MUST NOT** move `content-version`", and this reads the file rather than the sections. So a rewritten
`Rationale and provenance` on a standard, or a frontmatter key no `export:` block names, fails here where that clause
forbids the move.

It is narrower than the obligation in two places. The same clause says "A change to a field's accepted value format,
its name, or its removal **MUST** move `content-version`'s major where the change reaches a record whose type names
that field under `export:`", and a `.schema/` edit moves no record file. It also says "`content-version` **MUST** move
where a skill in the bundle changes what it tells a reader to do", and the skill tree sits in no corpus's folder.

Only the corpora under `examples/` are read. `template/` states no `content-version`, and a fixture corpus under
`tooling/` states one as part of the fixture.

A local run with no base branch reads nothing and passes, which is what a run on `main` does. The answer arrives on
the pull request.

[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
[std-VERS]: ../standards/versioning.md
[std-VERS.what-a-move-of-each-stamp-means]: ../standards/versioning.md#what-a-move-of-each-stamp-means
