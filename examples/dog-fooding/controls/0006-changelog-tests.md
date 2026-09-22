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
review-by: "2027-09-22"
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

`ChangelogTests` in `tooling/kac.tests` runs three tests:

* The first reads the `<Version>` element from `tooling/kac/kac.csproj` and looks for a `## <version> - <date>`
  heading in `tooling/kac/CHANGELOG.md`.
* The second fails a heading with nothing under it, which is a section added to satisfy the first test and left empty.
* The third reads `docs/index.md`, which tells a reader the tool is early because it has not reached 1.0.0. It fails
  that sentence once the version reaches 1.0.0.

The `tool` job runs all three under `dotnet test tooling/kac.tests`.

## Coverage and gaps

These tests cover the section for the version in front of them. A reviewer judges whether that section describes the
change, and a reader would notice it in the published release.

The first test also covers the rename. A `<Version>` that moved without a section of its own fails, so a release
cannot ship with its entries still under `## Unreleased`.

Nothing checks that a change to `kac` took its line under `## Unreleased`, and nothing checks that the branch owner
decided to release before `<Version>` moved. A reviewer catches both.

[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
