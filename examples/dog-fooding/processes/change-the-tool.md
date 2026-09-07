---
id: prc-change-the-tool
tier: procedural
status: active
applies-to: [ svc-kac ]
last-rehearsed: "never"
rehearsal-frequency:
requires-access:
owner: paul.law
tags: [ checks, dotnet ]
---

# Change the tool

`Process: prc-change-the-tool` `ACTIVE`

Change `kac` itself, or add a check it cannot express in the schema.

## When to use this

A check that is a predicate over frontmatter, sections, links or length is an `expr:` on a rule, and costs the YAML and
a fixture. Come here where the question needs C#, or where the command itself changes.

## Prerequisites

* The tool guidance at `tooling/CLAUDE.md`, which carries the test for expression against class and the two rule
  interfaces.
* The .NET SDK, and `dotnet run --project tooling/kac --` rather than a `kac` on your path.
* The fact table in `docs/design/expressions.md`, which says what an expression can already reach.

## Steps

1. Decide which of the three rungs you are on.
   * An `expr:` over facts that already exist is schema alone. Run [prc-change-the-schema] and stop.
   * A new fact plus an `expr:` is both. Adding a fact is one method on `Facts`, one row in `RuleExpr.Functions` and
     one row in the fact table, which `DocumentationTests` holds equal.
   * A question no expression can ask is a rule class. Carry on down this page.

   Read the fact table before you settle on a rung. `words()` is whole-document, and no fact measures a section.
2. Write the class under `kac.core/Rules/`. Its unit tests sit beside it and a line goes in the registry. Take the
   narrower interface wherever it will do.
3. Declare what it reports. An entry in `_checks.yaml`, and either a row in `ChecksTable.DocRows` or
   `on-type-page: false`. Three places have to agree and each fails a different meta-test.
4. Write a fixture that trips it, one per check id it emits. The coverage gate reads ids rather than branches.
5. Load `technical-writing`, then `writing-in-the-tool`, for the comments and the test names. Read the code under every
   comment you touch.
6. Update the command's page under `docs/cli/`, loading `writing-the-docs` for it, and `tooling/README.md` where the
   change reaches it. Regenerate the usage block with `KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests`.
7. Write the changelog entry under `## Unreleased`, because a user can observe this change.
8. Run all four layers, one `kac` invocation at a time: unit, behaviour specs, golden fixtures, then `validate` and
   `generate --check` in each corpus. Run the goldens as CI sees them, with `GITHUB_ACTIONS=true`.

   While you are still changing the code, `dotnet test tooling/kac.tests --filter "Kind!=Repository"` leaves out the
   guards that answer for this repository's own pages. Run everything before step 9.
9. Run [prc-pull-request].

## Verification

All four layers report green, and the command's page matches the parser's own model.

Close by naming what the tool now does, which layer proves it, and what you decided against building in C#.

## If it goes wrong

A warning fails the build here, so an analyser complaint stops you locally and not first in CI. What a release costs
is [prc-pull-request]'s to say, and step 9 is where you meet it.

## Related

* [std-CI.a-version-moves-by-hand-and-publishes-once] carries the changelog entry and the release. Step 7 says where in
  the order the entry falls.
* [prc-change-the-schema] is the cheaper answer where the check fits an expression.
* [svc-kac] is what this publishes.

[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
[svc-kac]: ../services/kac.md
