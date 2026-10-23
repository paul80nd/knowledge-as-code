---
id: dev-no-reproducible-build
type: deviation
tier: normative
status: closed
departs-from:
  - eng:pol-AUTV.REPRO
  - std-CONFIG.a-pin-names-an-exact-version
risk: low
accepted-on: "2026-09-07"
review-by: "2027-09-07"
closed-on: "2026-10-23"
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ build, reproducibility, sdk ]
---

# A build of `kac` takes whichever .NET 10 SDK the runner installs

`Deviation: dev-no-reproducible-build` `CLOSED`

Every workflow that built `kac` installed the .NET SDK as `10.0.x`, so two builds of one commit could run on different
SDKs. `global.json` now pins the SDK, and this record is closed.

## What we are doing instead

Each job that built `kac` installed the SDK with [tol-setup-dotnet] and `dotnet-version: 10.0.x`, which took the
newest 10.0 SDK on the day. [tol-dotnet-sdk] approved the same range. No `global.json` fixed the version, so version
control alone did not say which SDK built a commit.

The package published to nuget.org was built once, by the publish workflow, from the commit on `main`. Its run log
showed the SDK that job installed.

## Why we need it

Fixing the version needed a `global.json` and a way to move it when a patch shipped, and neither had been set up. The
range took each SDK patch without a pull request.

## What the risk is

Two builds of one commit could run on different SDK patches, and a difference between them showed only in the package
that shipped. Nothing here would explain it without reading both run logs. An SDK patch seldom changes what a build
produces, so the cost was a confusing difference and not a broken release.

## What compensates

* The publish workflow was the only thing that built what shipped, and its run log recorded the commit and the SDK.
* [ctl-0007] packs the tool and installs it on every pull request. That copy runs `kac new` and validates the corpus
  it created, so a build that cannot run is caught before it ships.
* nuget.org never lets a published version be replaced, so a rebuild cannot overwrite what people already have.

## How it closes

[#971](https://github.com/paul80nd/knowledge-as-code/issues/971) closed it. `global.json` at the repository root names
the exact SDK with `rollForward: disable`. Every workflow installs the version it names with `global-json-file`, and
Dependabot moves it when a patch ships. [tol-dotnet-sdk] states the version.

## Scope

Every build of `kac`: the publish workflow, CI, and a contributor's own machine.

## Related

* [std-CONFIG] states the exact-version rule this departed from.
* [tol-dotnet-sdk] records the SDK the workflows install.
* [tol-setup-dotnet] is the action that installs it.

[ctl-0007]: ../controls/0007-corpus-validation.md
[std-CONFIG]: ../standards/configuration.md
[tol-dotnet-sdk]: ../tools/build/dotnet-sdk.md
[tol-setup-dotnet]: ../tools/workflows/setup-dotnet.md
