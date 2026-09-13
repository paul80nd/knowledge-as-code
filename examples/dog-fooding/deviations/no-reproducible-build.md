---
id: dev-no-reproducible-build
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-AUTV.BITWISE
  - eng:pol-AUTV.REPRO
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ build, reproducibility, verification ]
---

# Two builds of `kac` are never compared

`Deviation: dev-no-reproducible-build` `ACTIVE`

Any clone builds the tool, and nothing states that as a rule or checks that two builds of one commit agree byte for
byte.

## What we are doing instead

`dotnet run --project tooling/kac` builds from a checkout with the .NET SDK the repository pins, on a runner GitHub
gives fresh for every job. That is a reproducible environment by construction, and no standard here says so.

The package published to nuget.org is built once, by the publish workflow, from the commit on `main`. Nobody has ever
built the same commit twice and compared the two `.nupkg` files.

## Why we need it

A byte-identical build needs `ContinuousIntegrationBuild`, a deterministic source path map and a source-link setup,
and then a job that builds twice and diffs. That is real work against a threat this repository does not face: the
package is built by a workflow whose run is public, from a commit anybody can read.

## What compensates

* The publish workflow is the only thing that builds what ships, and its run log records the commit.
* [ctl-0007] packs the tool and installs it on every pull request. That copy runs `kac new` and validates the corpus
  it created, so what ships has been run.
* [tol-dotnet-sdk] pins the SDK, and [std-CONFIG] requires that pin to be an exact version.
* nuget.org never lets a published version be replaced, so a rebuild cannot overwrite what people already have.

## How it closes

The publish workflow builds the package twice and fails where the two differ. A standard states the rule, and this
record closes when both exist.

Where that is still not worth doing at the review date, the alternative is provenance. An attestation names the
workflow, the commit and the runner. [dev-package-unscanned] asks for the same thing.

## Scope

The `kac` package published to nuget.org, and the local builds a contributor runs.

## Related

* [dev-package-unscanned] covers the provenance half of the same question.

[ctl-0007]: ../controls/0007-corpus-validation.md
[dev-package-unscanned]: package-unscanned.md
[std-CONFIG]: ../standards/configuration.md
[tol-dotnet-sdk]: ../tools/build/dotnet-sdk.md
