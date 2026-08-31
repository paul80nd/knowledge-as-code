---
id: std-CONT
tier: normative
status: draft
implements: [ pol-ACCS.LEAST, pol-ENVS.BASELIN, pol-MEXP.PUBLIC, pol-TRUS.MUTATE, pol-TRUS.SOURCE ]
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ base-images, containers, runtime ]
---

# A container image is pinned by digest and runs as a non-root user

`Standard: std-CONT` `DRAFT`

## Summary

An image is built from a base pinned by digest, tagged with the version it will keep forever, and started as a
non-root user with no port open beyond the one the service answers on.

## Rules

### The base image is chosen and pinned

- A Dockerfile **MUST** name its base image from the organisation's registry.
- A Dockerfile **MUST** pin the base image by digest rather than by a moving tag.
- A repository **MUST** rebuild against a refreshed base at least monthly, so a patched base reaches the running
  service.

_**Covers:** [pol-ENVS].BASELIN, [pol-TRUS].SOURCE_

### A published tag never moves

- A pipeline **MUST** tag an image with the build's own version.
- A registry **MUST** refuse a second push to a tag that already exists.
- A deployment **MUST** name an image by digest, so the running container is the one the pipeline built.

_**Covers:** [pol-TRUS].MUTATE_

### The container holds only what the service needs

- An image **MUST** declare a non-root `USER`.
- A container **MUST** run as that user.
- A container **MUST** run with a read-only root filesystem, writing only to a declared volume.
- An image **MUST** be built from a runtime base rather than an SDK base, so no compiler ships to production.
- An image **MUST NOT** expose a management, debug or metrics port to anything outside the cluster.

_**Covers:** [pol-ACCS].LEAST, [pol-MEXP].PUBLIC_

## Examples

```
Good
  FROM mcr.microsoft.com/dotnet/aspnet@sha256:0f9c...   via registry.example.com
  USER app
  EXPOSE 8080

Avoid
  FROM mcr.microsoft.com/dotnet/sdk:latest
  EXPOSE 8080 5000 22
```

The avoided form ships a compiler and an SSH port to production, and `latest` means a rebuild next week produces a
different image from the same Dockerfile.

## Conformance checklist

- [ ] Every `FROM` in the repository carries a digest.
- [ ] The base image digest was refreshed within the last month.
- [ ] `docker inspect` on the running image reports a non-root user.
- [ ] The deployment manifest names an image digest rather than a tag.
- [ ] The registry refuses a second push to a tag that already exists.
- [ ] The container exposes one port, and it is the one the service answers on.

## Rationale and provenance

A tag is a name somebody can repoint, and a digest is the bytes. Deploying by digest is what makes the image we tested
and the image running the same object.

## Sources and further reading

- **Normative.** [OCI Image Format Specification] defines the digest these rules pin to.
- **Informative.** [NIST SP 800-190] covers the container risks this standard answers a few of.

[NIST SP 800-190]: https://csrc.nist.gov/pubs/sp/800/190/final
[OCI Image Format Specification]: https://github.com/opencontainers/image-spec/blob/main/spec.md
[pol-ACCS]: ../../policies/security/accs-access-by-identity.md#clauses
[pol-ENVS]: ../../policies/security/envs-environment-separation.md#clauses
[pol-MEXP]: ../../policies/security/mexp-minimised-exposure.md#clauses
[pol-TRUS]: ../../policies/security/trus-trusted-components.md#clauses
