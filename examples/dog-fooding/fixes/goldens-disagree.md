---
id: fix-0003
type: fix
tier: normative
status: draft
symptom-keywords: [ ci, "committed generated files do not match the generator", github_actions, golden, kac.goldens,
  kac_update_goldens, "no golden", "passes locally", snapshot, "the export no longer writes" ]
applies-to: [ svc-kac ]
review-by: "2027-04-29"
owner: human:paul.law
sources:
  - { resource: https://github.com/paul80nd/knowledge-as-code/issues/1046 }
tags: [ goldens, testing ]
---

# The golden suite reports a diff nobody meant

`Fix: fix-0003` `DRAFT`

## Symptom

`dotnet test --project tooling/kac.goldens` reports `failed kac.goldens.GoldenTests.Scenario(name: "<name>")`. Under
it is a line opening `FAIL` and naming the scenario. What differs follows on the same line or on the indented lines
under it. For example:

* `committed generated files do not match the generator.`, on the `FAIL` line itself
* `the export no longer writes <path>: what a consumer reads has changed`
* `the export writes <path>, which the committed copy does not hold: a new file for a consumer`
* `<path> differs from the committed export: <the first difference>`
* `no expectation for <what> at <file>. run with KAC_UPDATE_GOLDENS=1, then read the diff`

The suite can also print `MISSING <name>: no golden.` Two variants share the cause: one of `kac.goldens` and
`kac.tests` passes while the other fails, and the branch is green on your machine and red in CI.

## Environment

A checkout of this repository with a .NET 10 SDK, on a branch that changed `.schema/`, `tooling/`, or what
`kac export` or `kac pack` writes. CI sets `GITHUB_ACTIONS` and compares the branch with the pull request's base.

## Cause

The tool on this branch emits output that differs from the committed golden. A change to `.schema/`, to what the export
writes, or to a check or its message moves a golden. Where only CI fails, the local run differed from CI's: it ran
without `GITHUB_ACTIONS`, or against an `origin/main` older than the pull request's base.

## Resolution

**Do not set `KAC_UPDATE_GOLDENS` before you have read what changed.** Regenerating a golden accepts whatever the tool
now emits, and the export golden is a contract a consumer reads.

Run each command from the root of the repository.

1. Run the failing scenario on its own, the way CI runs it. The filter matches the whole name.

   ```sh
   GITHUB_ACTIONS=true dotnet test --project tooling/kac.goldens --filter-trait "Scenario=<name>"
   ```

   A failed scenario prints one line opening `FAIL` or `MISSING`. Where it passes, or only `kac.tests` failed, fetch
   `main` and run the unit layer against it. Each failing test's message says what to change.

   ```sh
   git fetch origin main
   KAC_BASE_REF=origin/main dotnet test --project tooling/kac.tests
   ```

2. Where a line under `FAIL` says `the export no longer writes`, restore that path in the tool and leave the golden
   alone. A consumer that reads that path breaks.
3. Regenerate the scenario that failed.

   ```sh
   GITHUB_ACTIONS=true KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens --output Detailed --filter-trait "Scenario=<name>"
   ```

   It prints one line opening `UPDATE` under the scenario. The run passes even where that line says
   `WARNING: export failed` or `WARNING: still stale after regen`, so read the line for `WARNING`.

4. Read the diff, and list the files the update added.

   ```sh
   git diff tooling/tests/fixtures/
   git status --short tooling/tests/fixtures/
   ```

5. Confirm every moved line is one your change was meant to move. A line you cannot account for is a regression you
   have just accepted. Add the case that would have caught it before you regenerate again.
6. Run all three test layers, one at a time. Each prints `Test run summary: Passed!`.

   ```sh
   dotnet test --project tooling/kac.tests
   dotnet test --project tooling/kac.features
   GITHUB_ACTIONS=true dotnet test --project tooling/kac.goldens
   ```

7. Run `dotnet run --project ../../tooling/kac -- generate` in each corpus you changed under `examples/`, and in
   `template/` with `../tooling/kac` as the project.
8. Say in the commit message why the golden moved.

It worked when all three layers pass and the diff contains only lines you can account for. To undo step 3, run
`git checkout -- tooling/tests/fixtures/`, which also discards any other uncommitted change there. List the files it
leaves with `git clean -n tooling/tests/fixtures/`, and delete each one you did not write. Where an update prints a
`WARNING`, or the three layers disagree, raise an issue on this repository for the tool's owner.

## Why it happens

A golden records what the tool emits for a scenario: the generated files, the export, and the check and message of
every finding. Every type page's generated tables move with `.schema/`, so a schema change moves the `generate`
scenarios. The suite is a gate on purpose, and it only works where somebody reads the diff before accepting it.

## Related

* [svc-kac] is the service affected.
* [ctl-0009] is the check that runs the three layers.

[ctl-0009]: ../controls/0009-tool-test-layers.md
[svc-kac]: ../services/kac.md
