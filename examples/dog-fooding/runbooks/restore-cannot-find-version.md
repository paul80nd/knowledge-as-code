---
id: rbk-restore-cannot-find-version
type: runbook
tier: procedural
status: active
severity: sev3
last-rehearsed: "never"
requires-access:
  - A .NET 10 SDK and a checkout of this repository
  - Write access to the branch the fix lands on
owner: human:paul.law
tags: [ imports, restore, versioning ]
---

# kac restore cannot find the version the lock names

`Runbook: rbk-restore-cannot-find-version` `ACTIVE`

## Symptoms

* `kac restore` prints `restore: ../engineering/.dist/package holds no package for example-engineering <version>.` and
  exits 1.
* `kac validate` fails in `examples/payments` or `examples/dog-fooding`, and says to run `kac restore` first.
* The `corpora` job in `kac.yml` fails at **Restore what this corpus consumes**, for those two corpora only. `library`
  and `engineering` consume nothing, so they pass.

Nothing failed in the producer. `examples/engineering` validates, exports and packs.

## Immediate actions

1. Read the version out of the message. It is the one the consumer's lock asked for.
2. Leave `.corpus.yaml` alone until the diagnosis below says which field to move.

## Diagnosis

**Does `examples/engineering/.dist/package` exist?**

* **No** → the producer has never been packed in this checkout. Go to [Resolution](#resolution) and start at step 1.
* **Yes** → continue.

**Does that folder contain a `.nupkg` for the version in the message?**

```sh
ls examples/engineering/.dist/package
```

* **No** → the lock is stale. `kac pack` rebuilds the folder whole, so it contains the version packed last and nothing
  else. Go to [Resolution](#resolution) and start at step 1.
* **Yes** → the package is there and the restore refused it for another reason. [Escalate](#escalation).

## Resolution

1. Rebuild the producer's package:

   ```sh
   cd examples/engineering
   dotnet run --project ../../tooling/kac -- export
   dotnet run --project ../../tooling/kac -- pack
   ```

   The last line reports the version it packed. Every consumer has to ask for that version.

2. Open `.corpus.yaml` in `examples/payments`.
3. Set `resolved:` to the version step 1 reported.
4. Where the minor moved, set `version:` to a range that admits it. Below 1.0.0 a caret pins the minor, so `^0.10.0`
   does not admit `0.11.0`.
5. Repeat steps 2 to 4 in `examples/dog-fooding`.
6. Delete `.imports/` in both consumers. A restore keeps a folder that already has the version it resolved to.
7. Run the restore again in each consumer:

   ```sh
   cd examples/payments
   dotnet run --project ../../tooling/kac -- restore
   dotnet run --project ../../tooling/kac -- validate
   ```

Confirmed when both consumers restore and validate with no errors.

## Escalation

| When                                                          | Who                           | How                         |
|---------------------------------------------------------------|-------------------------------|-----------------------------|
| The folder has the version and the restore still refuses it   | Paul Law, as the tool's owner | An issue on this repository |
| The package was fetched at one version and says it is another | Paul Law, as the tool's owner | An issue on this repository |

## Afterwards

* Raise no postmortem. The gate catches a stale lock before a reader does.
* Where the producer's `content-version` moved on this branch, check that both consumers moved with it before the pull
  request opens.

## Related

* [ctl-0007] is the check that runs `validate` in every corpus.
* [std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request] is the rule a stale lock breaches.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request]: ../standards/versioning.md#a-producers-move-obliges-every-consumer-in-the-same-pull-request
