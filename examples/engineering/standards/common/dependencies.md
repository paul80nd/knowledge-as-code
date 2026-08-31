---
id: std-DEPS
tier: normative
status: draft
implements: [ pol-TRUS.INVENT, pol-TRUS.LICENCE, pol-TRUS.REPO, pol-TRUS.SCREEN, pol-TRUS.SOURCE, pol-TRUS.UNTRUST,
  pol-VURM.RANK, pol-VURM.SCAN, pol-VURM.SHIP, pol-VURM.TIMEBOX ]
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ dependencies, licences, supply-chain ]
---

# A dependency is pinned, screened and named in the inventory

`Standard: std-DEPS` `DRAFT`

## Summary

Every third-party package a build pulls in is pinned to a version, comes from a feed we control, and is screened for
vulnerabilities and licence terms before anyone adopts it. A critical finding stops the release.

## Rules

### Where a package comes from

- A build **MUST** resolve packages from the organisation's feed, which proxies the public registries.
- A repository **MUST** commit a lockfile pinning every direct and transitive dependency to a version.
- A build **MUST** install from the lockfile, and fail where the lockfile and the manifest disagree.
- A build **MUST** publish its artefacts to the managed repository, versioned and retained.
- A team **MUST NOT** add a package from a personal feed, a git URL or an archive downloaded by hand.

_**Covers:** [pol-TRUS].INVENT, [pol-TRUS].REPO, [pol-TRUS].SOURCE, [pol-TRUS].UNTRUST_

### What happens before adoption

- Somebody **MUST** screen a new package for known vulnerabilities before the pull request that adds it merges.
- Somebody **MUST** check a new package's licence against the allowed list before that pull request merges.
- A pull request adding a package **MUST** say what the package is for, so a reviewer can weigh it against writing the
  code.

_**Covers:** [pol-TRUS].LICENCE, [pol-TRUS].SCREEN_

### What happens afterwards

- A pipeline **MUST** scan the dependency tree on every build and on a weekly schedule.
- A team **MUST** rank a finding by its severity and by whether the affected path runs.
- A team **MUST** close a critical finding within 7 days and a high finding within 30.
- A team **MUST NOT** release with an open critical finding, absent a recorded deviation naming who accepts the risk.

_**Covers:** [pol-VURM].RANK, [pol-VURM].SCAN, [pol-VURM].SHIP, [pol-VURM].TIMEBOX_

## Examples

```
Good
  package-lock.json committed, and CI runs `npm ci`

Avoid
  CI runs `npm install`
```

The avoided command resolves the manifest afresh, so two builds of one commit can install different code.

```
Good
  # Adds Polly for the retry budget on the PSP call. Written by hand it is
  # about 200 lines, and the backoff maths is where we would get it wrong.

Avoid
  # adds left-pad
```

A reviewer can weigh the first against writing the code. The second gives them nothing to weigh.

## Conformance checklist

- [ ] Every package source configured in the repository points at the organisation's feed.
- [ ] The lockfile is committed, and CI installs from it rather than resolving afresh.
- [ ] The dependency scan runs on every build, and its results are visible without opening the tool.
- [ ] No critical finding is open past 7 days without a recorded deviation.
- [ ] Every licence in the tree appears on the allowed list.
- [ ] The build artefacts for the last release are still in the managed repository.

## Rationale and provenance

Most of the code we ship was written by somebody else. A pinned tree tells us exactly whose code that is, which is what
makes an advisory answerable in minutes rather than in a survey of every repository.

## Sources and further reading

- **Informative.** [OWASP Dependency-Check] and [OSV] cover the advisory data the scan is run against.

[OSV]: https://osv.dev/
[OWASP Dependency-Check]: https://owasp.org/www-project-dependency-check/
[pol-TRUS]: ../../policies/security/trus-trusted-components.md#clauses
[pol-VURM]: ../../policies/security/vurm-vulnerability-remediation.md#clauses
