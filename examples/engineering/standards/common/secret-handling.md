---
id: std-0003
tier: normative
status: draft
axis: common
implements:
  - pol-ENVS
  - pol-SCRT
applies-to:
  - all
review-by: "2027-08-26"
owner: paul.law
tags: [ credentials, key-management, secrets ]
---

# A secret is read from the store at run time

`Standard: std-0003` `DRAFT`

## Summary

A service reads every secret from the managed store when it starts or when it needs one, using an identity granted to
that service in that environment. Nothing else holds a secret.

## Rules

- A service **MUST** read every secret from the managed store, through an identity granted to the workload
  ([pol-SCRT.STORE]).
- A repository **MUST NOT** contain a secret, in source, in a configuration file, in a pipeline definition or in a test
  fixture ([pol-SCRT.EMBED]).
- A build **MUST NOT** bake a secret into an artefact or an image ([pol-SCRT.EMBED]).
- A secret **MUST** be replaceable in the store without a code change or a rebuild ([pol-SCRT.ROTATE]).
- Every secret **MUST** rotate on a period recorded against it in the store ([pol-SCRT.ROTATE]).
- A pipeline **MUST** run a secret scanner over the repository and its history, and fail on a finding
  ([pol-SCRT.LEAKED]).
- A service **MUST NOT** write a secret to a log, to a console, to an error message or to a support ticket
  ([pol-SCRT.LOGS]).
- An environment below production **MUST** hold its own secrets, distinct from production's ([pol-ENVS.REUSE]).

## Examples

```
Good
  DATABASE_PASSWORD=@AzureKeyVault(covers-prod/db-password)
  # resolved at start-up by the workload identity assigned to the service

Avoid
  DATABASE_PASSWORD=Tr0ub4dor&3              committed to the repository
  ENV DATABASE_PASSWORD=$(DB_PASSWORD)       baked into the image at build time
```

The first avoided form sits in the repository, so rotating it costs a commit. The second reads from the store and then
writes the value into the image, so rotating it costs a rebuild and a redeploy.

## Conformance checklist

- [ ] The service starts with no secret in its repository and no secret in its image.
- [ ] Every secret it reads resolves from the store through a workload identity.
- [ ] Each secret in the store carries a rotation period, and the last rotation is within it.
- [ ] The pipeline runs a secret scanner over the repository and its history.
- [ ] The production secrets are unreachable from any environment below production.
- [ ] A search of the last 30 days of logs for each secret returns nothing.

## Rationale and provenance

A secret in a repository is a secret in every clone, every fork and every backup of that repository, and rotating it
does not reach any of them. Reading from the store at run time keeps rotation to one place.

- [pol-ENVS] keeps production credentials unreachable from a lower environment.
- [pol-SCRT] commits us to holding secrets in a controlled store and never embedding one.

[pol-ENVS]: ../../policies/envs-environment-separation.md
[pol-ENVS.REUSE]: ../../policies/envs-environment-separation.md#clauses
[pol-SCRT]: ../../policies/scrt-secrets-are-never-embedded.md
[pol-SCRT.EMBED]: ../../policies/scrt-secrets-are-never-embedded.md#clauses
[pol-SCRT.LEAKED]: ../../policies/scrt-secrets-are-never-embedded.md#clauses
[pol-SCRT.LOGS]: ../../policies/scrt-secrets-are-never-embedded.md#clauses
[pol-SCRT.ROTATE]: ../../policies/scrt-secrets-are-never-embedded.md#clauses
[pol-SCRT.STORE]: ../../policies/scrt-secrets-are-never-embedded.md#clauses
