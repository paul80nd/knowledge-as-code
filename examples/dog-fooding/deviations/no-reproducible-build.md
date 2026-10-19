---
id: dev-no-reproducible-build
type: deviation
tier: normative
status: active
departs-from:
  - eng:pol-AUTV.REPRO
  - std-CONFIG.a-pin-names-an-exact-version
risk: low
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ build, reproducibility, sdk ]
---

# A build of `kac` takes whichever .NET 10 SDK the runner installs

`Deviation: dev-no-reproducible-build` `ACTIVE`

Every workflow that builds `kac` installs the .NET SDK as `10.0.x`, so two builds of one commit can run on different
SDKs.

## What we are doing instead

Each job that builds it installs the SDK with `actions/setup-dotnet` and `dotnet-version: 10.0.x`, which takes the
newest 10.0 SDK on the day. [tol-dotnet-sdk] approves the same range. No `global.json` fixes the version, so version
control alone does not say which SDK built a commit.

The package published to nuget.org is built once, by the publish workflow, from the commit on `main`. Its run log
shows the SDK that job installed.

## Why we need it

Fixing the version needs a `global.json` and a way to move it when a patch ships, and neither has been set up. The
range takes each SDK patch without a pull request.

## What the risk is

Two builds of one commit run on different SDK patches, and a difference between them shows only in the package that
shipped. Nothing here would explain it without reading both run logs. An SDK patch seldom changes what a build
produces, so the cost is a confusing difference and not a broken release.

## What compensates

* The publish workflow is the only thing that builds what ships, and its run log records the commit and the SDK.
* [ctl-0007] packs the tool and installs it on every pull request. That copy runs `kac new` and validates the corpus
  it created, so a build that cannot run is caught before it ships.
* nuget.org never lets a published version be replaced, so a rebuild cannot overwrite what people already have.

## How it closes

A `global.json` names the exact SDK, every workflow installs the version it names, and something moves it when a patch
ships. This record closes when that file is in the repository.
[#971](https://github.com/paul80nd/knowledge-as-code/issues/971) tracks the work.

## Scope

Every build of `kac`: the publish workflow, CI, and a contributor's own machine.

## Related

* [std-CONFIG] states the exact-version rule this departs from.
* [tol-dotnet-sdk] records the range the workflows install.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-CONFIG]: ../standards/configuration.md
[tol-dotnet-sdk]: ../tools/build/dotnet-sdk.md
