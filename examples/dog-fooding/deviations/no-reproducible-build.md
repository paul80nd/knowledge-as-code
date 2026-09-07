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
owner: paul.law
tags: [ build, reproducibility, verification ]
---

# Two builds of `kac` are never compared

`Deviation: dev-no-reproducible-build` `ACTIVE`

Any clone builds the tool, and nothing states that as a rule or checks that two builds of one commit agree byte for
byte.

## What we are doing instead

`dotnet run --project tooling/kac` builds from a checkout with the .NET SDK the repository pins, on a runner GitHub
gives fresh for every job. That is a reproducible environment by construction, and no standard here says so.

The package that reaches nuget.org is built once, by the publish workflow, from the commit on `main`. Nobody has ever
built the same commit twice and compared the two `.nupkg` files.

## Why we need it

A byte-identical build needs `ContinuousIntegrationBuild`, a deterministic source path map and a source-link setup, and
then a job that builds twice and diffs. That is real work against a threat this repository does not carry: the package
is built by a workflow whose run is public, from a commit anybody can read.

## What compensates

* The publish workflow is the only thing that builds what ships, and its run log names the commit.
* ctl-0007 stands a corpus up from the packed tool on every pull request, so what ships has been run.
* [tol-dotnet-sdk] pins the SDK, and [std-CONFIG] holds that pin to an exact version.
* nuget.org never lets a published version be replaced, so a rebuild cannot quietly overwrite what people already have.

## How it closes

The publish workflow builds the package twice and fails where the two differ. A standard states the rule, and this
record closes when both exist.

Where that is still not worth doing at the review date, the alternative is provenance: an attestation naming the
workflow, the commit and the runner, which [dev-package-unscanned] also asks for.

## Scope

The `kac` package published to nuget.org, and the local builds a contributor runs.

## Related

* [dev-package-unscanned] carries the provenance half of the same question.

[dev-package-unscanned]: package-unscanned.md
[std-CONFIG]: ../standards/configuration.md
[tol-dotnet-sdk]: ../tools/build/dotnet-sdk.md
