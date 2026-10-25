---
id: rbk-restore-cannot-find-version
type: runbook
tier: procedural
status: active
severity: sev3
last-rehearsed: "2026-10-18"
rehearsal-frequency: on-change
rehearsal-method: tabletop
requires-tools:
  - A .NET 10 SDK
  - A checkout of this repository
requires-access:
  - Write access to the branch the fix lands on
owner: human:paul.law
tags: [ imports, restore, versioning ]
---

# kac restore cannot find the version the lock names

`Runbook: rbk-restore-cannot-find-version` `ACTIVE`

## Symptoms

* `kac restore` exits 1 and prints one of these:
  * `restore: ../engineering/.dist/package holds no package for example-engineering <version>.`
  * `restore: 'example-engineering' has no version matching '<range>' at ../engineering/.dist/package.`, followed by
    `It holds <version>.`, or by `That folder holds no package for it.` where the folder is empty.
  * ``restore: there is no folder at ../engineering/.dist/package. `source:` names either a registry's service index
    or a folder holding the packages a producer built.``
* `kac validate` fails in `examples/payments` or `examples/dog-fooding` with `[import-restored]`, and says
  ``.imports/eng/ holds no export. Run `kac restore`.``
* The `corpora` and `round-trip` jobs in `kac.yml` fail at **Restore what this corpus consumes**, for those two corpora
  only. `library` and `engineering` consume nothing, so they pass.
* `kac restore` passes on your machine while CI fails. A restore skips a consumer whose `.imports/` already has the
  locked version, so a stale lock passes locally.

Nothing failed in the producer. `examples/engineering` validates, exports and packs.

## Impact

Nothing in production. `examples/payments` and `examples/dog-fooding` cannot validate, so the `corpora` job fails and
nothing merges.

## Immediate actions

1. Leave `.corpus.yaml` alone until the diagnosis below says which field to move.
2. Delete what the consumers restored earlier, from the root of the repository. CI starts with neither folder:

   ```sh
   rm -rf examples/payments/.imports examples/dog-fooding/.imports
   ```

   It prints nothing. Step 4 writes both folders again, so this step needs no undo.

3. Repack the producer, from `examples/engineering`:

   ```sh
   dotnet run --project ../../tooling/kac -- export
   dotnet run --project ../../tooling/kac -- pack
   ```

   `pack` prints `pack: sealed <n> file(s) as example-engineering <version>, cited as 'eng:'.` That version is the
   producer's `content-version`. `pack` empties `.dist/package` before it writes, so the earlier package is gone. To get
   it back, pack the producer again at its earlier `content-version`.

