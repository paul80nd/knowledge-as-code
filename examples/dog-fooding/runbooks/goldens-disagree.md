---
id: rbk-goldens-disagree
type: runbook
tier: procedural
status: active
applies-to: [ svc-kac ]
severity: sev3
last-rehearsed: "2026-10-18"
rehearsal-frequency: on-change
rehearsal-method: tabletop
requires-tools:
  - A .NET 10 SDK
  - A checkout of this repository
owner: human:paul.law
tags: [ goldens, testing ]
---

# The golden suite reports a diff nobody meant

`Runbook: rbk-goldens-disagree` `ACTIVE`

## Symptoms

* `dotnet test --project tooling/kac.goldens` reports `failed kac.goldens.GoldenTests.Scenario(name: "<name>")`.
  Under it is a line opening `FAIL` and naming the scenario. What differs follows on the same line or on the indented
  lines under it. For example:
  * `committed generated files do not match the generator.`, on the `FAIL` line itself
  * `the export no longer writes <path>: what a consumer reads has changed`
  * `the export writes <path>, which the committed copy does not hold: a new file for a consumer`
  * `<path> differs from the committed export: <the first difference>`
  * `no expectation for <what> at <file>. run with KAC_UPDATE_GOLDENS=1, then read the diff`
* It prints `MISSING <name>: no golden.`
* One of `dotnet test --project tooling/kac.goldens` and `dotnet test --project tooling/kac.tests` passes while the
  other fails.
* The branch is green on your machine and red in CI.

## Impact

Nothing in production. The branch stays red, so nothing on it merges until the diff is accounted for.

## Immediate actions

**Do not set `KAC_UPDATE_GOLDENS` before you have read what changed.** Regenerating a golden accepts whatever the tool
now emits. The export golden is a contract a consumer reads.

1. Read the scenario name from the line opening `FAIL`. Where only `kac.tests` failed, there is no name, so run step 2
   without the filter.
2. Run that scenario on its own, from the root of the repository, the way CI runs it:

   ```sh
   GITHUB_ACTIONS=true dotnet test --project tooling/kac.goldens --filter-trait "Scenario=<name>"
   ```

   CI sets `GITHUB_ACTIONS`, and a tool the suite runs can read it, so a run without it can pass where CI fails. The
   filter matches the whole name, so it runs that one scenario. A failed scenario prints one line opening `FAIL` or
   `MISSING`.

3. Note whether each `FAIL` reports a generated file, an export path, or a missing expectation.

## Diagnosis

**Does a line under `FAIL` say `the export no longer writes`?**

* **Yes** → a consumer that reads that path breaks. Restore the path and leave the golden alone. If you cannot restore
  it, [escalate](#escalation).
* **No** → continue.

**Did only `kac.tests` fail, and does it fail on your machine as well?**

```sh
dotnet test --project tooling/kac.tests
```

* **Yes** → the golden suite is not the cause. The failing test's message says what to change. Change it, then go to
  [Resolution](#resolution) and start at step 4.
* **No** → continue.

**Did step 2 of the immediate actions reproduce the failure?**

* **Yes** → continue.
* **No** → the unit layer's history guards compare your branch with `origin/main`, which is only as new as your last
  fetch. CI compares it with the pull request's base. Fetch, then run the unit layer against it from the root of the
  repository. `KAC_BASE_REF` makes the guards fail where `origin/main` is missing, as CI does:

  ```sh
  git fetch origin main
  KAC_BASE_REF=origin/main dotnet test --project tooling/kac.tests
  ```

  Where a test now fails, its message says what to change. Change it, then go to [Resolution](#resolution) and start
  at step 4. Where it still passes, [escalate](#escalation) with the CI job's log.

**Did you change `.schema/`?**

* **Yes** → the diff is expected. Every type page's generated tables move with `.schema/`, and so do the `generate`
  scenarios. Go to [Resolution](#resolution).
* **No** → continue.

**Did you change what `kac export` or `kac pack` writes?**

* **Yes** → the diff is a change to what a consumer reads. Confirm you meant it, then go to
  [Resolution](#resolution).
* **No** → continue.

**Did you change a check or a message in `tooling/`?**

* **Yes** → the diff is expected. A validate scenario's golden records the check and the message of every finding.
  Go to [Resolution](#resolution).
* **No** → [escalate](#escalation).

## Resolution

Run steps 1 to 4 from the root of the repository.

1. Regenerate the scenario that failed:

   ```sh
   GITHUB_ACTIONS=true KAC_UPDATE_GOLDENS=1 dotnet test --project tooling/kac.goldens --output Detailed --filter-trait "Scenario=<name>"
   ```

   It prints one line opening `UPDATE` under the scenario it regenerated. The run passes even where that line says
   `WARNING: export failed` or `WARNING: still stale after regen`, so read the line for `WARNING`. If it has one,
   [escalate](#escalation).

   To undo the regeneration, run `git checkout -- tooling/tests/fixtures/`. It also discards any other uncommitted
   change there. An update can write files git does not track yet, and the checkout leaves them. List them with
   `git clean -n tooling/tests/fixtures/`, and delete each one you did not write yourself.

2. Read the diff line by line, and list the files the update added:

   ```sh
   git diff tooling/tests/fixtures/
   git status --short tooling/tests/fixtures/
   ```

3. Confirm every moved line is one your change was meant to move. A line you cannot account for is a regression you have
   just accepted.
4. Run all three test layers, one at a time:

   ```sh
   dotnet test --project tooling/kac.tests
   dotnet test --project tooling/kac.features
   GITHUB_ACTIONS=true dotnet test --project tooling/kac.goldens
   ```

   Each prints `Test run summary: Passed!`.

5. Run `kac generate` in each corpus you changed, and in `template/`. From a corpus under `examples/`:

   ```sh
   dotnet run --project ../../tooling/kac -- generate
   ```

   From `template/`, write `../tooling/kac` for the project. Each run ends `updated <n> of <m> generated file(s).`, or
   `generated files already up to date; nothing written.` where nothing moved. Every file it writes is derived from
   the records, so this step needs no undo.

6. Say in the commit message why the golden moved.

Confirmed when all three layers pass and the diff contains only lines you can account for. If a step does not do what
it says, [escalate](#escalation).

## Escalation

| When                                                          | Who                           | How                          |
|---------------------------------------------------------------|-------------------------------|------------------------------|
| The export dropped a path and you cannot restore it           | Paul Law, as the tool's owner | An issue on this repository  |
| The three layers disagree and you cannot tell which is right  | Paul Law, as the tool's owner | An issue on this repository  |
| An update printed a `WARNING`                                 | Paul Law, as the tool's owner | An issue on this repository  |
| CI fails and the unit layer passes here against `origin/main` | Paul Law, as the tool's owner | An issue, with the job's log |

## Communication

| Who                           | What they need to know                           | How often                 |
|-------------------------------|--------------------------------------------------|---------------------------|
| Anybody waiting on the branch | The suite is red, and whether the diff was meant | Once, as soon as you know |

## Afterwards

* Raise no postmortem. The gate worked as designed.
* Where the diff turned out to be a regression, add the case that would have caught it. Add it before you regenerate
  anything.

## Related

* [svc-kac] is what this covers.
* [ctl-0009] is the check that runs the three layers.

[ctl-0009]: ../controls/0009-tool-test-layers.md
[svc-kac]: ../services/kac.md
