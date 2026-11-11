---
id: ctl-tool-pin-tests
type: control
tier: normative
status: active
verifies: [ std-CONFIG.PIN ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - all
review-by: "2027-11-11"
owner: human:paul.law
sources:
  - { resource: https://github.com/paul80nd/knowledge-as-code/issues/1319 }
tags: [ dependabot, pins, tools ]
---

# A tool record's versions contain what this repository pins

`Control: ctl-tool-pin-tests` `ACTIVE`

A pull request that moves a pin outside its tool record's `versions` fails until the record moves with it.

## What it checks

* [std-CONFIG.PIN] says "An `approved` or `trial` tool record's `versions` **MUST** contain each version this
  repository pins its package at".

## How it works

`ToolPinTests` in `tooling/kac.tests` reads every `approved` or `trial` tool record in this corpus that has `packages`.
`RepositoryPins` finds each package's pins in these files:

| Package URL              | Pinned in                                                                    |
|--------------------------|------------------------------------------------------------------------------|
| `pkg:nuget/...`          | a `PackageReference` in a project under `tooling/`                           |
| `pkg:pypi/...`           | a line in `docs/requirements.txt` or `.github/requirements.txt`              |
| `pkg:generic/dotnet-sdk` | `global.json`, and the SDK version in each starter pipeline                  |
| `pkg:github/...`         | a `uses:` line in a workflow, by the version in the comment after its commit |
| `pkg:golang/...`         | a `go install` line in a workflow                                            |

The workflows it reads are those under `.github/workflows/` and `template/.github/workflows/`. For each package it does
three things:

* It fails where a pin falls outside `versions`. A range ending `.x`, such as `7.x`, contains every version under that
  prefix. Any other value is one exact version, such as `10.0.403`.
* It fails where `versions` is absent and something pins the package.
* It fails where `versions` is present and nothing pins the package.

The `tool` job runs it under `dotnet test --project tooling/kac.tests` on every pull request, Dependabot's included.
It reads the files as they stand, so it needs no base branch.

## Coverage and gaps

It proves the record's `versions` field, and nothing in the record's body. A body naming the old version passes, so a
reviewer still reads the prose of a record whose pin moved.

It reads the pins listed above and no others. A package pinned anywhere else, such as a new manifest or a container
image, passes unread until `RepositoryPins` learns the file. A package nothing pins and whose record gives no
`versions` passes, as `tol-claude-code-cli` and `tol-gh` do.

It reads this repository alone. `drift-against-manifests` would compare the register with every repository in the
estate, and it is declared and does not run.

[std-CONFIG.PIN]: ../standards/configuration.md#pin-a-pin-names-an-exact-version