4. Restore, from `examples/payments` and then from `examples/dog-fooding`:

   ```sh
   dotnet run --project ../../tooling/kac -- restore
   ```

   It prints `restore: example-engineering <version> as 'eng:'.` where the lock and the package agree. Otherwise it
   prints a `restore:` line naming the problem, such as one of those under [Symptoms](#symptoms).

## Diagnosis

**Did step 4 print `restore: example-engineering <version> as 'eng:'.` in both consumers?**

* **Yes** → the lock and the package agree in this checkout. Commit and push `.corpus.yaml` in both consumers, then
  re-run the CI job. If it fails again, [escalate](#escalation) with the job's log.
* **No** → continue.

**Does the message say `there is no folder`?**

* **Yes** → step 3 wrote no package, or `source:` names another folder. Where `source:` in the consumer's
  `.corpus.yaml` is not `../engineering/.dist/package`, set it, then go to [Resolution](#resolution) and start at
  step 6. Otherwise [escalate](#escalation).
* **No** → continue.

**Does the message say `has no version matching`?**

* **Yes** → the range does not admit the version step 3 packed. Go to [Resolution](#resolution) and start at step 2.
* **No** → continue.

**Is the version in the message lower than `content-version` in `examples/engineering/.corpus.yaml`?** Compare the
major, then the minor, then the patch, each as a number: `0.10.0` is higher than `0.9.0`.

* **Yes** → the lock is stale. Go to [Resolution](#resolution) and start at step 2.
* **No** → continue.

**Is the version in the message higher than that `content-version`?**

* **Yes** → the lock is ahead of the producer. Where `examples/engineering`'s records changed on this branch and its
  `content-version` did not move, move it as [std-VERS] says and go to [Resolution](#resolution) at step 1. Otherwise
  go to [Resolution](#resolution) at step 2, and set the lock back.
* **No** → the package is there and the restore refused it for another reason. [Escalate](#escalation).

## Resolution

1. Rebuild the producer's package, from `examples/engineering`:

   ```sh
   dotnet run --project ../../tooling/kac -- export
   dotnet run --project ../../tooling/kac -- pack
   ```

   `pack` prints `pack: sealed <n> file(s) as example-engineering <version>, cited as 'eng:'.` Every consumer has to
   ask for that version. `pack` empties `.dist/package` before it writes, so the earlier package is gone.

2. Open `.corpus.yaml` in `examples/payments`.
3. Set `resolved:` to the producer's `content-version`.
4. Where the minor moved, set `version:` to a range that admits it. Below 1.0.0 a caret pins the minor, so `^0.10.0`
   does not admit `0.11.0`.
5. Repeat steps 2 to 4 in `examples/dog-fooding`.
6. Delete `.imports/` in both consumers, from the root of the repository. A restore keeps a folder that already has the
   version it resolved to, and the next step writes the folder again:

   ```sh
   rm -rf examples/payments/.imports examples/dog-fooding/.imports
   ```
7. Run the restore again, from `examples/payments` and then from `examples/dog-fooding`:

   ```sh
   dotnet run --project ../../tooling/kac -- restore
   dotnet run --project ../../tooling/kac -- validate
   ```

   `restore` prints `restore: example-engineering <version> as 'eng:'.` and writes `resolved:` into `.corpus.yaml`.
   `validate` reports `0 error(s)`. To undo what steps 3 to 7 wrote to `.corpus.yaml`, run this from the root of the
   repository. It also discards any other uncommitted edit to either file:

   ```sh
   git checkout -- examples/payments/.corpus.yaml examples/dog-fooding/.corpus.yaml
   ```

   The undo leaves the package step 1 wrote. A restore against the old lock fails until you pack the producer at the
   old version.

Confirmed when both consumers restore and validate with no errors. If a step does not do what it says,
[escalate](#escalation).

## Escalation

| When                                                          | Who                           | How                          |
|---------------------------------------------------------------|-------------------------------|------------------------------|
| The folder has the version and the restore still refuses it   | Paul Law, as the tool's owner | An issue on this repository  |
| The package was fetched at one version and says it is another | Paul Law, as the tool's owner | An issue on this repository  |
| The message says `there is no folder` and `source:` is right  | Paul Law, as the tool's owner | An issue on this repository  |
| CI fails again after the lock and the package agree here      | Paul Law, as the tool's owner | An issue, with the job's log |

## Communication

| Who                                                 | What they need to know                          | How often                 |
|-----------------------------------------------------|-------------------------------------------------|---------------------------|
| Anybody on a branch touching `examples/engineering` | The lock is stale, and which version to move to | Once, as soon as you know |

## Afterwards

* Raise no postmortem. The gate catches a stale lock before a reader does.
* Where the producer's `content-version` moved on this branch, check that both consumers moved their `resolved:` lock
  and their own `content-version` with it before the pull request opens.

## Related

* [ctl-0007] is the check that runs `validate` in every corpus.
* [std-VERS.CONSUME] is the rule a stale lock breaches.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-VERS]: ../standards/versioning.md
[std-VERS.CONSUME]: ../standards/versioning.md#consume-a-producers-move-obliges-every-consumer-in-the-same-pull-request
