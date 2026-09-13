---
id: svc-kac
type: service
tier: descriptive
status: live
repo: knowledge-as-code
platform: dotnet-tool
criticality: critical
owner: human:paul.law
tags: [ cli, dotnet-tool, nuget ]
---

# kac

`Service: svc-kac` `LIVE`

`kac` is the command line tool a corpus runs to validate itself, regenerate what it publishes, and package what it
exports. It ships to nuget.org as `KnowledgeAsCode.Tool` and installs the command `kac`.

## What it does

`kac` reads one corpus and judges it against the schema above it. `Program.cs` wires Spectre.Console.Cli to the verbs
and runs the corpus pre-flight. Every verb's logic is in `kac.core`. Taken from the header comment in
`tooling/kac/kac.csproj`.

The command surface is `new`, `validate`, `generate`, `restore`, `export`, `bundle`, `pack`, `checks`, `report` and
`update`. The documentation site has a page for each verb, at <https://paul80nd.github.io/knowledge-as-code/cli/>.

Two walk-ups decide what it reads. `kac` walks up for `.corpus.yaml` to find the corpus, then walks up again for
`.schema/` to find what to judge the corpus against. Taken from `tooling/CLAUDE.md`.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), at `tooling/kac`, with the
  engine beside it at `tooling/kac.core`
* **Platform**: .NET 10, packed as a dotnet tool
* **Deployed as**: a NuGet package on nuget.org, installed by `dotnet tool install`

## Environments

| Environment | URL                                                   | Notes                                                                               |
|-------------|-------------------------------------------------------|-------------------------------------------------------------------------------------|
| Development | No published URL                                      | A checkout runs it as `dotnet run --project tooling/kac -- <verb>`.                 |
| Test        | No published URL                                      | The `tool` job in `kac.yml` packs it and creates a corpus with the installed copy.  |
| Production  | <https://www.nuget.org/packages/KnowledgeAsCode.Tool> | Published by `publish-tool.yml` on a push to `main`.                                |

## Dependencies

None in this catalogue. `kac` reads a corpus from the filesystem. It calls nothing at run time.

The packages it is built from are in the tool register: [tol-spectre-console], [tol-yamldotnet] and [tol-markdig].

## Data

None of its own. Every input is a file in the corpus it was pointed at. Every output is written back beside that corpus
under `.dist/`, `.imports/` or `_reports/`. Taken from the `.gitignore` rule in [std-CONFIG].

## Operational notes

* **`facets` is left out because a downloaded command has no inbound surface.** An exposure facet describes the surface
  a service is called on. Nothing calls `kac`: it runs on the machine that installed it.
* **A published version cannot be replaced. A correction takes a higher version.** `publish-tool.yml` reads `<Version>`
  in `kac.csproj` and publishes where nuget.org does not already have that version, so the version moves by hand before
  the merge.
* **Runbooks**: [rbk-nuget-404-on-publish] covers the window in which nuget.org answers 404 for a version it has
  already accepted.

[rbk-nuget-404-on-publish]: ../runbooks/nuget-404-on-publish.md
[std-CONFIG]: ../standards/configuration.md
[tol-markdig]: ../tools/build/markdig.md
[tol-spectre-console]: ../tools/build/spectre-console.md
[tol-yamldotnet]: ../tools/build/yamldotnet.md
