---
id: fix-restore-cannot-find-version
type: fix
tier: normative
status: draft
symptom-keywords: [ ".imports", consumes, content-version, "has no version matching", "holds no package for",
  import-restored, kac restore, lock, "no folder at", resolved, stale lock ]
review-by: "2027-04-29"
owner: human:paul.law
sources:
  - { resource: https://github.com/paul80nd/knowledge-as-code/issues/1046 }
tags: [ imports, restore, versioning ]
---

# kac restore cannot find the version the lock names

`Fix: fix-restore-cannot-find-version` `DRAFT`

## Symptom

`kac restore` exits 1 and prints one of these:

* `restore: ../engineering/.dist/package holds no package for example-engineering <version>.`
* `restore: 'example-engineering' has no version matching '<range>' at ../engineering/.dist/package.`, followed by
  `It holds <version>.`, or by `That folder holds no package for it.` where the folder is empty.
* ``restore: there is no folder at ../engineering/.dist/package. `source:` names either a registry's service index
  or a folder holding the packages a producer built.``

`kac validate` fails in `examples/payments` or `examples/dog-fooding` with `[import-restored]`, and says
``.imports/eng/ holds no export. Run `kac restore`.`` In CI, the `corpora` and `round-trip` jobs in `kac.yml` fail at
**Restore what this corpus consumes** for those two corpora only. `kac restore` can pass on your machine while CI
fails. Nothing failed in the producer: `examples/engineering` validates, exports and packs.

## Environment

A checkout of this repository with a .NET 10 SDK, on a branch that moved `examples/engineering`. The consumers are
`examples/payments` and `examples/dog-fooding`, whose `.corpus.yaml` names the producer under `consumes:` with
`source: ../engineering/.dist/package`.

## Cause

The lock in a consumer's `.corpus.yaml` disagrees with the package the producer packed. Its `resolved:` names a version
lower or higher than the producer's `content-version`, or its `version:` range does not admit it. A restore skips a
consumer whose `.imports/` already has the locked version, so a stale lock passes locally and fails in CI, which starts
with no `.imports/`.

## Resolution

1. Repack the producer, from `examples/engineering`.

   ```sh
   dotnet run --project ../../tooling/kac -- export
   dotnet run --project ../../tooling/kac -- pack
   ```

   `pack` prints `pack: sealed <n> file(s) as example-engineering <version>, cited as 'eng:'.` Every consumer has to
   ask for that version. `pack` empties `.dist/package` before it writes, so the earlier package is gone.

2. Where `examples/engineering`'s records changed on this branch and its `content-version` did not move, move it as
   [std-VERS] says, then repeat step 1.
3. In `.corpus.yaml` in `examples/payments`, set `resolved:` to the producer's `content-version`. This sets a stale
   lock forward, and sets back a lock that is ahead of the producer.
4. Where the minor moved, set `version:` to a range that admits it. Below 1.0.0 a caret pins the minor, so `^0.10.0`
   does not admit `0.11.0`.
5. Where the message said `there is no folder` and `source:` names another folder, set `source:` to
   `../engineering/.dist/package`.
6. Repeat steps 3 to 5 in `examples/dog-fooding`.
7. Delete `.imports/` in both consumers, from the root of the repository. A restore keeps a folder that already has
   the version it resolved to.

   ```sh
   rm -rf examples/payments/.imports examples/dog-fooding/.imports
   ```

8. Restore and validate, from `examples/payments` and then from `examples/dog-fooding`.

   ```sh
   dotnet run --project ../../tooling/kac -- restore
   dotnet run --project ../../tooling/kac -- validate
   ```

It worked when `restore` prints `restore: example-engineering <version> as 'eng:'.` and `validate` reports
`0 error(s)` in both consumers. To undo what steps 3 to 8 wrote, run
`git checkout -- examples/payments/.corpus.yaml examples/dog-fooding/.corpus.yaml` from the root of the repository. It
also discards any other uncommitted edit to either file, and leaves the package step 1 wrote. Before the pull request
opens, check that both consumers moved their own `content-version` with the lock.

Raise an issue on this repository for the tool's owner where any of these happens:

* The folder has the version and the restore still refuses it.
* The package was fetched at one version and says it is another.
* The message says `there is no folder` and `source:` already names `../engineering/.dist/package`.
* CI fails again after the lock and the package agree here. Attach the job's log.

## Why it happens

A consumer's lock and the producer's `content-version` move by hand, in the same pull request, as [std-VERS.CONSUME]
states. `.imports/` is untracked, so a restore over a folder you kept proves nothing about the lock CI reads.

## Related

* [ctl-corpus-validation] is the check that runs `validate` in every corpus.
* [std-VERS.CONSUME] is the rule a stale lock breaches.

[ctl-corpus-validation]: ../controls/corpus-validation.md
[std-VERS]: ../standards/versioning.md
[std-VERS.CONSUME]: ../standards/versioning.md#consume-a-producers-move-obliges-every-consumer-in-the-same-pull-request
