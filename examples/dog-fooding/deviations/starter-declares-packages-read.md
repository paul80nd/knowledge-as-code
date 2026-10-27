---
id: dev-starter-declares-packages-read
type: deviation
tier: normative
status: active
departs-from:
  - std-CI.PERMS
risk: low
accepted-on: "2026-10-27"
review-by: "2027-10-27"
closed-on:
applies-to:
  - svc-kac
owner: human:paul.law
tags: [ github-actions, permissions, starter-pipeline ]
---

# The GitHub starter declares `packages: read` before any step uses it

`Deviation: dev-starter-declares-packages-read` `ACTIVE`

The GitHub starter pipeline grants its `validate` job `packages: read` in every new corpus, including one that
consumes nothing, until `kac` can grant it only where `consumes:` has an entry.

## What we are doing instead

`template/.github/workflows/kac.yml` declares `packages: read` on its `validate` job. The restore and validate steps
pass `github.token` as `KAC_REGISTRY_TOKEN`, and that token reads a GitHub Packages feed with this permission. A corpus
`kac new` creates declares no `consumes:`, so until it does, no step in the job uses the permission.

## Why we need it

The starter restores from GitHub Packages with no edit to the pipeline, which is what
[#1214](https://github.com/paul80nd/knowledge-as-code/issues/1214) asks. Without the permission, the first corpus to
consume another fails `restore` with a 401, and its author has to find the missing line in a workflow they did not
write.

## What the risk is

Every step in the job can read every package the repository's token is allowed to read, in a corpus that reads none.
The permission is read-only, so a compromised step can read a package and cannot publish, delete or change one.

## What compensates

* The permission is `read`, and the job grants no package write.
* A comment above the key says what it is for, and that a corpus consuming nothing, or only public feeds, can delete
  it along with the two `KAC_REGISTRY_TOKEN` lines.
* The job runs only the `kac` that `.config/dotnet-tools.json` pins, and actions pinned to a commit.

## How it closes

It closes when the starter can declare the permission only for a corpus that consumes. That needs `kac new` or
`kac update` to write it when `consumes:` gains an entry, or GitHub Actions to grant a permission conditionally. At the
review date, weigh whether either exists. Where neither does, weigh whether [std-CI] should admit a starter's read-only
permission for a step it ships.

[#1219](https://github.com/paul80nd/knowledge-as-code/issues/1219) asked for this departure.

## Scope

The GitHub starter pipeline in `template/`, and every corpus `kac new` creates with `--ci github`. This repository's own
workflows are not covered.

## Related

* [std-CI] states the permission rule this departs from.
* [svc-kac] ships the starter pipeline.

[std-CI]: ../standards/workflows.md
[svc-kac]: ../services/kac.md
