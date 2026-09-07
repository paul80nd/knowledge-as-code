---
id: rbk-goldens-disagree
type: runbook
tier: procedural
status: active
applies-to: [ svc-kac ]
severity: sev3
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
  - A .NET 10 SDK and a checkout of this repository
owner: paul.law
tags: [ goldens, testing ]
---

# The golden suite reports a diff nobody meant

`Runbook: rbk-goldens-disagree` `ACTIVE`

## Symptoms

* `dotnet run tooling/kac-tests.cs` exits 1 and prints one of:
  * `FAIL <name>: committed generated files do not match the generator.`
  * `the export no longer writes <path>: what a consumer reads has changed`
  * `no expectation for <what> at <file>. run with --update, then read the diff`
* The golden suite passes and `dotnet test tooling/kac.tests` fails, or the other way round.
* The branch is green on your machine and red in CI.

## Immediate actions

**Do not run `--update` before you have read what changed.** Regenerating a golden blesses whatever the tool now
emits, and the export golden is a published contract a consumer reads.

1. Read the scenario name out of the failing line.
2. Run that scenario on its own:

   ```sh
   dotnet run tooling/kac-tests.cs -- <name>
   ```

3. Note whether the run names a generated file, an export path, or a missing expectation.

## Diagnosis

**Does the failure reproduce on your machine?**

* **Yes** → continue.
* **No** → the three layers assert different things about the same corpus, so a regenerated golden can leave you green
  here and red in CI. Go to [Resolution](#resolution) and start at step 4.

**Did you change `.schema/`?**

* **Yes** → every type page's generated tables move with it, and so do the `generate` scenarios. The diff is expected.
  Go to [Resolution](#resolution).
* **No** → continue.

**Did you change what `kac export` or `kac pack` writes?**

* **Yes** → the diff is a change to what a consumer reads. Confirm you meant it, then go to
  [Resolution](#resolution).
* **No** → continue.

**Does the failing line say the export stopped writing a path?**

* **Yes** → a consumer that reads that path breaks. Restore the path rather than the golden, and
  [escalate](#escalation) if you cannot.
* **No** → [escalate](#escalation).

## Resolution

1. Regenerate the one scenario that failed, never the whole set:

   ```sh
   dotnet run tooling/kac-tests.cs -- --update <name>
   ```

2. Read the diff line by line:

   ```sh
   git diff tooling/tests/fixtures/<name>
   ```

3. Confirm every moved line is one your change was meant to move. A line you cannot account for is a regression you
   have just blessed.
4. Run all three test layers, one at a time:

   ```sh
   dotnet test tooling/kac.tests
   dotnet test tooling/kac.features
   dotnet run tooling/kac-tests.cs
   ```

5. Run `kac generate` in each corpus you changed, and in `template/`.
6. Say in the commit message why the golden moved.

Confirmed when all three layers pass and the diff carries only lines you can account for.

## Escalation

| When                                                         | Who                           | How                         |
|--------------------------------------------------------------|-------------------------------|-----------------------------|
| The export dropped a path and you cannot restore it          | Paul Law, as the tool's owner | An issue on this repository |
| The three layers disagree and you cannot tell which is right | Paul Law, as the tool's owner | An issue on this repository |

## Afterwards

* Raise no postmortem. This is a gate doing its job.
* Where the diff turned out to be a regression, add the case that would have caught it before regenerating anything.

## Related

* [svc-kac] is what this covers.
* [ctl-0009] is the check that runs the three layers.

[ctl-0009]: ../controls/0009-tool-test-layers.md
[svc-kac]: ../services/kac.md
