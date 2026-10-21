---
id: std-NETTST
type: standard
tier: normative
status: draft
implements: [ pol-AUTV.COVER, pol-AUTV.LEVELS ]
depends-on: [ std-TEST ]
verified-by: [ ctl-0008 ]
applies-to:
  - all
review-by: "2027-08-31"
owner: human:paul.law
tags: [ csharp, testcontainers, xunit ]
---

# A .NET test project runs under one framework, and an integration test owns its dependencies

`Standard: std-NETTST` `DRAFT`

## Summary

A repository's C# tests use one test framework and one assertion style. A unit test crosses no process boundary. An
integration test starts the dependencies it needs, and shares none of them with another run.

## Rules

This standard adds the .NET detail to [std-TEST], which says which level a test belongs at. Read [std-TEST] first.

### The framework and the project layout

- A repository **MUST** use one test framework across its test projects.
- Every test project **MUST** run under `dotnet test`.
- A test project **SHOULD** use xUnit, because the estate's examples and controls are written against it.
- A test project **MUST** be named for the project it tests, with a `.Tests` or `.IntegrationTests` suffix.
- A project **MUST** use one assertion style throughout its tests.
- A project **SHOULD** assert with the test framework's own `Assert`.
- A project **MAY** assert with AwesomeAssertions, or with another library whose licence is one of the allowed ids in
  [std-DEPS's conformance checklist](../../common/dependencies.md#conformance-checklist).

_**Covers:** [pol-AUTV].LEVELS_

### A unit test crosses no boundary

- A unit test **MUST NOT** open a socket, read the filesystem, or start a container.
- A test **MUST NOT** call `Thread.Sleep` or `Task.Delay` to wait for something.
- A test **SHOULD** take its time from an injected clock, such as `TimeProvider`.
- A test **MUST** await every asynchronous call, so the test that caused a failure is the test that reports it.

_**Covers:** [pol-AUTV].LEVELS_

### An integration test starts what it needs

- An integration test **MUST** start the dependencies it needs.
- An integration test **MUST NOT** share an instance of a dependency with another run.
- An integration test **SHOULD** start those dependencies with Testcontainers.
- An integration test for an ASP.NET service **SHOULD** host it in the test process with `WebApplicationFactory`.
- An integration test **MUST** leave no state behind, so the suite passes when it runs twice.

_**Covers:** [pol-AUTV].LEVELS_

### Coverage is collected by the build

- A build **MUST** collect coverage from its test run and publish the report.
- A build **SHOULD** collect it with `dotnet test --collect:"XPlat Code Coverage"`, which writes a Cobertura report.
- A test project **MUST** be excluded from its own coverage figure.

_**Covers:** [pol-AUTV].COVER_

## Examples

```
Good
  [Fact]
  public async Task Authorise_Returns503_WhenThePspTimesOut()

Avoid
  [Fact]
  public async void TestAuthorise()
```

The avoided signature is `async void`, so xUnit cannot await it. An exception thrown after the first await ends the
whole run, and the test is never reported as failed.

```
Good
  await using var db = new PostgreSqlBuilder().WithImage("postgres:16.4").Build();

Avoid
  var db = "Server=sql-test-01;Database=covers_tests;";
```

The avoided form shares one database with every other run, so diagnosing a failure means reading the other suites
before your own.

## Conformance checklist

- [ ] Every test project in the repository references the same test framework, and `dotnet test` runs them all.
- [ ] Every test project uses one assertion style.
- [ ] No unit test project references Testcontainers or a database driver.
- [ ] A search for `Thread.Sleep` and `async void` across the test projects returns nothing.
- [ ] The integration suite passes on a machine running no shared database, queue or service.
- [ ] Running the integration suite twice in a row passes both times.
- [ ] The build publishes a coverage report, and the test projects are excluded from it.

## Rationale and provenance

Starting the dependency per run costs a few seconds and gives a result you can act on. A suite that shares a database
fails for reasons that belong to somebody else's run, and people stop reading a suite they cannot trust.

The rules name a test framework, a container tool and an assertion library only in a `SHOULD` or a `MAY`, because
any product reaching the outcome meets the `MUST`.
FluentAssertions moved to a commercial licence at [version 8.0.0], so this standard names AwesomeAssertions. It is a
community fork of FluentAssertions 7, published under Apache 2.0.

## Sources and further reading

- **Informative.** [xUnit.net documentation] describes the framework these rules recommend.
- **Informative.** [Testcontainers for .NET] covers the container lifecycle an integration test depends on.
- **Informative.** [AwesomeAssertions] is the assertion library this standard names.

## Changelog

- 2026-10-21: an assertion library's licence is checked against the allowed ids in std-DEPS's conformance checklist.
- 2026-10-18: graded the rules. The outcome stays a `MUST`, and xUnit, Testcontainers, `WebApplicationFactory`, the
  injected clock and the coverage collector become `SHOULD`. AwesomeAssertions replaces FluentAssertions.

To depart from a `MUST` or `MUST NOT` rule here, request a deviation. To depart from a `SHOULD` or `SHOULD NOT` rule,
say why in the change.

[AwesomeAssertions]: https://github.com/AwesomeAssertions/AwesomeAssertions
[Testcontainers for .NET]: https://dotnet.testcontainers.org/
[version 8.0.0]: https://github.com/fluentassertions/fluentassertions/releases/tag/8.0.0
[xUnit.net documentation]: https://xunit.net/docs/getting-started/v3/getting-started
[pol-AUTV]: ../../../policies/delivery/autv-automated-verification.md#clauses
[std-TEST]: ../../common/testing.md
