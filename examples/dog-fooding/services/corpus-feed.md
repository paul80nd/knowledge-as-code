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

The four worked corpora published as packages a downstream corpus can restore. `kac pack` seals each one, and
`publish-corpus.yml` pushes it to GitHub Packages at the version that corpus's `.corpus.yaml` states.

## What it does

One publisher, four packages: `example-libraries`, `example-engineering`, `example-payments` and `example-dogfooding`.
A package id is the `corpus:` value in the descriptor and the version is `content-version`. Taken from
`docs/cli/pack.md`.

`publish-corpus.yml` runs on a push to `main` that touched one of the corpora. A `verify` job holding no write
permission validates, exports and packs all four. A second job rebuilds the same package and pushes it. Taken from the
header comment in `.github/workflows/publish-corpus.yml`.

A step called **Decide whether this version is new** asks the registry which versions it already holds, and the push
step is skipped where this one is among them. A registry keeps a published version forever, so a correction is a new
`content-version` and never a second push of this one.

Publishing these corpora is how this repository proves the pack-and-push half of its own tool against a real registry,
and how somebody deciding whether to adopt it can pull one and look.

## Where it lives

* **Repository**: [`knowledge-as-code`](https://github.com/paul80nd/knowledge-as-code), at `examples`, published by
  `.github/workflows/publish-corpus.yml`
* **Platform**: a NuGet package holding a sealed export, with no runtime
* **Deployed as**: four packages on the GitHub Packages NuGet feed

## Environments

| Environment | URL                                                | Notes                                                                                                                                        |
|-------------|----------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| Development | No published URL                                   | `kac pack` writes the package under the corpus's own `.dist/package/`.                                                                       |
| Test        | No published URL                                   | The `corpora` job in `kac.yml` seals every corpus, and `import-round-trip` passes one to a consumer through a folder. Neither one publishes. |
| Production  | <https://nuget.pkg.github.com/paul80nd/index.json> | Published by `publish-corpus.yml` on a push to `main`. Reading the feed needs a GitHub token.                                                |

## Dependencies

None in this catalogue. A package is a file the registry serves, and it calls nothing.

[svc-kac] seals every one of them, which is a build-time dependency rather than a call, so it is not an edge here.

## Data

Each package carries a sealed export of one corpus, and nothing else. Three of the four describe an invented estate,
and `example-dogfooding` describes this repository. A package's own description says which of the two a reader has.
`.corpus.yaml` states that description and `kac pack` writes it into the envelope.

A restored package is not the consumer's to keep. `kac restore` writes what it takes into `.imports/`, which
[std-CONFIG] keeps untracked.

## Operational notes

* **One record covers four packages.** [The type page](../services.md) carries the rule it stands on.
* **`platform` is `static` because a package has no runtime.** The value covers generated files served as they were
  built, and a sealed export is that inside a NuGet envelope.
* **`criticality` sits level with [svc-marketplace], though nothing here restores from the feed.** `examples/payments`
  and `examples/dog-fooding` name `../engineering/.dist/package` as their `source:`, so every build in this repository
  reads a folder. The reader who meets a broken feed is outside, which puts the grade above `supporting`.
* **`facets` is left out because a package has no inbound surface of its own.** The registry serving it is GitHub's,
  and the same reasoning leaves [svc-kac] without one on nuget.org.
* **A corpus joins the publishing matrix by name.** `publish-corpus.yml` lists all four in its `paths:` filter and in
  both matrices. Declaring a shortcode is what lets the pack succeed once it is listed, because a pack is refused
  without one.

[std-CONFIG]: ../standards/configuration.md
[svc-kac]: kac.md
[svc-marketplace]: marketplace.md
