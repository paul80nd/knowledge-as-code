---
id: ctl-0008
type: control
tier: normative
status: active
verifies: [ std-NETTST ]
mechanism: not-enforced
applies-to:
  - all
review-by: "2027-10-17"
owner: human:paul.law
tags: [ coverage, dotnet, testing ]
---

# Nothing checks that a .NET build collects coverage

`Control: ctl-0008` `ACTIVE`

No check fails a .NET build that runs its tests and collects no coverage.

## What it checks

* [std-NETTST.coverage-is-collected-by-the-build] says a build "**MUST** collect coverage from its test run and publish
  the report".
* It says a build "**SHOULD** write the report in Cobertura format: with `--collect:"XPlat Code Coverage"` under
  VSTest, or with `--coverage --coverage-output-format cobertura` under Microsoft.Testing.Platform".

## How it works

Nothing runs. The check this rule needs is a pipeline step after `dotnet test`. It fails where the run wrote no
Cobertura report:

```sh
find . -path '*/TestResults/*' -name '*cobertura.xml' | grep -q .
```

The search matches both forms the `SHOULD` names. VSTest writes `TestResults/<guid>/coverage.cobertura.xml`, and
Microsoft.Testing.Platform writes `TestResults/<guid>.cobertura.xml`. A build using another collector, as the `SHOULD`
allows, changes the path the step searches for.

The step has to come from a shared pipeline template that branch policy requires. A team writing its own pipeline
leaves out the step along with the collector. A pre-commit check or a Claude Code hook sees the pipeline definition, and
could only search it for the collector flag.

## Coverage and gaps

A reviewer reading the pipeline definition is the only thing that notices a missing collector today.

The check proves a report was written. It does not prove the report was published, or that each test project is
excluded from its own figure.

[std-NETTST.coverage-is-collected-by-the-build]: ../standards/platform/dotnet/testing.md#coverage-is-collected-by-the-build
