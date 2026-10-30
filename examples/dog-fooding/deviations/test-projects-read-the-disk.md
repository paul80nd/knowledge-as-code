---
id: dev-test-projects-read-the-disk
type: deviation
tier: normative
status: active
departs-from:
  - eng:std-NETTST.LAYOUT
  - eng:std-NETTST.UNIT
risk: low
accepted-on: "2026-10-23"
review-by: "2027-10-23"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ dotnet, testing ]
---

# The tool's test projects read the disk and carry no .Tests suffix

`Deviation: dev-test-projects-read-the-disk` `ACTIVE`

`kac`'s three test projects read the filesystem, and none is named with a `.Tests` or `.IntegrationTests` suffix. This
stands until the projects are split by what their tests read.

## What we are doing instead

`eng:std-NETTST.LAYOUT` asks for the suffix, and `eng:std-NETTST.UNIT` forbids a unit test to read the filesystem.

| Project                | Layer   | What it reads or starts                                       |
|------------------------|---------|---------------------------------------------------------------|
| `tooling/kac.tests`    | unit    | fixture directories, and `git` and `dotnet` processes         |
| `tooling/kac.features` | feature | fixture corpora on disk, and processes in some step files     |
| `tooling/kac.goldens`  | golden  | fixture corpora on disk, and a built `kac` for every fixture  |

Most of `kac.tests` loads its fixtures from directories. `GitCli` starts `git`, and `CliReference` starts `dotnet`.

A `Kind=Repository` trait marks the tests in `kac.tests` that read this repository, and a tight loop leaves them out
with `--filter-not-trait "Kind=Repository"`. `KindTests` fails a test that reads `Repo.Root` without the trait, unless
it is named as one that reads the template to exercise the tool.

## Why we need it

A fixture is a corpus, and a corpus is a folder of files. Loading it from disk runs the same loader `kac` runs.

Renaming the projects to `kac.Tests` and `kac.IntegrationTests` means splitting `kac.tests` by what each test reads. A
large share of its tests read a fixture, so the split leaves a small project beside a large one, and adds a build to
every test run.

## What the risk is

A test that reads the disk can pass on one machine and fail on another, where a path or a line ending differs. A
contributor who expects the suffix may not find which project holds which kind of test.

## What compensates

* CI runs all three projects on `ubuntu-latest` and `windows-latest`, so a test that depends on one machine's disk
  fails there.
* The `Kind` trait and `KindTests` keep the tests that read this repository apart from the tests of `kac`. A tight loop
  gets the fast feedback `eng:std-NETTST.UNIT` is after.
* `tooling/README.md` says what the unit layer contains, and how the `Kind` trait divides it.

## How it closes

`kac.tests` splits into a project whose tests read nothing and a project whose tests read fixtures or start processes,
each named with its suffix. `kac.features` and `kac.goldens` take `.IntegrationTests`. The cost is sorting every test in
`kac.tests` by what it reads, and a fourth project to build in every test run. This record closes on that split. Where
the owner re-accepts it instead, the review weighs whether any contributor has been misled by the missing suffix.

## Scope

The three test projects under `tooling/`. `kac` itself and every corpus here are outside it.

## Related

* [svc-kac] is the tool the three projects test.

[svc-kac]: ../services/kac.md
