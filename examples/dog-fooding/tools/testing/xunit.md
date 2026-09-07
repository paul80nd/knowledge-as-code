---
id: tol-xunit
type: tool
tier: descriptive
status: approved
versions: 3.1.x
licence: Apache-2.0
decided-in:
replaces:
successor:
owner: paul.law
tags: [ testing, unit-tests ]
---

# xUnit.net

`Tool: tol-xunit` `APPROVED`

The test framework two of the three test layers run on: the unit tests in `kac.tests`, and the behaviour specs in
`kac.features` that [tol-reqnroll] binds to it.

## What we use it for

`kac.tests` asserts what `kac.core` does with a corpus it builds for the purpose. `kac.features` runs Gherkin feature
documents through the same runner, so one `dotnet test` command covers either project.

`Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` sit beside it in both projects. Neither is a choice of its
own: they are what makes `dotnet test` find and run the tests.

The third layer, `tooling/kac-tests.cs`, uses none of this. It is a file-based program comparing `kac` output against
committed goldens, and it reports its own results.

## Status

**approved** since 2026-08-04.

## Where it is used

* [svc-kac] is tested with it.

## Alternatives considered

None. `kac.features` needs a runner Reqnroll binds to, and xUnit was already carrying the unit tests.

## Licence and obligations

Apache-2.0. It asks that the licence and any notice file travel with a redistribution. Nothing here redistributes it:
the packages are test-only and no published artefact carries them.

## Related

* [tol-reqnroll] runs the feature documents on top of it.

[svc-kac]: ../../services/kac.md
[tol-reqnroll]: reqnroll.md
