---
id: tol-github-pages
type: tool
tier: descriptive
status: approved
packages:
  - { purl: pkg:github/actions/configure-pages, versions: 6.x }
  - { purl: pkg:github/actions/deploy-pages, versions: 5.x }
  - { purl: pkg:github/actions/upload-pages-artifact, versions: 5.x }
homepage: https://pages.github.com/
licence: NOASSERTION
licence-declared: MIT
decided-on: "2026-08-24"
review-by: "2027-08-24"
owner: human:paul.law
tags: [ deployment, github-actions, github-pages ]
---

# GitHub Pages actions

`Tool: tol-github-pages` `APPROVED`

Three actions that publish the documentation site to GitHub Pages. They were adopted together, and each is pinned on
its own.

## What we use it for

The `publish` job of `publish-docs.yml` runs them in order, after `mkdocs build`:

| Action                          | What it does here                                                                |
|---------------------------------|----------------------------------------------------------------------------------|
| `actions/configure-pages`       | Reads the site's Pages settings. `enablement` stays `false`, so it changes none. |
| `actions/upload-pages-artifact` | Archives `site/` as a tar file and uploads it as the `github-pages` artifact.    |
| `actions/deploy-pages`          | Deploys that artifact and outputs the page URL the environment shows.            |

The job declares `pages: write` and `id-token: write`, and runs in the `github-pages` environment. `deploy-pages` uses
the first to create the deployment and the second to request an OIDC token that proves where the deployment came from.
`configure-pages` and `deploy-pages` each take the job's `github.token` by default.

`upload-pages-artifact` is a composite action. Its shell steps write the archive, and its last step runs
`actions/upload-artifact`, which the `upload-pages-artifact` maintainers pin to a commit. This repository neither
chooses nor pins `actions/upload-artifact`, so it has no record of its own. When Dependabot moves the
`upload-pages-artifact` pin, the `upload-artifact` commit can move with it.

Every step pins its action to a commit, and Dependabot moves each pin on its own. Each `versions` approves the major
its pin sits in: `configure-pages` at `v6.0.0`, `upload-pages-artifact` at `v5.0.0` and `deploy-pages` at `v5.0.1`.

## Where it is used

* [svc-docs-site] is published with them.

## Alternatives considered

None recorded.

## Licence and obligations

All three repositories declare MIT. `upload-pages-artifact` bundles no code. `configure-pages` and `deploy-pages` each
run bundled code under `dist/`, and each ships a `dist/licenses.txt` listing the npm packages in it. Most are MIT. The
others are these:

| Licence       | Packages                                                                                            |
|---------------|-----------------------------------------------------------------------------------------------------|
| Apache-2.0    | `@opentelemetry/api`, `@protobuf-ts/runtime-rpc`, `b4a`, `before-after-hook`, `crc-32`, `eslint-visitor-keys`, `readdir-glob` |
| BSD-2-Clause  | `espree`, `webidl-conversions`                                                                      |
| BlueOak-1.0.0 | `path-scurry`                                                                                       |
| ISC           | `deprecation`, `glob`, `graceful-fs`, `inherits`, `lru-cache`, `minimatch`, `minipass`, `once`, `sax`, `universal-user-agent`, `wrappy` |
| 0BSD          | `tslib`                                                                                             |

Some entries need a closer read:

* `chainsaw` and `traverse` state `MIT/X11`, which is the MIT licence. `traverse` includes the MIT text.
* `dot-object` states no licence in `dist/licenses.txt`. Its npm package at 2.1.4 includes an `MIT-LICENSE` file.
* The `lodash` licence waives rights in the sample code of its documentation under CC0. That sample code is not in the
  bundle, so CC0 does not apply to it.

`licence` is `NOASSERTION`, because two packages `deploy-pages` bundles state no licence. `buffers` 0.1.1 and
`http-status-messages` 1.1.0 have none in `dist/licenses.txt`, in their npm registry metadata, or in any file of their
npm packages. Until both are known, no licence can be concluded for the whole bundle. If both turn out to be MIT or
compatible with it, the conclusion is MIT and the licences in the table above.

`actions/upload-artifact` declares MIT. Its `.licenses/` folder records MIT packages and one BlueOak-1.0.0 package,
`minimatch`, and the table above already lists BlueOak-1.0.0.

The runner fetches the actions for each job, and nothing here redistributes them. The published site contains the
built pages and no part of the actions, so nothing follows for it.

## Related

* [std-CI] requires every action to be pinned to a commit.
* [tol-mkdocs] builds the site these actions publish.

[std-CI]: ../../standards/workflows.md
[svc-docs-site]: ../../services/docs-site.md
[tol-mkdocs]: ../docs/mkdocs.md
