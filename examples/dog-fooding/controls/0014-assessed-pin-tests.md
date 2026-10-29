---
id: ctl-0014
type: control
tier: normative
status: active
verifies: [ std-A11Y.ASSESS ]
mechanism: ci
frequency: per-pr
evidence: The `tool` job's log on the pull request, under the step "Run kac.core unit tests".
applies-to:
  - svc-docs-site
  - svc-kac
review-by: "2027-10-19"
owner: human:paul.law
tags: [ accessibility, assessment, dependabot ]
---

# A pinned component's assessment names the version it is pinned at

`Control: ctl-0014` `ACTIVE`

A pull request that moves the major or minor of an assessed component fails until its tool record moves with it. Below
0.1.0, any move of the pin fails it.

## What it checks

* [std-A11Y.ASSESS] says "The assessment **MUST** name the version it
  was made against".
* It says "A pull request moving a pin to a new major or minor version **MUST** include a new assessment".
* It says "A pull request moving a pin below 0.1.0 to any new version **MUST** include a new assessment".

## How it works

`AssessedPinTests` in `tooling/kac.tests` reads every tool record in this corpus that has an `Accessibility` section
and `packages`. For each package it does four things:

* It finds the pin. A `pkg:nuget` package is a `PackageReference` in a project under `tooling/`. A `pkg:pypi` package
  is a line in `docs/requirements.txt`.
* It fails where nothing pins the package.
* It fails where the pin falls outside the `versions` range, such as `0.57.x`. Below 0.1.0 the range is the pinned
  version itself, such as `0.0.72`, and it fails where the two differ.
* It fails where the `Accessibility` section names no version with the pin's major and minor, such as `0.57.2`. Below
  0.1.0 the section has to name the pinned version itself.

The `tool` job runs it under `dotnet test --project tooling/kac.tests` on every pull request, Dependabot's included.
It reads the files as they stand, so it needs no base branch.

## Coverage and gaps

This check proves that the range and the version the assessment names match the pin. It does not prove that anybody
repeated the assessment. A reviewer who edits the version number alone passes it, so the conformance checklist in
[std-A11Y] still asks the reviewer.

It reads NuGet and pip pins only. A component rendering either surface through another ecosystem goes unchecked until
this test learns its manifest.

At or above 0.1.0, a patch release inside the range passes, because std-A11Y asks for a new assessment there only at
a major or a minor.

[std-A11Y]: ../standards/accessibility.md
[std-A11Y.ASSESS]: ../standards/accessibility.md#assess-a-pinned-component-carries-an-accessibility-assessment
