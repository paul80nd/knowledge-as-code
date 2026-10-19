---
id: tol-setup-dotnet
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/actions/setup-dotnet, versions: 6.x }
homepage: https://github.com/actions/setup-dotnet
licence: MIT AND Apache-2.0 AND BlueOak-1.0.0 AND ISC AND 0BSD
licence-declared: MIT
decided-on: "2026-08-03"
review-by: "2027-08-03"
owner: human:paul.law
tags: [ dotnet, github-actions, sdk ]
---

# actions/setup-dotnet

`Tool: tol-setup-dotnet` `APPROVED`

The action that installs the .NET SDK on the runner. Every job that builds, tests, packs or publishes with `dotnet`
runs it.

## What we use it for

Every step passes `dotnet-version: 10.0.x`, which installs the newest 10.0 SDK on the day. The `round-trip` and
`import-round-trip` jobs in `kac.yml` run it on `windows-latest` as well as `ubuntu-latest`.

The action takes no token. It runs in the `publish` jobs of `publish-corpus.yml`, `publish-plugin.yml` and
`publish-tool.yml`, so its code runs in a job with a write permission.

Every step pins the action to the commit behind `v6.0.0`, and Dependabot moves the pin. `versions` approves the major
the pin sits in.

## Where it is used

* [svc-corpus-feed] is packed with the SDK it installs.
* [svc-kac] is built, tested and packed with it.
* [svc-marketplace] is bundled with it.

## Alternatives considered

None recorded.

## Licence and obligations

The repository and its `package.json` declare MIT. The action runs bundled code under `dist/`, and the `.licenses/`
folder at the pinned commit records each npm dependency in it. Most are MIT. The others are these:

| Licence       | Packages                                                                         |
|---------------|----------------------------------------------------------------------------------|
| Apache-2.0    | `@protobuf-ts/runtime`, `@protobuf-ts/runtime-rpc`, `before-after-hook`          |
| BlueOak-1.0.0 | one version of `minimatch`                                                       |
| ISC           | the other version of `minimatch`, `semver`, `universal-user-agent`               |
| 0BSD          | `tslib`                                                                          |

The five entries marked `other` are two versions of `@actions/http-client`, `balanced-match`, `brace-expansion` and
`concat-map`. Each licence file is the MIT text.

The runner fetches the action for each job, and nothing here redistributes it, so nothing follows for anything this
repository publishes.

## Related

* [dev-no-reproducible-build] records that `10.0.x` installs whichever 10.0 SDK is newest.
* [std-CI] requires every action to be pinned to a commit.
* [tol-dotnet-sdk] is the SDK it installs.

[dev-no-reproducible-build]: ../../deviations/no-reproducible-build.md
[std-CI]: ../../standards/workflows.md
[svc-corpus-feed]: ../../services/corpus-feed.md
[svc-kac]: ../../services/kac.md
[svc-marketplace]: ../../services/marketplace.md
[tol-dotnet-sdk]: ../build/dotnet-sdk.md
