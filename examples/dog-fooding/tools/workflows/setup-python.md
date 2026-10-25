---
id: tol-setup-python
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/actions/setup-python, versions: 7.x }
homepage: https://github.com/actions/setup-python
licence: MIT AND Apache-2.0 AND BlueOak-1.0.0 AND ISC AND 0BSD
licence-declared: MIT
decided-on: "2026-08-24"
review-by: "2027-08-24"
owner: human:paul.law
tags: [ github-actions, python ]
---

# actions/setup-python

`Tool: tol-setup-python` `APPROVED`

The action that installs Python on the runner, for the linters and for the documentation site.

## What we use it for

Every step passes `python-version: '3.13'`. Three jobs run it:

| Job                             | What it installs next                     |
|---------------------------------|-------------------------------------------|
| `lint` in `kac.yml`             | yamllint, from `.github/requirements.txt` |
| `docs` in `kac.yml`             | Zensical, from `docs/requirements.txt`    |
| `publish` in `publish-docs.yml` | the same, before it publishes the site    |

The `docs` and `publish` jobs pass `cache: pip`. The action saves the pip cache when either job ends.

The action's `token` input defaults to the job's `github.token`. The `publish` job of `publish-docs.yml` declares
`pages: write` and `id-token: write`, so the action runs in a job with both.

Every step pins the action to the commit behind `v7.0.0`, and Dependabot moves the pin. `versions` approves the major
the pin sits in.

## Where it is used

* [svc-docs-site] is built with the Python it installs.

## Alternatives considered

None recorded.

## Licence and obligations

The repository and its `package.json` declare MIT. The action runs bundled code under `dist/`, and the `.licenses/`
folder at the pinned commit records each npm dependency in it. Most are MIT. The others are these:

| Licence       | Packages                                                           |
|---------------|--------------------------------------------------------------------|
| Apache-2.0    | `@protobuf-ts/runtime`, `@protobuf-ts/runtime-rpc`                 |
| BlueOak-1.0.0 | one version of `minimatch`                                         |
| ISC           | `@iarna/toml`, the other version of `minimatch`, `semver`          |
| 0BSD          | `tslib`                                                            |

The four entries marked `other` are `@actions/http-client`, `balanced-match`, `brace-expansion` and `concat-map`. Each
licence file is the MIT text.

The runner fetches the action for each job, and nothing here redistributes it, so nothing follows for anything this
repository publishes.

## Related

* [std-CI] requires every action to be pinned to a commit.
* [tol-yamllint] is installed on the Python it sets up.
* [tol-zensical] is installed on it too.

[std-CI]: ../../standards/workflows.md
[svc-docs-site]: ../../services/docs-site.md
[tol-yamllint]: ../lint/yamllint.md
[tol-zensical]: ../docs/zensical.md
