---
id: dev-package-unscanned
tier: normative
status: active
departs-from:
  - eng:pol-TRUS.ATTEST
  - eng:pol-TRUS.MALWARE
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-kac
owner: paul.law
tags: [ package, provenance, supply-chain ]
---

# Nothing scans or attests the package `kac` ships

`Deviation: dev-package-unscanned` `ACTIVE`

The `.nupkg` published to nuget.org passes through no malware scan, and carries nothing a consumer could check its
origin against.

## What we are doing instead

The package is built by the publish workflow from the commit on `main`, on a runner GitHub gives fresh for that run.
Dependabot watches the dependencies, and [std-CONFIG] holds every pin to an exact version.

Whoever installs `kac` gets a package whose provenance is the nuget.org listing and nothing else. There is no
attestation naming the workflow, the commit or the runner that built it.

## Why we need it

Both are available and neither is free. GitHub's artifact attestation needs the publish workflow reworked around it,
and a scan needs a scanner chosen and a failure path decided. Neither has been done, and the package is a documentation
tool built from a public repository.

## What compensates

* The package is built by one workflow, whose run log is public and names the commit it built.
* ctl-0008 restores the published package and compares what it holds against the repository.
* Trusted publishing means no key exists that would let anybody else push under this name.
* nuget.org refuses a second push to a version it already holds, so what shipped cannot be replaced.

## How it closes

The publish workflow generates a build provenance attestation, and the release notes say how to verify it. A scan runs
over the package before the push and fails the job on a finding. This record closes when both are in the workflow.

## Scope

The `kac` package on nuget.org, and the plugin served from the marketplace branch.

## Related

* `eng:pol-TRUS.ATTEST` and `eng:pol-TRUS.MALWARE` are the clauses this departs from.
* [dev-no-reproducible-build] carries the other half of the provenance question.
* [std-CONFIG] holds the pins the package is built from.

[dev-no-reproducible-build]: no-reproducible-build.md
[std-CONFIG]: ../standards/configuration.md
