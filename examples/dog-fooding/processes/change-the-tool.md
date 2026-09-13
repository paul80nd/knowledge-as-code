---
id: prc-change-the-tool
type: process
tier: procedural
status: active
applies-to: [ svc-kac ]
last-rehearsed: "never"
owner: human:paul.law
tags: [ checks, dotnet ]
---

# Change the tool

`Process: prc-change-the-tool` `ACTIVE`

Change `kac` itself, or add a check it cannot express in the schema.

## When to use this

The question needs C#, or the command itself changes. A check that is a predicate over frontmatter, sections, links or
length is an `expr:` on a rule, and costs the YAML and a fixture.

## Prerequisites

* The tool guidance at `tooling/CLAUDE.md`, which states the test for expression against class and the two rule
  interfaces.
* The .NET SDK, and `dotnet run --project tooling/kac --` in place of a `kac` on your path.
* The fact table in `docs/design/expressions.md`, which lists what an expression can already read.

## Steps

1. Decide which of the three routes applies.
   * An `expr:` over facts that already exist is schema alone. Run [prc-change-the-schema] and stop.
   * A new fact plus an `expr:` is both. Adding a fact is one method on `Facts`, one row in `RuleExpr.Functions` and
     one row in the fact table, which `DocumentationTests` keeps equal.
   * A question no expression can ask is a rule class. Continue with this page.

   Read the fact table before you settle on a route. `words()` is whole-document, and no fact measures a section.
2. Write the class under `kac.core/Rules/`. Take the narrower interface wherever it will do.
3. Add the class to the registry: `DocumentRules.All`, or `CorpusRules.All` for a corpus rule.
4. Write its unit tests in `tooling/kac.tests/`, beside `CorpusRuleTests.cs` and `DocumentRuleTests.cs`. Nothing under
   `kac.core/Rules/` is a test.
5. Declare what it reports: an entry in `_checks.yaml`, and either a row in `ChecksTable.DocRows` or
   `on-type-page: false`. Three places have to agree, and each one fails a different meta-test.
6. Write a fixture that trips it, one per check id it emits. The coverage gate counts ids, not branches.
7. Load `technical-writing`, then `writing-in-the-tool`, for the comments and the test names. Read the code under every
   comment you touch.
8. Load `technical-writing`, then `writing-the-docs`, for the pages a reader outside this repository reads.
9. Update the command's page under `docs/cli/`.
10. Update `tooling/README.md` where the change affects it.
11. Regenerate the usage block with `KAC_UPDATE_DOCS=1 dotnet test tooling/kac.tests`.
12. Write the changelog entry under `## Unreleased`, because a user can observe this change.
13. Run all four layers, one `kac` invocation at a time: unit, behaviour specs, golden fixtures, then `validate` and
    `generate --check` in each corpus. Run the goldens with `GITHUB_ACTIONS=true`, which is how CI runs them.

    While you are still changing the code, `dotnet test tooling/kac.tests --filter "Kind!=Repository"` skips the guards
    that check this repository's own pages. Run everything before step 14.
14. Run [prc-pull-request].

## Verification

All four layers pass, and the command's page matches the parser's own model.

Close by stating what the tool now does, which layer proves it, and what you decided against building in C#.

## If it goes wrong

A warning fails the build here, so an analyser complaint stops you locally, before CI. [prc-pull-request] states what a
release costs, and step 14 is where you read it.

## Related

* [std-CI.a-version-moves-by-hand-and-publishes-once] states the changelog entry and the release. Step 12 says where
  the entry falls in the order.
* [prc-change-the-schema] is the cheaper answer where the check fits an expression.
* [svc-kac] is what this publishes.

[prc-change-the-schema]: change-the-schema.md
[prc-pull-request]: pull-request.md
[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
[svc-kac]: ../services/kac.md
