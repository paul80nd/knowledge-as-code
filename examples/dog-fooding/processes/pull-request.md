---
id: prc-pull-request
type: process
tier: procedural
status: active
applies-to: [ svc-corpus-feed, svc-docs-site, svc-kac, svc-marketplace ]
last-rehearsed: "2026-09-07"
requires-access:
  - Push to paul80nd/knowledge-as-code, and the `gh` CLI signed in to it
owner: human:paul.law
tags: [ contributing, review ]
---

# Open a pull request

`Process: prc-pull-request` `ACTIVE`

Every change lands this way, so this is the last thing you do to whatever you were working on.

## When to use this

You have finished a change on a branch and committed it. A push to `main` is rejected, so a pull request is the only
route in. If you are here because a build broke, you want a [runbook](../runbooks.md) instead.

## Prerequisites

* A branch off `main`, carrying the change as commits.
* The `gh` CLI signed in to `paul80nd/knowledge-as-code`.
* `kac` run as `dotnet run --project tooling/kac --`, which is the tool this branch holds.

## Steps

1. List the pages your change made wrong, and fix them. Nothing in CI reads prose for meaning, so this pass is yours. A
   change to a command reaches `docs/` and often `tooling/README.md`. A change to what the tool is for reaches the root
   `README.md` and `PACKAGE.md`. A change to the schema reaches `.schema/README.md`, `.schema/meta/type.schema.json`,
   `docs/framework/metadata.md` and `docs/design/held-to.md`.
2. Where `kac` changed, write the changelog entry on this branch. One written after the merge reaches nobody.
3. Where `kac` changed, put the release call to the branch owner and recommend an answer. Recommend releasing where the
   change stands on its own. Recommend holding where it is one part of a group that is no use apart.
4. Run `kac update --check --from ../../` inside each corpus under `examples/` that you changed. Where you changed an
   overlay file in `template/`, or a rule in `manifest.yaml`, run it inside every one of them instead.
5. Run the test layers your change touches, one `kac` invocation at a time. Where you are unsure, run all four.
6. Write each commit message to `technical-writing`. The subject says what changed. The body says why.
7. Put a behaviour change in a commit of its own, apart from any refactor.
8. Write the pull request body to state the reason and the evidence. Name each test layer you ran and what it
   reported. Do not retell the diff.
9. Open the pull request. Say what you did not do, and why.

## Verification

Every layer you ran reports zero errors, and `gh pr view` shows the request open against `main`.

Close the session by naming the branch, what each commit carries, which layers you ran and what each one reported, and
what you left undone.

## If it goes wrong

A push to `main` is rejected rather than merged, so a mistake here costs a branch and never the trunk. A version
nuget.org has already accepted cannot be replaced, so a release that shipped wrong is followed by the next patch.

## Related

* [std-CI.a-version-moves-by-hand-and-publishes-once] carries the changelog entry, the `<Version>` move and the
  `content-version` each corpus owes. Steps 2 and 3 say where in the order those fall.
* [std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request] carries the lock and the range every
  consumer owes back, and what proves them before the merge rather than in CI.
* [std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved] says what a file two trees hold owes you. Step
  4 is where you prove it.
* [svc-kac] is what a release publishes.

[std-CI.a-version-moves-by-hand-and-publishes-once]: ../standards/workflows.md#a-version-moves-by-hand-and-publishes-once
[std-CONFIG.a-value-living-in-more-than-one-tree-is-copied-and-proved]: ../standards/configuration.md#a-value-living-in-more-than-one-tree-is-copied-and-proved
[std-VERS.a-producers-move-obliges-every-consumer-in-the-same-pull-request]: ../standards/versioning.md#a-producers-move-obliges-every-consumer-in-the-same-pull-request
[svc-kac]: ../services/kac.md
