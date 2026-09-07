---
id: ctl-0006
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
tags: [ changelog, releases, versioning ]
---

# A version carries the changelog section its release publishes

`Control: ctl-0006` `ACTIVE`

`publish-tool.yml` opens a GitHub release whose body is the changelog's section for the version it published.

## What it checks

* [std-CI.a-version-moves-by-hand-and-publishes-once] says the release notes "**MUST** be that version's section of
  `tooling/kac/CHANGELOG.md`".

## How it works

`ChangelogTests` in `tooling/kac.tests` reads the `<Version>` element from `tooling/kac/kac.csproj` and looks for a
`## <version> - <date>` heading in `tooling/kac/CHANGELOG.md`. A second test fails that heading where nothing sits
under it, which is the section added to satisfy the first and left empty. A third reads `docs/index.md`, which tells
a reader the tool is early because it has not reached 1.0.0, and fails that sentence once the version does.

The `tool` job runs all three under `dotnet test tooling/kac.tests`.

## Coverage and gaps

The reach is the section for the version in front of it. Whether that section describes the change is a reviewer's
judgement, and the release it becomes is where a reader would notice.

Nothing checks that `<Version>` moved at all. A pull request shipping a tool change under a version nuget.org already
holds passes here, and its publish finishes green having published nothing.

[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
