---
id: std-NETTST
tier: normative
status: draft
implements: [ pol-AUTV.COVER, pol-AUTV.LEVELS ]
applies-to:
  - all
review-by: "2027-08-31"
owner: paul.law
tags: [ csharp, testcontainers, xunit ]
---

# A .NET test suite is xUnit, and an integration test owns its dependencies

`Standard: std-NETTST` `DRAFT`

## Summary

C# tests are written with xUnit. A unit test touches no process boundary, and an integration test starts the
dependencies it needs in containers of its own.

## Rules

This standard adds the .NET shape to [std-TEST], which says which level a test belongs at. Read that one first.

### The framework and the project layout

- A test project **MUST** use xUnit, so one runner reports every suite in the estate.
- A test project **MUST** be named for the project it tests, with a `.Tests` or `.IntegrationTests` suffix.
- A project **MUST** assert with `Assert` or with FluentAssertions.
- A project **MUST** keep to the one it picked.

_**Covers:** [pol-AUTV].LEVELS_

### A unit test crosses no boundary

- A unit test **MUST NOT** open a socket, read the filesystem, or start a container.
- A test **MUST NOT** call `Thread.Sleep` or `Task.Delay` to wait for something.
- A test **MUST** take its time from an injected clock.
- A test **MUST** await every asynchronous call, so a failure surfaces in the test that caused it.

_**Covers:** [pol-AUTV].LEVELS_

### An integration test starts what it needs

- An integration test **MUST** start its dependencies with Testcontainers.
- An integration test for an ASP.NET service **MUST** host it with `WebApplicationFactory` rather than by deploying it.
- An integration test **MUST** leave no state behind, so the suite passes when it runs twice.

_**Covers:** [pol-AUTV].LEVELS_

### Coverage is collected by the build

- A build **MUST** collect coverage with `dotnet test --collect:"XPlat Code Coverage"` and publish the report.
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

The avoided signature is `async void`, so xUnit cannot await it and an exception thrown after the first await kills the
run rather than failing the test.

```
Good
  await using var db = new PostgreSqlBuilder().WithImage("postgres:16.4").Build();

Avoid
  var db = "Server=sql-test-01;Database=covers_tests;";
```

The avoided form shares one database with everybody else's run, so a failure means reading the other suites before your
own.

## Conformance checklist

- [ ] Every test project references xUnit and no second test framework.
- [ ] No unit test project references Testcontainers or a database driver.
- [ ] A search for `Thread.Sleep` and `async void` across the test projects returns nothing.
- [ ] The integration suite passes on a machine with no services running but Docker.
- [ ] Running the integration suite twice in a row passes both times.
- [ ] The build publishes a coverage report, and the test projects are excluded from it.

## Rationale and provenance

An integration suite that shares a database fails for reasons that belong to somebody else, and a suite people cannot
trust stops being read. Starting the dependency per run costs a few seconds and buys a result that means something.

## Sources and further reading

- **Normative.** [xUnit.net documentation] is the framework these rules are written against.
- **Informative.** [Testcontainers for .NET] covers the container lifecycle an integration test depends on.

[Testcontainers for .NET]: https://dotnet.testcontainers.org/
[std-TEST]: ../../common/testing.md
[xUnit.net documentation]: https://xunit.net/docs/getting-started/v3/getting-started
[pol-AUTV]: ../../../policies/delivery/autv-automated-verification.md#clauses
