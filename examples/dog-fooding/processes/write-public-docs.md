---
id: prc-write-public-docs
type: process
tier: procedural
status: active
applies-to: [ svc-docs-site, svc-kac ]
last-rehearsed: "never"
owner: human:paul.law
tags: [ mkdocs, nuget ]
---

# Write the public documentation

`Process: prc-write-public-docs` `ACTIVE`

Write or rewrite a page addressed to somebody outside this repository.

## When to use this

You are changing the root `README.md`, the package page nuget.org renders, or a page of the documentation site. The
reader has installed nothing and has nobody here to ask, and that is the whole difference from every other surface.

## Prerequisites

* `technical-writing` and `writing-the-docs`, loaded in that order.
* The source for every fact the page states, so each one can be checked.
* The .NET SDK, where you touched the package page.

## Steps

1. Read the existing page cold before you open anything else. Write down what a reader knows after the first paragraph,
   after the second, and after the first section. That is the diagnosis.
2. Check every factual claim against its source, including one you are rewording rather than inventing. A rewrite drops
   a fact more easily than it drops a word, and the reader can check nothing.
3. Open on what the reader gets. A definition answers a question nobody has asked yet.
4. Put each fact where its reader meets it. A flag met while running the tool belongs at `--help` and in the reference.
   A page somebody reads before installing carries what decides them.
5. Answer [std-PROSE]'s conformance checklist against every page you changed.
6. Answer [std-A11Y]'s conformance checklist against every page you changed.
7. Pack the package where you touched the package page. `dotnet pack tooling/kac/kac.csproj` proves it still renders as
   the readme nuget.org receives.
8. Run `mkdocs build --strict` where you touched the site. A dead link fails it, and a page the nav does not list is
   caught by `NavigationTests` instead.
9. Run [prc-pull-request].

## Verification

The build reports no dead link, every box on both conformance checklists is ticked, and a reader who has installed
nothing can say what the thing is after the first paragraph.

Close by naming what changed and why, every claim you checked against the source, and every rule you left behind.

## Related

* [std-A11Y] carries what a page owes a reader who cannot see it. Step 6 is where you answer it.
* [std-PROSE] states the writing rules every page here follows. Step 5 is where you answer it.
* [svc-docs-site] is what publishes the site.
* [prc-pull-request] is how the change lands.

[prc-pull-request]: pull-request.md
[std-A11Y]: ../standards/accessibility.md
[std-PROSE]: ../standards/prose.md
[svc-docs-site]: ../services/docs-site.md
