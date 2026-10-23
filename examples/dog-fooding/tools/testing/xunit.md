---
id: tol-xunit
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:nuget/xunit.v3, versions: 4.0.x }
homepage: https://xunit.net/
licence: Apache-2.0 AND MIT
licence-declared: Apache-2.0 AND MIT
decided-on: "2026-08-04"
review-by: "2027-08-04"
owner: human:paul.law
tags: [ testing, unit-tests ]
---

# xUnit.net

`Tool: tol-xunit` `APPROVED`

The test framework all three test layers run on: the unit tests in `kac.tests`, the behaviour specs in `kac.features`
that [tol-reqnroll] binds to it, and the golden scenarios in `kac.goldens`.

## What we use it for

`kac.tests` asserts what `kac.core` does with a corpus it builds for the purpose. `kac.features` runs Gherkin feature
documents through the same runner. `kac.goldens` runs `kac` as a process over each fixture and compares what it wrote
with the committed goldens, one test case per fixture. One `dotnet test --project <path>` command covers any of the
three.

All three projects reference xUnit as `xunit.v3`, which makes each project a Microsoft.Testing.Platform app.
`global.json` sets `test.runner` to `Microsoft.Testing.Platform`, so `dotnet test` runs each project as that app. A
trait filter is an argument to the app: `--filter-trait "Kind=Repository"` selects the repository tests, and
`--filter-not-trait` skips them. In `kac.goldens`, `--filter-trait "Scenario=<name>"` selects one fixture.

`xunit.v3` brings in `Microsoft.Testing.Extensions.Telemetry`, which sends usage data to Microsoft unless told not to.
Both workflows set `TESTINGPLATFORM_TELEMETRY_OPTOUT` and `DOTNET_CLI_TELEMETRY_OPTOUT`, so CI sends none. A
contributor's machine sends it unless one of the two is set there.

## Where it is used

* [svc-kac] is tested with it.

## Alternatives considered

None. `kac.features` needs a runner Reqnroll binds to, and xUnit already ran the unit tests.

## Licence and obligations

`xunit.v3` is Apache-2.0. The Microsoft.Testing.Platform packages it brings in are MIT, which is why this entry names
both. Apache-2.0 asks that the licence and any notice file travel with a redistribution. Nothing here redistributes
them: the packages are test-only, and no published artefact includes them.

## Related

* [tol-reqnroll] runs the feature documents on top of it.

[svc-kac]: ../../services/kac.md
[tol-reqnroll]: reqnroll.md
