---
id: tol-xunit
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:nuget/Microsoft.NET.Test.Sdk, versions: 18.x }
  - { purl: pkg:nuget/xunit.runner.visualstudio, versions: 3.1.x }
  - { purl: pkg:nuget/xunit.v3, versions: 3.1.x }
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

The test framework two of the three test layers run on: the unit tests in `kac.tests`, and the behaviour specs in
`kac.features` that [tol-reqnroll] binds to it.

## What we use it for

`kac.tests` asserts what `kac.core` does with a corpus it builds for the purpose. `kac.features` runs Gherkin feature
documents through the same runner, so one `dotnet test` command covers either project.

`Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are referenced beside it in both projects. They are what
makes `dotnet test` find and run the tests, and neither was chosen on its own.

The third layer, `tooling/kac-tests.cs`, uses none of this. It is a file-based program. It compares `kac` output
against committed goldens, and reports its own results.

## Where it is used

* [svc-kac] is tested with it.

## Alternatives considered

None. `kac.features` needs a runner Reqnroll binds to, and xUnit already ran the unit tests.

## Licence and obligations

The two xUnit packages are Apache-2.0 and `Microsoft.NET.Test.Sdk` is MIT, which is why this entry names both.
Apache-2.0 asks that the licence and any notice file travel with a redistribution. Nothing here redistributes them:
the packages are test-only, and no published artefact includes them.

## Related

* [tol-reqnroll] runs the feature documents on top of it.

[svc-kac]: ../../services/kac.md
[tol-reqnroll]: reqnroll.md
