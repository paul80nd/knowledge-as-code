---
id: std-CONT
type: standard
tier: normative
status: draft
implements: [ pol-ACCS.LEAST, pol-ENVS.BASELIN, pol-MEXP.PUBLIC, pol-TRUS.MUTATE, pol-TRUS.SOURCE ]
applies-to:
  - all
review-by: "2027-08-31"
owner: human:paul.law
tags: [ base-images, containers, runtime ]
---

# A container image is pinned by digest and runs as a non-root user

`Standard: std-CONT` `DRAFT`

## Summary

An image is built from a base pinned by digest. The pipeline tags it with the build's own version. That tag never
moves. The container runs as a non-root user and opens one port, the one the service listens on.

## Rules

### The base image is chosen and pinned

- A Dockerfile **MUST** take its base image from the organisation's registry.
- A Dockerfile **MUST** pin the base image by digest rather than by a moving tag.
- A repository **MUST** rebuild against a refreshed base at least monthly, so a patched base is deployed to the running
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

The avoided form ships a compiler and an SSH port to production. `latest` pins nothing, so a rebuild next week produces
a different image from the same Dockerfile.

## Conformance checklist

- [ ] Every `FROM` in the repository has a digest.
- [ ] The base image digest was refreshed within the last month.
- [ ] `docker inspect` on the running image reports a non-root user.
- [ ] The deployment manifest names an image digest rather than a tag.
- [ ] The registry refuses a second push to a tag that already exists.
- [ ] The container exposes one port, and it is the one the service listens on.

## Rationale and provenance

Deploying by digest means the image that runs is the image you tested. A tag is a name somebody can repoint. A digest is
a hash of the bytes.

## Sources and further reading

- **Normative.** [OCI Image Format Specification] defines the digest these rules pin to.
- **Informative.** [NIST SP 800-190] lists the container risks. This standard covers some of them.

[NIST SP 800-190]: https://csrc.nist.gov/pubs/sp/800/190/final
[OCI Image Format Specification]: https://github.com/opencontainers/image-spec/blob/main/spec.md
[pol-ACCS]: ../../policies/security/accs-access-by-identity.md#clauses
[pol-ENVS]: ../../policies/security/envs-environment-separation.md#clauses
[pol-MEXP]: ../../policies/security/mexp-minimised-exposure.md#clauses
[pol-TRUS]: ../../policies/security/trus-trusted-components.md#clauses
