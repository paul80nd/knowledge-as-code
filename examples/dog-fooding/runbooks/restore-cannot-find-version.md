---
id: rbk-restore-cannot-find-version
tier: procedural
status: active
applies-to:
severity: sev3
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
  - A .NET 10 SDK and a checkout of this repository
  - Write access to the branch the fix lands on
owner: paul.law
tags: [ imports, restore, versioning ]
---

# kac restore cannot find the version the lock names

`Runbook: rbk-restore-cannot-find-version` `ACTIVE`

## Symptoms

* `kac restore` prints `restore: ../engineering/.dist/package holds no package for example-engineering <version>.` and
  exits 1.
* `kac validate` fails in `examples/payments` or `examples/dog-fooding`, naming `restore` as the command to run first.
* The `corpora` job in `kac.yml` fails at **Restore what this corpus consumes**, for those two corpora only. `library`
  and `engineering` consume nothing, so they pass.

What you are not seeing: any failure in the producer. `examples/engineering` validates, exports and packs.

## Immediate actions

1. Read the version out of the message. It is the one the consumer's lock asked for.
2. Leave `.corpus.yaml` alone until the diagnosis below says which field to move.

## Diagnosis

**Does `examples/engineering/.dist/package` exist?**

* **No** → the producer has never been packed in this checkout. Go to [Resolution](#resolution) and start at step 1.
* **Yes** → continue.

**Does that folder hold a `.nupkg` for the version in the message?**

```sh
ls examples/engineering/.dist/package
```

* **No** → the lock is stale. `kac pack` rebuilds the folder whole, so it holds the version packed last and nothing
  else. Go to [Resolution](#resolution) and start at step 1.
* **Yes** → the package is there and the restore refused it for another reason. [Escalate](#escalation).

## Resolution

1. Rebuild the producer's package:

   ```sh
   cd examples/engineering
   dotnet run --project ../../tooling/kac -- export
   dotnet run --project ../../tooling/kac -- pack
   ```

   The last line names the version it sealed. That is the version every consumer has to ask for.

2. Open `.corpus.yaml` in `examples/payments` and set `resolved:` to that version.
3. Where the minor moved, set `version:` to a range admitting it. Below 1.0.0 a caret pins the minor, so `^0.10.0`
   does not admit `0.11.0`.
4. Repeat steps 2 and 3 in `examples/dog-fooding`.
5. Delete `.imports/` in both consumers. A restore keeps a folder already holding the version it resolved to, so a
   stale folder hides the fix.
6. Run the restore again in each consumer:

   ```sh
   cd examples/payments
   dotnet run --project ../../tooling/kac -- restore
   dotnet run --project ../../tooling/kac -- validate
   ```

Confirmed when both consumers restore and validate with no errors.

## Escalation

| When                                                          | Who                           | How                         |
|---------------------------------------------------------------|-------------------------------|-----------------------------|
| The folder holds the version and the restore still refuses it | Paul Law, as the tool's owner | An issue on this repository |
| The refusal names the package's own contents                  | Paul Law, as the tool's owner | An issue on this repository |

## Afterwards

* Raise no postmortem. A stale lock is caught by the gate rather than by a reader.
* Where the producer's `content-version` moved on this branch, check that both consumers moved with it before the pull
  request opens.

## Related

* [ctl-0007] is the check that runs `validate` in every corpus.
* [std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request] is the rule a stale lock breaches.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request]: ../standards/versioning.md#a-producers-move-obliges-every-consumer-in-the-same-pull-request
