---
id: tol-setup-go
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/actions/setup-go, versions: 7.x }
homepage: https://github.com/actions/setup-go
licence: MIT AND Apache-2.0 AND BlueOak-1.0.0 AND ISC AND 0BSD
licence-declared: MIT
decided-on: "2026-09-02"
review-by: "2027-09-02"
owner: human:paul.law
tags: [ github-actions, go ]
---

# actions/setup-go

`Tool: tol-setup-go` `APPROVED`

The action that installs Go on the runner, so the `lint` job can build actionlint.

## What we use it for

The `lint` job in `kac.yml` is the one job that runs it. The step passes `go-version: stable`, which installs the newest
stable Go on the day. The next step runs `go install` with the actionlint version the job pins.

`stable` is a moving version. [std-CI] allows one in a job with `contents: read`, and `lint` declares nothing wider.
The action's `token` input defaults to the job's `github.token`.

Every step pins the action to the commit behind `v7.0.0`, and Dependabot moves the pin. `versions` approves the major
the pin sits in.

## Where it is used

Nowhere in this catalogue. It runs in the `lint` job of `kac.yml`, and no service ships what it installs.

## Alternatives considered

None recorded.

## Licence and obligations

The repository and its `package.json` declare MIT. The action runs bundled code under `dist/`, and the `.licenses/`
folder at the pinned commit records each npm dependency in it. Most are MIT. The others are these:

| Licence       | Packages                                           |
|---------------|----------------------------------------------------|
| Apache-2.0    | `@protobuf-ts/runtime`, `@protobuf-ts/runtime-rpc` |
| BlueOak-1.0.0 | one version of `minimatch`                         |
| ISC           | the other version of `minimatch`, `semver`         |
| 0BSD          | `tslib`                                            |

The four entries marked `other` are `@actions/http-client`, `balanced-match`, `brace-expansion` and `concat-map`. Each
licence file is the MIT text.

The runner fetches the action for each job, and nothing here redistributes it, so nothing follows for anything this
repository publishes.

## Related

* [std-CI] requires every action to be pinned to a commit.
* [tol-actionlint] is built with the Go it installs.

[std-CI]: ../../standards/workflows.md
[tol-actionlint]: ../lint/actionlint.md
