---
id: dev-package-unscanned
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-TRUS.MALWARE
risk: medium
accepted-on: "2026-10-21"
review-by: "2027-03-07"
closed-on:
applies-to:
  - svc-corpus-feed
  - svc-kac
  - svc-marketplace
owner: human:paul.law
tags: [ malware, package, supply-chain ]
---

# Nothing scans the packages this repository publishes

`Deviation: dev-package-unscanned` `ACTIVE`

The `.nupkg` published to nuget.org, the plugin served from the marketplace branch and the four corpus packages on
GitHub Packages pass through no malware scan.

## What we are doing instead

Each package is built by a publish workflow from the commit on `main`, on a runner GitHub gives fresh for that run.
`publish-corpus.yml` builds the corpus packages, and its `verify` job validates and packs each corpus with no write
permission. Dependabot watches the dependencies, and [std-CONFIG] requires each pin to name one version. Nothing scans
what the build produced before it is pushed.

## Why we need it

A scan needs a scanner chosen and a failure path decided, and neither has been done. Each package is a documentation
tool or a corpus of records, built from a public repository by a workflow anybody can read.

## What the risk is

Something malicious gets into a package through a dependency or a build step, and nothing looks for it before it ships.
A consumer installing `kac`, or restoring a corpus package, takes what it contains on trust.

## What compensates

* Each package is built by one workflow, whose run log is public and records the commit it built.
* [ctl-0007] packs the tool and installs it on every pull request. That copy runs `kac new` and validates the corpus
  it created, so what ships has been run.
* Trusted publishing means no key exists that would let anybody else push `kac` under its name.
* `publish-corpus.yml` pushes a corpus package with the job's own `github.token`, which expires when the job ends.
* A registry keeps a published version forever, so what shipped cannot be replaced. nuget.org refuses a second push to
  a version, and `publish-corpus.yml` pushes only a version GitHub Packages does not have.

## How it closes

A scan runs over every package before it is published, and fails the job on a finding. This record closes when that
scan is in the publish workflows.

## Scope

The `kac` package on nuget.org, the plugin served from the marketplace branch, and the four corpus packages
[svc-corpus-feed] publishes to GitHub Packages.

## Related

* [std-CONFIG] governs the pins the packages are built from.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-CONFIG]: ../standards/configuration.md
[svc-corpus-feed]: ../services/corpus-feed.md
