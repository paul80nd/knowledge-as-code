---
id: svc-corpus-feed
type: service
tier: descriptive
status: live
repo: knowledge-as-code
platform: static
criticality: important
owner: human:paul.law
tags: [ github-packages, nuget ]
---

# Corpus package feed

`Service: svc-corpus-feed` `LIVE`

The corpus package feed publishes the four worked corpora as packages a downstream corpus can restore. `kac pack`
builds each package, and `publish-corpus.yml` pushes it to GitHub Packages at the version that corpus's `.corpus.yaml`
states.

## What it does

One publisher builds four packages: `example-libraries`, `example-engineering`, `example-payments` and
`example-dogfooding`. A package id is the `corpus:` value in the descriptor. The version is `content-version`. Taken
from `docs/cli/pack.md`.

`publish-corpus.yml` runs on a push to `main` that touched one of the corpora. A `verify` job with no write permission
validates, exports and packs all four. A second job rebuilds the same package and pushes it. Taken from the header
comment in `.github/workflows/publish-corpus.yml`.

A step called **Decide whether this version is new** asks the registry which versions it already has. The push step is
skipped where this version is among them. A registry keeps a published version forever, so a correction takes a new
`content-version`.

Publishing these corpora does two jobs. It proves the pack-and-push half of `kac` against a real registry. It also lets
somebody deciding whether to adopt the framework pull a package and read it.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), at `examples`, published by
  `.github/workflows/publish-corpus.yml`
* **Platform**: a NuGet package containing one corpus export, with no runtime
* **Deployed as**: four packages on the GitHub Packages NuGet feed

## Environments

| Environment | URL                                                | Notes                                                                                                                                        |
|-------------|----------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| Development | No published URL                                   | `kac pack` writes the package under the corpus's own `.dist/package/`.                                                                       |
| Test        | No published URL                                   | The `corpora` job in `kac.yml` packs every corpus, and `import-round-trip` passes one to a consumer through a folder. Neither one publishes. |
| Production  | <https://nuget.pkg.github.com/paul80nd/index.json> | Published by `publish-corpus.yml` on a push to `main`. Reading the feed needs a GitHub token.                                                |

## Dependencies

None in this catalogue. A package is a file the registry serves. It calls nothing.

[svc-kac] builds every one of them. That is a build-time dependency, so it is not an edge here.

## Data

Each package contains one corpus export and nothing else. Three of them describe an invented estate, and
`example-dogfooding` describes this repository. Each package's description says which kind a reader has. `.corpus.yaml`
states that description, and `kac pack` writes it into the package envelope.

A consumer does not commit a restored package. `kac restore` writes what it takes into `.imports/`, which [std-CONFIG]
keeps untracked.

## Operational notes

* **One record covers four packages.** [The type page](../services.md) states the rule that allows this.
* **`platform` is `static` because a package has no runtime.** The value covers generated files served as they were
  built, and an export inside a NuGet package is exactly that.
* **`criticality` matches [svc-marketplace], though nothing here restores from the feed.** `examples/payments` and
  `examples/dog-fooding` name `../engineering/.dist/package` as their `source:`, so every build in this repository
  reads a folder. The reader who meets a broken feed is outside this repository, so the grade is above `supporting`.
* **`facets` is left out because a package has no inbound surface of its own.** GitHub owns the registry that serves
  it, and the same reasoning leaves [svc-kac] without a facet on nuget.org.
* **A corpus joins the publishing matrix by name.** `publish-corpus.yml` lists all four in its `paths:` filter and in
  both matrices. A listed corpus also needs a shortcode, because `kac pack` refuses a corpus without one.

[std-CONFIG]: ../standards/configuration.md
[svc-kac]: kac.md
[svc-marketplace]: marketplace.md
