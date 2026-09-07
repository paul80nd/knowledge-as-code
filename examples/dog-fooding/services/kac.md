---
id: svc-kac
type: service
tier: descriptive
status: live
repo: knowledge-as-code
platform: mixed
criticality: critical
depends-on:
data-stores:
owner: paul.law
facets:
tags: [ cli, dotnet-tool, nuget ]
---

# kac

`Service: svc-kac` `LIVE`

The command line tool a corpus runs to validate itself, regenerate what it publishes, and seal what it exports. It ships
to nuget.org as `KnowledgeAsCode.Tool` and installs the command `kac`.

## What it does

`kac` reads one corpus and judges it against the schema above it. `Program.cs` wires Spectre.Console.Cli to the verbs
and does the corpus pre-flight, and every verb's logic lives in `kac.core`. Taken from the header comment in
`tooling/kac/kac.csproj`.

The command surface is `validate`, `generate`, `checks`, `export`, `pack`, `restore`, `bundle`, `update` and `new`. The
documentation site carries a page for each of them, at <https://paul80nd.github.io/knowledge-as-code/cli/>.

Two walk-ups decide what it reads. `.corpus.yaml` finds the corpus, and `.schema/` above that finds what to judge the
corpus against. Taken from `tooling/CLAUDE.md`.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), at `tooling/kac`, with the
  engine beside it at `tooling/kac.core`
* **Platform**: .NET 10, packed as a dotnet tool
* **Deployed as**: a NuGet package on nuget.org, installed by `dotnet tool install`

## Environments

| Environment | URL                                                   | Notes                                                                                |
|-------------|-------------------------------------------------------|--------------------------------------------------------------------------------------|
| Development | No published URL                                      | A checkout runs it as `dotnet run --project tooling/kac -- <verb>`.                  |
| Test        | No published URL                                      | The `tool` job in `kac.yml` packs it and stands a corpus up with the installed copy. |
| Production  | <https://www.nuget.org/packages/KnowledgeAsCode.Tool> | Published by `publish-tool.yml` on a push to `main`.                                 |

## Dependencies

None in this catalogue. `kac` reads a corpus from the filesystem and calls nothing at run time.

The packages it is built from are in the tool register: [tol-spectre-console], [tol-yamldotnet] and [tol-markdig].

## Data

None of its own. Every input is a file in the corpus it was pointed at, and every output is written back beside that
corpus under `.dist/`, `.imports/` or `_reports/`. Taken from the `.gitignore` rule in [std-CONFIG].

## Operational notes

* **`platform` carries `mixed` because the enum has no value for a command line tool.** The values in
  `.schema/services.yaml` are the library estate's, and every corpus in this repository reads that one schema rather
  than holding its own. Nothing here is built on the web, API, function or static runtimes the enum names.
* **`facets` is bare for the same reason.** An exposure facet describes an inbound surface, and a downloaded command
  has none. The two values the vocabulary offers, `public` and `internal`, both claim one.
* **A published version cannot be replaced, only followed.** `publish-tool.yml` reads `<Version>` in `kac.csproj` and
  publishes where nuget.org does not already hold that version, so the version moves by hand before the merge.
* **Runbooks**: [rbk-nuget-404-on-publish] covers the window in which nuget.org answers 404 for a version it has
  already accepted.

[rbk-nuget-404-on-publish]: ../runbooks/nuget-404-on-publish.md
[std-CONFIG]: ../standards/configuration.md
[tol-markdig]: ../tools/build/markdig.md
[tol-spectre-console]: ../tools/build/spectre-console.md
[tol-yamldotnet]: ../tools/build/yamldotnet.md
