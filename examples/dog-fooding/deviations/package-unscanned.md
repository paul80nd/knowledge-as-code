---
id: dev-package-unscanned
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-TRUS.MALWARE
risk: medium
accepted-on: "2026-09-07"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-kac
  - svc-marketplace
owner: human:paul.law
tags: [ malware, package, supply-chain ]
---

# Nothing scans the package `kac` ships

`Deviation: dev-package-unscanned` `ACTIVE`

The `.nupkg` published to nuget.org and the plugin served from the marketplace branch pass through no malware scan.

## What we are doing instead

The package is built by the publish workflow from the commit on `main`, on a runner GitHub gives fresh for that run.
Dependabot watches the dependencies, and [std-CONFIG] requires each pin to name one version. Nothing scans what the
build produced before it is pushed.

## Why we need it

A scan needs a scanner chosen and a failure path decided, and neither has been done. The package is a documentation
tool built from a public repository, by one workflow anybody can read.

## What the risk is

Something malicious gets into the package through a dependency or a build step, and nothing looks for it before it
ships. A consumer installing `kac` takes what it contains on trust.

## What compensates

* The package is built by one workflow, whose run log is public and records the commit it built.
* [ctl-0007] packs the tool and installs it on every pull request. That copy runs `kac new` and validates the corpus
  it created, so what ships has been run.
* Trusted publishing means no key exists that would let anybody else push under this name.
* nuget.org refuses a second push to a version it already has, so what shipped cannot be replaced.

## How it closes

A scan runs over the package and the plugin before they are published, and fails the job on a finding. This record
closes when that scan is in the publish workflows.

## Scope

The `kac` package on nuget.org, and the plugin served from the marketplace branch.

## Related

* [std-CONFIG] governs the pins the package is built from.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-CONFIG]: ../standards/configuration.md
